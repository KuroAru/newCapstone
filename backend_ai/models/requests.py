from __future__ import annotations

from pydantic import BaseModel, Field


class ChatRequest(BaseModel):
    prompt: str = Field(..., min_length=1, max_length=2000)
    system: str = "당신은 저택의 도우미입니다."
    use_tools: bool = True
