from __future__ import annotations

import json
import logging
from typing import AsyncIterator

from groq import AsyncGroq

from models.responses import SSEEvent
from providers.base import AIProvider

logger = logging.getLogger(__name__)


class GroqProvider(AIProvider):

    def __init__(self, api_key: str, model: str = "llama-3.3-70b-versatile") -> None:
        self._client = AsyncGroq(api_key=api_key)
        self._model = model

    @property
    def name(self) -> str:
        return "groq"

    async def stream_chat(
        self,
        messages: list[dict],
        tools: list[dict] | None = None,
        temperature: float = 0.7,
        max_tokens: int = 512,
    ) -> AsyncIterator[SSEEvent]:
        kwargs: dict = {
            "model": self._model,
            "messages": messages,
            "temperature": temperature,
            "max_tokens": max_tokens,
            "stream": True,
        }
        if tools:
            kwargs["tools"] = tools
            kwargs["tool_choice"] = "auto"

        stream = await self._client.chat.completions.create(**kwargs)

        full_text_parts: list[str] = []
        tool_calls_acc: dict[int, dict] = {}

        async for chunk in stream:
            delta = chunk.choices[0].delta if chunk.choices else None
            if delta is None:
                continue

            if delta.content:
                full_text_parts.append(delta.content)
                yield SSEEvent(type="text_delta", content=delta.content)

            if delta.tool_calls:
                self._accumulate_tool_calls(delta.tool_calls, tool_calls_acc)

        for fc_event in self._emit_tool_call_events(tool_calls_acc):
            yield fc_event
        yield SSEEvent(type="done", full_text="".join(full_text_parts))

    @staticmethod
    def _accumulate_tool_calls(
        tool_calls: list, acc: dict[int, dict]
    ) -> None:
        for tc in tool_calls:
            idx = tc.index
            if idx not in acc:
                acc[idx] = {"name": "", "arguments": ""}
            if tc.function.name:
                acc[idx]["name"] += tc.function.name
            if tc.function.arguments:
                acc[idx]["arguments"] += tc.function.arguments

    @staticmethod
    def _emit_tool_call_events(acc: dict[int, dict]) -> list[SSEEvent]:
        events: list[SSEEvent] = []
        for _idx in sorted(acc):
            tc = acc[_idx]
            try:
                args = json.loads(tc["arguments"]) if tc["arguments"] else {}
            except json.JSONDecodeError:
                logger.warning("Failed to parse tool call arguments: %s", tc["arguments"])
                args = {}
            events.append(SSEEvent(type="function_call", name=tc["name"], arguments=args))
        return events
