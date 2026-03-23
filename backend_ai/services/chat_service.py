from __future__ import annotations

import logging
from typing import AsyncIterator

from models.requests import ChatRequest
from models.responses import ChatResponse, FunctionCallResult, SSEEvent
from providers.base import AIProvider
from tools.registry import ToolRegistry

logger = logging.getLogger(__name__)


class ChatService:
    """Orchestrates AI provider calls with tool injection and automatic fallback."""

    def __init__(
        self,
        primary: AIProvider,
        fallback: AIProvider | None,
        registry: ToolRegistry,
        temperature: float = 0.7,
        max_tokens: int = 512,
    ) -> None:
        self._primary = primary
        self._fallback = fallback
        self._registry = registry
        self._temperature = temperature
        self._max_tokens = max_tokens

    _TOOL_INSTRUCTION = (
        "\n\n[중요: 응답 방식] "
        "1) 반드시 캐릭터 대사를 텍스트로 먼저 말하세요. "
        "2) 텍스트와 별개로, 제공된 tool/function을 호출하여 게임 액션을 지시하세요. "
        "3) 절대로 텍스트 안에 function call 구문이나 JSON을 넣지 마세요. "
        "텍스트 응답과 tool 호출은 완전히 분리되어야 합니다."
    )

    def _build_messages(self, request: ChatRequest) -> list[dict]:
        system_content = request.system
        if request.use_tools and len(self._registry) > 0:
            system_content += self._TOOL_INSTRUCTION
        return [
            {"role": "system", "content": system_content},
            {"role": "user", "content": request.prompt},
        ]

    def _get_tools(self, request: ChatRequest) -> list[dict] | None:
        if not request.use_tools or len(self._registry) == 0:
            return None
        return self._registry.get_all_openai_format()

    async def stream_chat(self, request: ChatRequest) -> AsyncIterator[SSEEvent]:
        messages = self._build_messages(request)
        tools = self._get_tools(request)

        try:
            logger.info("Attempting primary provider: %s", self._primary.name)
            async for event in self._primary.stream_chat(
                messages=messages,
                tools=tools,
                temperature=self._temperature,
                max_tokens=self._max_tokens,
            ):
                yield event
            return
        except Exception as exc:
            logger.error("Primary provider (%s) failed: %s", self._primary.name, exc)

        if self._fallback is None:
            yield SSEEvent(type="error", content="모든 AI 엔진 실패")
            yield SSEEvent(type="done", full_text="")
            return

        try:
            logger.info("Falling back to: %s", self._fallback.name)
            async for event in self._fallback.stream_chat(
                messages=messages,
                tools=tools,
                temperature=self._temperature,
                max_tokens=self._max_tokens,
            ):
                yield event
        except Exception as exc:
            logger.error("Fallback provider (%s) also failed: %s", self._fallback.name, exc)
            yield SSEEvent(type="error", content="모든 AI 엔진 실패")
            yield SSEEvent(type="done", full_text="")

    async def chat(self, request: ChatRequest) -> ChatResponse:
        """Non-streaming chat that collects all events into a single ChatResponse."""
        full_text_parts: list[str] = []
        function_calls: list[FunctionCallResult] = []
        error_text: str | None = None

        async for event in self.stream_chat(request):
            if event.type == "text_delta" and event.content:
                full_text_parts.append(event.content)
            elif event.type == "function_call" and event.name:
                function_calls.append(
                    FunctionCallResult(name=event.name, arguments=event.arguments or {})
                )
            elif event.type == "error":
                error_text = event.content
            elif event.type == "done" and event.full_text:
                full_text_parts = [event.full_text]

        response_text = "".join(full_text_parts) if full_text_parts else (error_text or "")
        return ChatResponse(response=response_text, function_calls=function_calls)
