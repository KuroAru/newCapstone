from __future__ import annotations

from typing import AsyncIterator

import pytest

from models.requests import ChatRequest
from models.responses import SSEEvent
from providers.base import AIProvider
from services.chat_service import ChatService, _user_visible_ai_error
from tools.game_tools import GAME_TOOLS
from tools.registry import ToolRegistry


class _MockProvider(AIProvider):
    """Configurable mock provider for testing."""

    def __init__(self, provider_name: str, events: list[SSEEvent] | None = None, should_fail: bool = False):
        self._name = provider_name
        self._events = events or []
        self._should_fail = should_fail

    @property
    def name(self) -> str:
        return self._name

    async def stream_chat(self, messages, tools=None, temperature=0.7, max_tokens=512) -> AsyncIterator[SSEEvent]:
        if self._should_fail:
            raise RuntimeError(f"{self._name} failure")
        for event in self._events:
            yield event


def test_user_visible_ai_error_rate_limit() -> None:
    msg = _user_visible_ai_error(RuntimeError("Error code: 429 - rate_limit_exceeded"))
    assert "한도" in msg


def test_user_visible_ai_error_generic() -> None:
    assert _user_visible_ai_error(RuntimeError("broken")) == "모든 AI 엔진 실패"


def _build_registry() -> ToolRegistry:
    reg = ToolRegistry()
    reg.register_many(GAME_TOOLS)
    return reg


def _request(prompt: str = "테스트") -> ChatRequest:
    return ChatRequest(prompt=prompt, system="테스트 시스템", use_tools=True)


@pytest.mark.asyncio
class TestChatServiceStreaming:

    async def test_primary_success(self):
        events = [
            SSEEvent(type="text_delta", content="응답"),
            SSEEvent(type="done", full_text="응답"),
        ]
        service = ChatService(
            primary=_MockProvider("groq", events),
            fallback=_MockProvider("gemini", [SSEEvent(type="done", full_text="fallback")]),
            registry=_build_registry(),
        )

        collected = [e async for e in service.stream_chat(_request())]
        assert any(e.type == "text_delta" and e.content == "응답" for e in collected)
        assert collected[-1].type == "done"

    async def test_fallback_on_primary_failure(self):
        fallback_events = [
            SSEEvent(type="text_delta", content="폴백 응답"),
            SSEEvent(type="done", full_text="폴백 응답"),
        ]
        service = ChatService(
            primary=_MockProvider("groq", should_fail=True),
            fallback=_MockProvider("gemini", fallback_events),
            registry=_build_registry(),
        )

        collected = [e async for e in service.stream_chat(_request())]
        assert any(e.content == "폴백 응답" for e in collected if e.type == "text_delta")

    async def test_all_providers_fail_yields_error(self):
        service = ChatService(
            primary=_MockProvider("groq", should_fail=True),
            fallback=_MockProvider("gemini", should_fail=True),
            registry=_build_registry(),
        )

        collected = [e async for e in service.stream_chat(_request())]
        error_events = [e for e in collected if e.type == "error"]
        assert len(error_events) == 1
        assert "실패" in error_events[0].content

    async def test_no_fallback_yields_error(self):
        service = ChatService(
            primary=_MockProvider("groq", should_fail=True),
            fallback=None,
            registry=_build_registry(),
        )

        collected = [e async for e in service.stream_chat(_request())]
        assert any(e.type == "error" for e in collected)


@pytest.mark.asyncio
class TestChatServiceNonStreaming:

    async def test_chat_collects_text_and_function_calls(self):
        events = [
            SSEEvent(type="text_delta", content="켁켁!"),
            SSEEvent(type="function_call", name="give_hint", arguments={"hint_level": "moderate", "target_object": "bed", "hint_category": "location"}),
            SSEEvent(type="function_call", name="emote", arguments={"emotion": "mock"}),
            SSEEvent(type="done", full_text="켁켁!"),
        ]
        service = ChatService(
            primary=_MockProvider("groq", events),
            fallback=None,
            registry=_build_registry(),
        )

        result = await service.chat(_request())
        assert result.response == "켁켁!"
        assert len(result.function_calls) == 2
        assert result.function_calls[0].name == "give_hint"
        assert result.function_calls[1].name == "emote"

    async def test_chat_returns_error_text_on_failure(self):
        service = ChatService(
            primary=_MockProvider("groq", should_fail=True),
            fallback=None,
            registry=_build_registry(),
        )

        result = await service.chat(_request())
        assert "실패" in result.response

    async def test_tools_disabled_passes_none(self):
        events = [
            SSEEvent(type="text_delta", content="답변"),
            SSEEvent(type="done", full_text="답변"),
        ]
        service = ChatService(
            primary=_MockProvider("groq", events),
            fallback=None,
            registry=_build_registry(),
        )

        request = ChatRequest(prompt="test", use_tools=False)
        result = await service.chat(request)
        assert result.response == "답변"
        assert len(result.function_calls) == 0
