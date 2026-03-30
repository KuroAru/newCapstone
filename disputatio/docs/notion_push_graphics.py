# -*- coding: utf-8 -*-
"""Emit JSON args for Notion MCP update_content (stdout). Run from repo root."""
import json
from pathlib import Path

HERE = Path(__file__).resolve().parent
SECTION = (HERE / "graphics-gdd-section8-notion.md").read_text(encoding="utf-8")
OLD = (
    '<callout icon="📋">\n'
    "\t본 기획서는 2026-03-28 기준으로 업데이트되었습니다. 구현 현황 및 로드맵은 실제 코드베이스 분석 결과를 반영합니다.\n"
    "</callout>"
)
payload = {
    "page_id": "32cea40d2678817b9f32fc52f944c472",
    "command": "update_content",
    "properties": {},
    "content_updates": [{"old_str": OLD, "new_str": SECTION + "\n" + OLD}],
}
print(json.dumps(payload, ensure_ascii=False))
