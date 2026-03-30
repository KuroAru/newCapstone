"""One-off generator for Notion graphics section markdown. Run from repo root or any cwd."""
from __future__ import annotations

import os
import re
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / "Assets" / "Sprite"
OUT = Path(os.environ.get("TEMP", ".")) / "notion_graphics_section.notion.md"


def purpose_for(rel: str) -> str:
    base = rel.split("/")[-1]
    b = base.lower()
    parts: list[str] = []
    if "_bg" in b or b.endswith("bg.png") or "bg.png" in b:
        parts.append("배경")
    if "example" in b or "예시" in base:
        parts.append("레이아웃 참고/예시")
    if "door" in b or "문" in base:
        parts.append("문/출입")
    if "window" in b or "창" in base:
        parts.append("창문")
    if "light" in b or "flashlight" in b or "불" in base:
        parts.append("조명/손전등 상태")
    if "open" in b and "close" not in b:
        parts.append("열림 상태")
    if "close" in b or "closed" in b:
        parts.append("닫힘 상태")
    if "lock" in b or "자물쇠" in base:
        parts.append("잠금/열쇠")
    if "jerky" in b:
        parts.append("육포 조리 연출")
    if "smoke" in b:
        parts.append("연기 이펙트")
    if "parrot" in b or "앵무" in base:
        parts.append("체셔(앵무) 연출")
    if "desk" in b or "책상" in base:
        parts.append("책상")
    if "chair" in b:
        parts.append("의자")
    if "bed" in b or "침대" in base:
        parts.append("침대")
    if "bookshelf" in b or "book" in b:
        parts.append("책장/책")
    if "drawer" in b or "서랍" in base or "설랍" in base:
        parts.append("서랍")
    if "frame" in b or "액자" in base:
        parts.append("액자")
    if "showcase" in b or "case" in b:
        parts.append("진열장")
    if "whiteboard" in b or "칠판" in base:
        parts.append("칠판")
    if "curtain" in b or "커튼" in base:
        parts.append("커튼")
    if "nest" in b:
        parts.append("둥지(앵무)")
    if "skeleton" in b:
        parts.append("해골")
    if "letter" in b:
        parts.append("편지")
    if "fence" in b:
        parts.append("울타리/창살")
    if "locker" in b:
        parts.append("사물함")
    if "key" in b and "monkey" not in b:
        parts.append("열쇠")
    if "chainsaw" in b:
        parts.append("인트로 전기톱 프레임")
    if rel.lower().startswith("saveimage"):
        parts.append("세이브 슬롯 썸네일")
    if rel.lower().startswith("ui/"):
        parts.append("UI")
    if "charactor" in rel.lower():
        parts.append("캐릭터/참고")
    if "말/" in rel or rel.endswith("Sprite-0006.png"):
        parts.append("아이 방 — 네 기사 목마(요한계시록)")
    if "funiture" in rel.lower():
        parts.append("가구")
    if rel.lower().startswith("mention"):
        parts.append("오프닝 멘션/저택 외관 레이어")
    if "office" in rel.lower() and "new_office" in b:
        parts.append("탐정 사무실")
    if rel.lower().startswith("uti_room"):
        parts.append("유틸(세탁실)")
    if rel.lower().startswith("lab/"):
        parts.append("실험실 배경")
    if "jail" in rel.lower() or "감옥" in base:
        parts.append("감옥")
    if "wiferoom" in b or rel.lower().startswith("wiferoom"):
        parts.append("아내 방")
    if "2f_study" in b or rel.lower().startswith("tutorroom"):
        parts.append("가정교사 방(공부방)")
    if rel.lower().startswith("studyroom"):
        parts.append("서재")
    if rel.lower().startswith("made_room"):
        parts.append("가정부 방")
    if rel.lower().startswith("kitchen"):
        parts.append("주방")
    first = rel.split("/")[0].lower()
    if first == "hall" or first == "hallway":
        parts.append("1층 홀/복도")
    if rel.startswith("2floor"):
        parts.append("2층 홀/복도")
    if rel.lower().startswith("intro"):
        parts.append("인트로")
    if rel.lower().startswith("item"):
        parts.append("아이템 아이콘")
    if base.startswith("Sprite-") or re.match(r"^[0-9a-f]{8,}\.png$", b):
        return "용도 확인 필요(제네릭 파일명)"
    if not parts:
        return "씬 오브젝트/프롭(파일명 기준 추정)"
    return " · ".join(dict.fromkeys(parts))


def esc(s: str) -> str:
    out: list[str] = []
    for c in s:
        if c in r"\*~`$[]<>{}|^":
            out.append("\\" + c)
        else:
            out.append(c)
    return "".join(out)


def main() -> None:
    metas = sorted(ROOT.rglob("*.png.meta"))
    rows = [str(m.relative_to(ROOT)).replace("\\", "/").replace(".meta", "") for m in metas]

    g: dict[str, list[str]] = defaultdict(list)
    for rel in rows:
        seg = rel.split("/")[0]
        g[seg].append(rel)

    lines: list[str] = []
    lines.append("## 8. 그래픽 기획 (2D 스프라이트 카탈로그)")
    lines.append(
        f"본 섹션은 저장소 `Assets/Sprite` 기준(PNG **{len(rows)}**개, 2026-03-30 스캔)으로 "
        "2D 그래픽 자원을 공간·기능 단위로 정리한다. 썸네일은 노션에 별도 업로드하거나, "
        "아래 **상대 경로**로 로컬 에셋을 연결한다."
    )
    lines.append("### 8-1. 개요")
    lines.append(
        "- **파이프라인**: 폴더명 ≈ 씬/공간(홀, 복도, 방, UI 등). "
        "파일명에 상태 접미사(`_open` / `_closed`, `Light_On` / `Off` 등)가 붙은 경우 인게임 스프라이트 스왑용으로 추정."
    )
    lines.append("- **범위**: `disputatio/Assets/Sprite` 이하만 포함(셰이더 팩·`mokotan/EffectSprite` 등은 제외).")
    lines.append("### 8-2. 네이밍 패턴")
    lines.append("- `*_BG`, `*BG.png`: 배경 레이어")
    lines.append("- `Kitchen_*`, `MaidRoom_*`, `2f_studyroom_*`, `2f_wiferoom_*`, `jail_*`: 방·기능 접두사")
    lines.append("- `*_inside_*`: 감옥 등 중첩 공간(안의 안) 레이어")
    lines.append("- `*extend*`, `*_extend_*`: 확대/상세 뷰용 변형")
    lines.append("### 8-3. Mokotan 빌드 씬 ↔ 스프라이트 폴더(교차 참조)")
    lines.append('<table fit-page-width="true" header-row="true">')
    lines.append("<tr><td>씬(unity 경로 요약)</td><td>관련 Sprite 폴더</td><td>비고</td></tr>")
    xref = [
        ("Mokotan/Opening_Office.unity", "Office, office.png", "사무실 오프닝"),
        ("Mokotan/Opening_Mention*.unity", "Mention/", "멘션/저택 외관"),
        ("First Floor/Hall_*.unity", "Hall/", "현관 홀"),
        ("First Floor/…/Hall_Left*.unity", "LeftHallway/", "1층 좌측 복도"),
        ("First Floor/…/Hall_Right*.unity", "RightHallway/, RightHallwayBack/", "1층 우측 복도"),
        ("First Floor/Kitchen.unity", "kitchen/", "주방"),
        ("First Floor/UtilityRoom.unity", "Uti_room/", "유틸"),
        ("First Floor/…/StudyRoom*.unity", "StudyRoom/", "서재"),
        ("First Floor/MaidRoom.unity", "made_room/", "가정부 방"),
        ("First Floor/…/Prison*.unity", "Jail/", "감옥"),
        ("Second Floor/2floorMainHall.unity", "2floorHall/", "2층 메인홀"),
        ("Second Floor/2floorHallway_*.unity", "2floorLeftHallway/, 2floorRightHallway/", "2층 복도"),
        ("Second Floor/… (Tutor / 공부방)", "TutorRoom/", "가정교사 방"),
        ("Second Floor/… (Wife)", "WifeRoom/", "아내 방"),
        ("Second Floor/… (Son / Child)", "ChildSP/", "아이 방"),
        ("IntroScene 등", "Intro/", "인트로"),
        ("MainMenu 등", "SaveImage/, UI/, Main_Title_o.png", "메뉴·세이브 썸네일"),
    ]
    for a, b, c in xref:
        lines.append(f"<tr><td>{esc(a)}</td><td>{esc(b)}</td><td>{esc(c)}</td></tr>")
    lines.append("</table>")
    lines.append("### 8-4. 폴더별 에셋 카탈로그")
    lines.append("각 토글 안에 **상대 경로**(`Assets/Sprite/…`), **용도 추정**, **상태/비고** 표를 둔다.")

    order = sorted(g.keys(), key=lambda x: (x.lower().endswith(".png"), x.lower()))
    for folder in order:
        rels = sorted(g[folder])
        title = folder if not folder.endswith(".png") else f"(루트) {folder}"
        lines.append("<details>")
        lines.append(f"<summary>**{esc(title)}** ({len(rels)}개)</summary>")
        lines.append('\t<table fit-page-width="true" header-row="true">')
        lines.append("\t<tr><td>상대 경로</td><td>용도 추정</td><td>상태/비고</td></tr>")
        for rel in rels:
            ap = "Assets/Sprite/" + rel
            bn = rel.split("/")[-1].lower()
            st = ""
            if (
                "_open" in bn
                or "_close" in bn
                or "light_on" in bn
                or "light_off" in bn
                or bn.endswith("_on.png")
                or bn.endswith("_off.png")
            ):
                st = "상태 분기 스프라이트"
            elif "extend" in bn:
                st = "확대/디테일 변형"
            lines.append(
                f"\t<tr><td>`{esc(ap)}`</td><td>{esc(purpose_for(rel))}</td><td>{esc(st or '—')}</td></tr>"
            )
        lines.append("\t</table>")
        lines.append("</details>")

    lines.append('<callout icon="📋">')
    lines.append(
        "\t스프라이트 목록은 리포지토리 `Assets/Sprite`와 동기화할 때 이 섹션을 갱신한다. "
        "애매한 파일명은 **용도 확인 필요**로 표기했다."
    )
    lines.append("</callout>")

    text = "\n".join(lines)
    OUT.write_text(text, encoding="utf-8")
    print(OUT)
    print("chars", len(text))


if __name__ == "__main__":
    main()
