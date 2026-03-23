from __future__ import annotations

import logging
import os

from fastapi import FastAPI, HTTPException
from fastapi.responses import StreamingResponse

from config import get_settings
from models.requests import ChatRequest
from models.responses import ChatResponse
from providers.groq_provider import GroqProvider
from providers.gemini_provider import GeminiProvider
from services.chat_service import ChatService
from tools.game_tools import GAME_TOOLS
from tools.registry import ToolRegistry

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI(title="Disputatio AI Backend")

# ---------------------------------------------------------------------------
# Bootstrap
# ---------------------------------------------------------------------------
settings = get_settings()

registry = ToolRegistry()
registry.register_many(GAME_TOOLS)

primary = GroqProvider(api_key=settings.groq_api_key, model=settings.default_model_groq) if settings.groq_api_key else None
fallback = GeminiProvider(api_key=settings.google_api_key, model=settings.default_model_gemini) if settings.google_api_key else None

if primary is None and fallback is None:
    logger.critical("No AI provider API keys configured – server will reject all /chat requests")

_first_available = primary or fallback
_second_available = fallback if primary else None

chat_service: ChatService | None = (
    ChatService(
        primary=_first_available,
        fallback=_second_available,
        registry=registry,
        temperature=settings.default_temperature,
        max_tokens=settings.max_tokens,
    )
    if _first_available
    else None
)


# ---------------------------------------------------------------------------
# Endpoints
# ---------------------------------------------------------------------------
@app.get("/")
def health_check():
    return {"status": "online", "message": "Server is Running!"}


@app.post("/chat", response_model=ChatResponse)
async def chat(request: ChatRequest):
    """Backward-compatible endpoint: returns full response + function_calls at once."""
    if chat_service is None:
        raise HTTPException(status_code=500, detail="API 키 설정 필요")

    result = await chat_service.chat(request)

    if not result.response and not result.function_calls:
        raise HTTPException(status_code=500, detail="모든 AI 엔진 실패")

    return result


@app.post("/chat/stream")
async def chat_stream(request: ChatRequest):
    """SSE streaming endpoint – tokens arrive in real-time."""
    if chat_service is None:
        raise HTTPException(status_code=500, detail="API 키 설정 필요")

    async def event_generator():
        async for event in chat_service.stream_chat(request):
            yield f"data: {event.model_dump_json()}\n\n"

    return StreamingResponse(
        event_generator(),
        media_type="text/event-stream",
        headers={
            "Cache-Control": "no-cache",
            "X-Accel-Buffering": "no",
            "Connection": "keep-alive",
        },
    )


if __name__ == "__main__":
    import uvicorn

    port = int(os.environ.get("PORT", 8000))
    uvicorn.run(app, host="0.0.0.0", port=port)
