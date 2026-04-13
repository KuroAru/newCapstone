from __future__ import annotations

import pytest

from models.requests import ChatRequest


def test_prompt_only() -> None:
    r = ChatRequest(prompt="hello", system="s")
    assert r.prompt == "hello"


def test_message_only_fills_prompt() -> None:
    r = ChatRequest(message="from message", system="s")
    assert r.prompt == "from message"


def test_user_id_optional_and_ignored_in_core_model() -> None:
    r = ChatRequest(prompt="a", system="s", user_id="u-1")
    assert r.user_id == "u-1"
    assert r.prompt == "a"


def test_extra_fields_ignored() -> None:
    r = ChatRequest.model_validate(
        {"prompt": "x", "system": "s", "some_legacy": 1},
    )
    assert r.prompt == "x"


def test_empty_prompt_and_message_raises() -> None:
    with pytest.raises(Exception):
        ChatRequest.model_validate({"system": "s", "prompt": "", "message": ""})
