from __future__ import annotations

import logging
from typing import AsyncIterator

import google.generativeai as genai

from models.responses import SSEEvent
from providers.base import AIProvider

logger = logging.getLogger(__name__)


def _openai_tools_to_gemini(tools: list[dict]) -> list[genai.protos.Tool] | None:
    """Convert OpenAI-format tool definitions to Gemini FunctionDeclaration format."""
    if not tools:
        return None

    declarations: list[genai.protos.FunctionDeclaration] = []
    for tool in tools:
        func = tool.get("function", {})
        declarations.append(
            genai.protos.FunctionDeclaration(
                name=func.get("name", ""),
                description=func.get("description", ""),
                parameters=_convert_params(func.get("parameters", {})),
            )
        )
    return [genai.protos.Tool(function_declarations=declarations)]


_TYPE_MAP = {
    "string": genai.protos.Type.STRING,
    "integer": genai.protos.Type.INTEGER,
    "number": genai.protos.Type.NUMBER,
    "boolean": genai.protos.Type.BOOLEAN,
    "object": genai.protos.Type.OBJECT,
    "array": genai.protos.Type.ARRAY,
}


def _convert_params(schema: dict) -> genai.protos.Schema | None:
    if not schema:
        return None

    schema_type = _TYPE_MAP.get(schema.get("type", ""), genai.protos.Type.STRING)
    properties = {}
    for prop_name, prop_schema in schema.get("properties", {}).items():
        prop_type = _TYPE_MAP.get(prop_schema.get("type", "string"), genai.protos.Type.STRING)
        prop_kwargs: dict = {"type_": prop_type}
        if "description" in prop_schema:
            prop_kwargs["description"] = prop_schema["description"]
        if "enum" in prop_schema:
            prop_kwargs["enum"] = prop_schema["enum"]
        properties[prop_name] = genai.protos.Schema(**prop_kwargs)

    return genai.protos.Schema(
        type_=schema_type,
        properties=properties,
        required=schema.get("required", []),
    )


def _messages_to_gemini_contents(messages: list[dict]) -> tuple[str | None, list[dict]]:
    """Split messages into system instruction + Gemini contents."""
    system_instruction: str | None = None
    contents: list[dict] = []
    for msg in messages:
        role = msg.get("role", "user")
        text = msg.get("content", "")
        if role == "system":
            system_instruction = text
        else:
            gemini_role = "model" if role == "assistant" else "user"
            contents.append({"role": gemini_role, "parts": [text]})
    return system_instruction, contents


class GeminiProvider(AIProvider):

    def __init__(self, api_key: str, model: str = "gemini-2.0-flash") -> None:
        genai.configure(api_key=api_key)
        self._model_name = model
        self._api_key = api_key

    @property
    def name(self) -> str:
        return "gemini"

    async def stream_chat(
        self,
        messages: list[dict],
        tools: list[dict] | None = None,
        temperature: float = 0.7,
        max_tokens: int = 512,
    ) -> AsyncIterator[SSEEvent]:
        system_instruction, contents = _messages_to_gemini_contents(messages)

        model = genai.GenerativeModel(
            model_name=self._model_name,
            system_instruction=system_instruction,
            tools=_openai_tools_to_gemini(tools) if tools else None,
            generation_config=genai.GenerationConfig(
                temperature=temperature,
                max_output_tokens=max_tokens,
            ),
        )

        response = await model.generate_content_async(
            contents=contents,
            stream=True,
        )

        full_text_parts: list[str] = []

        async for chunk in response:
            for part in chunk.parts:
                if part.text:
                    full_text_parts.append(part.text)
                    yield SSEEvent(type="text_delta", content=part.text)

                if part.function_call and part.function_call.name:
                    args = dict(part.function_call.args) if part.function_call.args else {}
                    yield SSEEvent(
                        type="function_call",
                        name=part.function_call.name,
                        arguments=args,
                    )

        yield SSEEvent(type="done", full_text="".join(full_text_parts))
