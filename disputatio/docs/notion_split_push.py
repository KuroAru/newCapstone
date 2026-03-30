# -*- coding: utf-8 -*-
"""Split graphics GDD at </details> boundaries for safe Notion MCP payloads."""
import json
import re
from pathlib import Path

HERE = Path(__file__).resolve().parent
MD = (HERE / "graphics-gdd-section8-notion.md").read_text(encoding="utf-8")
MARKER = "\n\n[[[GDD_GRAPHICS_SPLIT]]]\n\n"
CALLOUT = (
    '<callout icon="📋">\n'
    "\t본 기획서는 2026-03-28 기준으로 업데이트되었습니다. 구현 현황 및 로드맵은 실제 코드베이스 분석 결과를 반영합니다.\n"
    "</callout>"
)
OLD_TEST = "## 8. 그래픽 기획 (임시 앵커 테스트)\n"

DETAIL_END = re.compile(r"</details>\n")


def chunk_by_details(text: str, max_body: int) -> list[str]:
    if len(text) <= max_body:
        return [text]
    chunks: list[str] = []
    start = 0
    while start < len(text):
        if len(text) - start <= max_body:
            chunks.append(text[start:])
            break
        limit = start + max_body
        region = text[start:limit]
        m = None
        for match in DETAIL_END.finditer(region):
            m = match
        if m is None:
            raise SystemExit(f"No </details> boundary within {max_body} chars from offset {start}")
        end_abs = start + m.end()
        chunks.append(text[start:end_abs])
        start = end_abs
    return chunks


first_details = MD.find("<details>")
if first_details < 0:
    raise SystemExit("No <details> in markdown")
prefix = MD[:first_details]
detail_tail = MD[first_details:]
# Keep each JSON payload small (Notion MCP)
detail_chunks = chunk_by_details(detail_tail, max_body=7000)
all_chunks = [prefix + detail_chunks[0]] + detail_chunks[1:]

payloads: list[dict] = []
first_old = OLD_TEST
for i, ch in enumerate(all_chunks):
    is_last = i == len(all_chunks) - 1
    if i == 0:
        new_s = ch + (MARKER + CALLOUT if not is_last else "\n\n" + CALLOUT)
    elif not is_last:
        new_s = ch + MARKER + CALLOUT
    else:
        new_s = ch + "\n\n" + CALLOUT
    payloads.append(
        {
            "page_id": "32cea40d2678817b9f32fc52f944c472",
            "command": "update_content",
            "properties": {},
            "content_updates": [{"old_str": first_old, "new_str": new_s}],
        }
    )
    first_old = MARKER + CALLOUT

for idx, p in enumerate(payloads, start=1):
    (HERE / f"notion_payload_part{idx}.json").write_text(
        json.dumps(p, ensure_ascii=False), encoding="utf-8"
    )
    print(f"part{idx}", len(json.dumps(p, ensure_ascii=False)))
print("num_parts", len(payloads))
