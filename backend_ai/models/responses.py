from __future__ import annotations

from typing import Any, Literal

from pydantic import BaseModel


class FunctionCallResult(BaseModel):
    name: str
    arguments: dict[str, Any]


class ChatResponse(BaseModel):
    response: str
    function_calls: list[FunctionCallResult] = []


class SSEEvent(BaseModel):
    type: Literal["text_delta", "function_call", "error", "done"]
    content: str | None = None
    name: str | None = None
    arguments: dict[str, Any] | None = None
    full_text: str | None = None
