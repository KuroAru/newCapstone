# -*- coding: utf-8 -*-
"""Generate Notion-flavored markdown for section 8 (graphics GDD) from sprite inventory."""
from __future__ import annotations

import re
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parent
INV = ROOT / "sprite-png-inventory.txt"
OUT = ROOT / "graphics-gdd-section8-notion.md"


def purpose_for(name: str) -> str:
    n = name.lower()
    if "예시" in name or "example" in n:
        return "기획/합성 참고용 썸네일"
    if "_bg" in n or name.endswith("BG.png") or n.endswith("_bg.png"):
        return "배경 레이어"
    if "open" in n and "door" in n:
        return "문/개폐 연출 (열림)"
    if "_open" in n or "open.png" in n:
        return "상태: 열림/펼침"
    if "_closed" in n or "close" in n:
        return "상태: 닫힘"
    if "light_on" in n or "_on.png" in n and "light" in n:
        return "조명 켜짐"
    if "light_off" in n or ("_off.png" in n and "light" in n):
        return "조명 꺼짐"
    if "flashlight" in n:
        return "손전등 연출"
    if "smoke" in n:
        return "이펙트 (연기)"
    if "key" in n and "locker" not in n:
        return "열쇠/키 오브젝트"
    if "frame" in n or "액자" in name:
        return "액자/프레임 오브젝트"
    if "door" in n:
        return "문 오브젝트"
    if "window" in n or "창" in name:
        return "창문"
    if "desk" in n or "책상" in name:
        return "책상"
    if "bed" in n or "침대" in name:
        return "침대"
    if "bookshelf" in n or "책장" in name:
        return "책장"
    if "nest" in n:
        return "앵무새 둥지"
    if "whiteboard" in n:
        return "칠판"
    if "curtain" in n:
        return "커튼"
    if "chair" in n:
        return "의자"
    if "drawer" in n or "서랍" in name or "설랍" in name:
        return "서랍/수납"
    if "locker" in n:
        return "사물함"
    if "skeleton" in n:
        return "해골 (연출)"
    if "letter" in n:
        return "편지"
    if "fullbg" in n:
        return "전체 배경 합성"
    if "fence" in n:
        return "울타리/철창"
    if "showcase" in n:
        return "진열장"
    if "hallway" in n or "복도" in name:
        return "복도 구성 요소"
    if "saveimage" in n or name.startswith(("1.", "2.", "3.", "4.", "5.", "6.", "7.", "8.", "9.", "10.")):
        return "세이브 슬롯 썸네일"
    if "ui/" in n or name in ("button.png", "dialogue.png", "main_button.png"):
        return "UI 공통"
    if "arrow" in n:
        return "UI 방향 화살표"
    if "item/" in n:
        return "아이콘/인벤토리용 아이템"
    if "charactor" in n or "parrot" in n:
        return "캐릭터/앵무 스프라이트"
    if "말" in name or "정복" in name or "전쟁" in name or "역병" in name:
        return "네 기사/목마 퍼즐 조각"
    if "sprite-0006" in n:
        return "목마/기사 (용도 확인 필요)"
    if "gemini" in n or "generated" in n:
        return "생성 이미지 (멘션 씬 등)"
    if "pan" in n or "fry" in n or "burner" in n or "fridge" in n or "sink" in n or "trash" in n:
        return "주방 조리/설비 오브젝트"
    if "jerky" in n:
        return "육포 조리 상태"
    if "hose" in n:
        return "싱크 호스 상태"
    if "washing" in n or "laundry" in n or "panel" in n or "uti_" in n:
        return "다용도실(세탁/전기 패널)"
    if "clock" in n:
        return "시계"
    if "dressing" in n:
        return "화장대"
    if "diary" in n:
        return "일기장"
    if "card" in n and "library" in n:
        return "서재 카드"
    if "book" in n and "library" in n:
        return "서재 책/책장 변형"
    if "entrance" in n:
        return "입구 연출"
    if "jesus" in n or "marry" in n or "mary" in n:
        return "복도 종교 도상/액자"
    return "씬 구성 스프라이트"


def top_folder(rel: str) -> str:
    # Assets/Sprite/X/... or Assets/Sprite/file.png
    parts = rel.replace("\\", "/").split("/")
    if len(parts) >= 4:
        return parts[2]
    return "_SpriteRoot"


FOLDER_KO = {
    "2floorHall": "2층 메인 홀",
    "2floorLeftHallway": "2층 왼쪽 복도",
    "2floorRightHallway": "2층 오른쪽 복도",
    "Charactor": "캐릭터",
    "ChildSP": "아이 방 (ChildRoom)",
    "Hall": "1층 홀",
    "Hallway": "복도 (공통)",
    "Intro": "인트로/오프닝",
    "Item": "아이템 아이콘",
    "Jail": "감옥 (Prison)",
    "kitchen": "주방",
    "Lab": "실험실/연구 (Lab)",
    "LeftHallway": "1층 왼쪽 복도",
    "made_room": "가정부 방 (MaidRoom)",
    "Mention": "멘션/자막 씬",
    "Office": "사무실 (오프닝)",
    "RightHallway": "1층 오른쪽 복도",
    "RightHallwayBack": "1층 오른쪽 복도(뒤)",
    "SaveImage": "세이브 슬롯 썸네일",
    "StudyRoom": "서재 (StudyRoom)",
    "TutorRoom": "가정교사 방",
    "UI": "UI 공통",
    "Uti_room": "유틸리티 룸",
    "WifeRoom": "아내의 방",
    "_SpriteRoot": "Sprite 폴더 직하위(루트)",
}


def escape_cell(s: str) -> str:
    return s.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")


def main() -> None:
    lines = [
        ln.strip().lstrip("\ufeff")
        for ln in INV.read_text(encoding="utf-8").splitlines()
        if ln.strip()
    ]
    by_folder: dict[str, list[str]] = defaultdict(list)
    for rel in lines:
        by_folder[top_folder(rel)].append(rel)

    scene_map_rows = [
        ("Mokotan 씬(빌드)", "주요 스프라이트 폴더"),
        ("Opening_Office, Office 관련", "Office/"),
        ("Opening_Mention, Mention", "Mention/"),
        ("Hall_playerble, CentralHall류", "Hall/, Hallway/"),
        ("Kitchen", "kitchen/"),
        ("UtilityRoom", "Uti_room/"),
        ("Hall_Left / Hallway_Left*", "LeftHallway/"),
        ("Hall_Right / Hallway_Right*", "RightHallway/, RightHallwayBack/"),
        ("MaidRoom", "made_room/"),
        ("StudyRoom, BookCase*", "StudyRoom/"),
        ("Prison, PrisonEntrance", "Jail/"),
        ("2floorMainHall", "2floorHall/"),
        ("2floorHallway_Left, 2floorLeft*", "2floorLeftHallway/"),
        ("2floorHallway_Right, 2floorRight*", "2floorRightHallway/"),
        ("TutorRoom", "TutorRoom/"),
        ("ChildRoom", "ChildSP/"),
        ("WifeRoom, DressingRoom", "WifeRoom/"),
        ("BedRoom", "WifeRoom/ 또는 안방 전용 에셋 혼용 시 확인"),
        ("IntroScene", "Intro/"),
        ("메인 메뉴·UI", "UI/, SaveImage/"),
        ("플레이어/앵무", "Charactor/"),
    ]

    parts: list[str] = []
    parts.append("## 8. 그래픽 기획 (2D 스프라이트)")
    parts.append("### 8-1. 개요")
    parts.append(
        "- Unity 프로젝트 **`Assets/Sprite`** 이하 PNG **253종**(2026-03 기준, `.meta` 기준 집계)은 공간·기능 단위 하위 폴더로 정리되어 있다."
    )
    parts.append(
        "- 네이밍: **`_BG`** 배경, **`_open` / `_closed` / `close`** 상태 분기, **`Light_On` / `Off`** 조명, **`Kitchen_`**, **`2f_`**, **`MaidRoom_`**, **`jail_`** 등 접두로 씬 배치 시 식별이 쉽다."
    )
    parts.append(
        "- 본 절은 **레포 내 상대 경로**를 기준으로 기획·아트·씬 배치 담당이 동일 용어로 소통하도록 한다. (노션 이미지 업로드 없이 경로만으로 추적 가능)"
    )
    parts.append("### 8-2. 씬·공간과 스프라이트 폴더 대응 (요약)")
    parts.append('<table header-row="true" fit-page-width="true">')
    for a, b in scene_map_rows:
        parts.append("<tr>")
        parts.append(f"<td>{escape_cell(a)}</td>")
        parts.append(f"<td>{escape_cell(b)}</td>")
        parts.append("</tr>")
    parts.append("</table>")
    parts.append("### 8-3. 폴더별 스프라이트 카탈로그")
    parts.append(
        "각 항목 **용도** 열은 파일명·폴더 관례에 따른 **기획 추정**이며, 실씬 배치와 다를 경우 인게임 기준으로 수정한다."
    )

    for folder in sorted(by_folder.keys(), key=lambda x: (x.lower(), x)):
        label = FOLDER_KO.get(folder, folder)
        rels = sorted(by_folder[folder])
        parts.append(f'<details>\n<summary><strong>{escape_cell(folder)}</strong> — {escape_cell(label)} ({len(rels)}개)</summary>')
        parts.append('<table header-row="true" fit-page-width="true">')
        parts.append("<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>")
        for rel in rels:
            name = rel.rsplit("/", 1)[-1]
            pur = purpose_for(name)
            parts.append("<tr>")
            parts.append(f"<td>`{rel}`</td>")
            parts.append(f"<td>{escape_cell(pur)}</td>")
            parts.append("</tr>")
        parts.append("</table>\n</details>")

    parts.append("### 8-4. 전체 경로 목록 (원본)")
    parts.append(
        f"- 레포 파일: `disputatio/docs/sprite-png-inventory.txt` ({len(lines)}줄, 정렬됨)"
    )

    OUT.write_text("\n".join(parts) + "\n", encoding="utf-8")
    print(f"Wrote {OUT} ({len(OUT.read_text(encoding='utf-8'))} chars)")


if __name__ == "__main__":
    main()
