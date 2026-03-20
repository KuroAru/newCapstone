from __future__ import annotations

from tools.game_tools import GAME_TOOLS, GIVE_HINT_TOOL, UPDATE_QUIZ_TOOL, EMOTE_TOOL
from tools.registry import ToolRegistry


class TestToolRegistry:

    def test_register_single_tool(self):
        reg = ToolRegistry()
        reg.register(GIVE_HINT_TOOL)
        assert len(reg) == 1
        assert "give_hint" in reg.tool_names

    def test_register_many(self):
        reg = ToolRegistry()
        reg.register_many(GAME_TOOLS)
        assert len(reg) == 3
        assert set(reg.tool_names) == {"give_hint", "update_quiz", "emote"}

    def test_get_tool_returns_copy(self):
        reg = ToolRegistry()
        reg.register(EMOTE_TOOL)
        tool = reg.get_tool("emote")
        assert tool is not None
        assert tool["function"]["name"] == "emote"
        tool["function"]["name"] = "modified"
        assert reg.get_tool("emote")["function"]["name"] == "emote"

    def test_get_tool_missing_returns_none(self):
        reg = ToolRegistry()
        assert reg.get_tool("nonexistent") is None

    def test_openai_format_output_shape(self):
        reg = ToolRegistry()
        reg.register_many(GAME_TOOLS)
        tools = reg.get_all_openai_format()
        assert len(tools) == 3
        for t in tools:
            assert t["type"] == "function"
            assert "name" in t["function"]
            assert "parameters" in t["function"]


class TestGameToolDefinitions:

    def test_give_hint_required_params(self):
        params = GIVE_HINT_TOOL["function"]["parameters"]
        assert set(params["required"]) == {"hint_level", "target_object", "hint_category"}

    def test_update_quiz_required_params(self):
        params = UPDATE_QUIZ_TOOL["function"]["parameters"]
        assert set(params["required"]) == {"is_correct", "quiz_complete"}

    def test_emote_required_params(self):
        params = EMOTE_TOOL["function"]["parameters"]
        assert params["required"] == ["emotion"]

    def test_emote_enum_values(self):
        emotion_prop = EMOTE_TOOL["function"]["parameters"]["properties"]["emotion"]
        assert set(emotion_prop["enum"]) == {"mock", "laugh", "angry", "surprised", "helpful", "sad"}

    def test_hint_level_enum_values(self):
        prop = GIVE_HINT_TOOL["function"]["parameters"]["properties"]["hint_level"]
        assert set(prop["enum"]) == {"subtle", "moderate", "direct"}
