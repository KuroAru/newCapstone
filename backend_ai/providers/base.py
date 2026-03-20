from __future__ import annotations

from abc import ABC, abstractmethod
from typing import AsyncIterator

from models.responses import SSEEvent


class AIProvider(ABC):
    """Abstract base for AI providers (DIP: high-level modules depend on this interface)."""

    @property
    @abstractmethod
    def name(self) -> str:
        ...

    @abstractmethod
    async def stream_chat(
        self,
        messages: list[dict],
        tools: list[dict] | None = None,
        temperature: float = 0.7,
        max_tokens: int = 512,
    ) -> AsyncIterator[SSEEvent]:
        """Yield SSE events as the AI generates a response."""
        ...
        yield  # pragma: no cover  –  makes this a valid AsyncIterator stub
