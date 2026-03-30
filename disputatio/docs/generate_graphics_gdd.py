# One-off generator: disputatio/docs/graphics-sprite-gdd.md from sprite-png-inventory.txt
from __future__ import annotations

import re
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parent
INV_PATH = ROOT / "sprite-png-inventory.txt"
OUT_PATH = ROOT / "graphics-sprite-gdd.md"


def load_lines() -> list[str]:
    text = INV_PATH.read_text(encoding="utf-8-sig")
    return [ln.strip() for ln in text.splitlines() if ln.strip()]


def infer(path: str) -> tuple[str, str]:
    lower = path.lower()
    name = path.split("/")[-1].lower()
    is_2f = "2floor" in lower or "/2f_" in lower
    is_1f_hall = lower.startswith("hall/") and not is_2f

    bits: list[str] = []
    state = "단일"

    if "예시" in path or "example" in name:
        bits.append("레이아웃·합성 참고용 목업")
        state = "목업(참고)"

    if "_bg" in name or name.endswith("bg.png") or path.endswith("_BG.png"):
        bits.append("배경")
        state = "배경 레이어"

    if is_2f and "hallway" in lower:
        bits = ["2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭"]
        if "_bg" in name or "bg.png" in name:
            state = "배경 레이어"
        elif "예시" in path:
            state = "목업(참고)"
        else:
            state = "전경 오브젝트(진열장·액자 등)"
        if "frame" in name or "액자" in path:
            bits.append("벽 액자(성화·성모 등 테마 구분)")
        if "case" in name or "showcase" in name:
            bits.append("유리 진열장")
        if "_불" in path or "light" in name:
            bits.append("조명 연출 변형")
        return " · ".join(dict.fromkeys(bits)), state

    if is_2f and "2floorhall" in lower.replace("/", ""):
        bits = ["2층 메인 홀"]
        if "jesus" in name:
            bits.append("중앙 성화/장식")
        if "leftdoor" in name:
            bits.append("좌측 출입문")
        elif "rightdoor" in name:
            bits.append("우측 출입문")
        elif "door" in name:
            bits.append("출입문")
        if "예시" in path:
            state = "목업(참고)"
        elif "_bg" in name or path.endswith("BG.png"):
            state = "배경 레이어"
        else:
            state = "전경"
        return " · ".join(dict.fromkeys(bits)), state

    if is_1f_hall:
        bits = ["1층 저택 현관(홀)"]
        if "parrot_talk" in name:
            bits.append("앵무 대화 연출 전용 프롭")
        if "flashlight" in name:
            bits.append("손전등 조명")
            state = "On/Off" if "_on" in name or "_off" in name else state
        if "door" in name and "flashlight" in name:
            state = "문 + 손전등 On/Off"
        if "floor_light" in name:
            bits.append("바닥 조명")
            state = "켜짐" if "on" in name else state
        if "hallway_door" in name:
            bits.append("복도 쪽 문 조명")
            state = "켜짐"
        if "저택현관" in path:
            bits.append("전경 구도 참고")
        if "door.png" == name:
            bits.append("문(기본)")
        return " · ".join(dict.fromkeys(bits)), state

    if lower.startswith("left hallway/") or lower.startswith("lefthallway/"):
        bits = ["1층 좌측 복도(Hall_Left 계열)"]
        if "light" in name:
            bits.append("조명 On/Off 분기")
            state = "조명 On/Off"
        if "door" in name:
            bits.append("문 조명")
        return " · ".join(dict.fromkeys(bits)), state

    if lower.startswith("righthallwayback/"):
        bits = ["1층 우측 복도 후면(뒤를 본 시점)"]
        if "x2" in name:
            bits.append("2배 확대")
        if "frame" in name:
            bits.append("액자")
        if "showcase" in name:
            bits.append("진열장")
        st = "목업(참고)" if "예시" in path else "전경 레이어"
        return " · ".join(dict.fromkeys(bits)), st

    if lower.startswith("righthallway/"):
        bits = ["1층 우측 복도 정면"]
        if "x2" in name:
            bits.append("2배 확대 연출용 레이어")
        if "frame" in name:
            bits.append("액자")
        if "showcase" in name:
            bits.append("진열장")
        if "예시" in path:
            state = "목업(참고)"
        else:
            state = "단일"
        return " · ".join(dict.fromkeys(bits)), state

    if lower.startswith("childsp/"):
        if "childbg" in name:
            return "아이 방 전체 배경", "배경 레이어"
        if "/말/" in lower or lower.startswith("childsp/말/"):
            if "sprite-0006" in name:
                return "4기사 목마 일러스트(4번째)", "확인 필요(임시 파일명)"
            return "4기사 목마(정복·전쟁·역병 등) 테마 컷", "단일"
        if "funiture" in lower:
            return "아이 방 가구·소품(상자·침대·탁자·서랍 등)", "단일/확대 뷰(파일명 참고)"

    if lower.startswith("kitchen/"):
        b = "주방 프롭(싱크·냉장고·버너·문·쓰레기통 등)"
        if "jerky" in name:
            return "육포 조리 연출", "조리 전/후" if "no_cooked" in name else "조리 후"
        if "hose" in name:
            return "싱크 호스", "펼침" if "open" in name else "수납"
        if "smoke" in name:
            return "가열·연기 이펙트", "단일"
        if "주방예시" in path:
            return "주방 합성 목업", "목업(참고)"
        return b, "단일"

    if lower.startswith("made_room/"):
        b = "메이드룸(가정부실) 가구·책·수납"
        if "drawer" in name:
            return "서랍", "열림" if "open" in name else ("닫힘" if "closed" in name else "기본")
        if "keyshelf" in name or "keyshelf" in lower:
            return "열쇠 선반", "열쇠 없음(확장)" if "nokey" in name else "단일"
        if "가정부실" in path:
            return "메이드룸 목업", "목업(참고)"
        return b, "단일"

    if lower.startswith("tutorroom/"):
        b = "2층 과외방(공부방) 창·책상·칠판·커튼·둥지 등"
        if "whiteboard_extend" in name:
            return "칠판 확대(상호작용/확대 뷰)", "확장"
        if "공부방예시" in path:
            return "과외방 목업", "목업(참고)"
        return b, "단일"

    if lower.startswith("jail/"):
        b = "감옥(전경·내부·내부의 내부 계층)"
        if "fullbg" in name:
            return "감옥 외부/전체 배경", "배경 레이어"
        if "inside_inside" in name:
            return "감옥 내부 속 상세(침대·편지·열쇠)", "단일"
        if "inside_bg" in name:
            return "감옥 내부 배경", "배경 레이어"
        if "locker" in name:
            return "사물함", "열쇠 포함" if "key" in name else "단일/변형"
        if "감옥" in path and "예시" in path:
            return "감옥 목업", "목업(참고)"
        return b, "단일"

    if lower.startswith("studyroom/"):
        b = "서재(1층) 책장·일기·카드·전면/우측 뷰"
        if "lock" in name or "open" in name:
            if "diary" in name:
                return "일기장", "잠금/오픈" if "lock" in name or "open" in name else "단일"
            if "book" in name:
                return "책(서고 퍼즐)", "잠금/오픈 분기"
        if "hidden" in name:
            return "숨겨진 칸/책", "오픈 분기 가능"
        if "entrance" in name:
            return "서재 입구", "문 포함" if "door" in name else "단일"
        if "서재" in path and "예시" in path:
            return "서재 목업", "목업(참고)"
        return b, "단일"

    if lower.startswith("wiferoom/"):
        return "2층 부인 방(안방) 창·화장대·서랍·액자·시계·문", "문 열림" if "door_open" in name else "단일"

    if lower.startswith("uti_room/"):
        b = "다용도실(세탁실) 세탁기·전기 패널·조명"
        if "flashlight" in name or "light" in name:
            state = "손전등/조명 On" if "on" in name else "단일"
        if "switch" in name:
            return "패널 스위치", "On/Off"
        if "panel_inside" in name:
            return "패널 내부 클로즈업", "변형(파일명 번호)"
        return b, state

    if lower.startswith("office/"):
        if "calling" in name:
            return "사무실 — 전화 통화 중 연출", "통화 중"
        if "nothing" in name:
            return "사무실 — 통화 없음", "대기"
        return "사무실 목업", "목업(참고)"

    if lower.startswith("intro/"):
        if "chainsaw" in name:
            m = re.search(r"_(\d+)\.png$", name)
            n = m.group(1) if m else "?"
            return "오프닝 체인소 연출(프레임 시퀀스)", f"프레임 {n}/4"
        if "first_bg" in name:
            return "인트로 배경", "배경 레이어"
        return "인트로 목업", "목업(참고)"

    if lower.startswith("mention/"):
        if "affter" in lower or "mansion_" in name:
            return "오프닝 멘션 — 저택 외경 레이어(문·울타리·달·별·그림자 등)", "잠금/오픈 분기" if "lock" in name or "open" in name else "단일"
        if re.match(r"^[0-9a-f]{8,}\.png$", name) or "gemini" in name:
            return "멘션용 임시 이미지", "확인 필요"
        return "오프닝 멘션 UI/소품", "단일"

    if lower.startswith("item/"):
        return "획득·사용 아이템(열쇠·물병 등)", "단일"

    if lower.startswith("puzzle/"):
        return "단어/필터 카드 퍼즐 UI", "단일"

    if lower.startswith("ui/"):
        return "공통 UI(대화창·방향 버튼·메인 버튼)", "단일"

    if lower.startswith("lab/"):
        return "실험실(지하 Lab) 배경", "배경 레이어"

    if lower.startswith("charactor/"):
        return "플레이어/앵무 등 캐릭터 스프라이트", "단일"

    if lower.startswith("hallway/tworoot") or name == "tworoot.png":
        return "복도 분기(두 갈래) 시각", "단일"

    if lower.startswith("saveimage/"):
        return "세이브 데이터 슬롯 썸네일(장소별)", "단일"

    if path == "Main_Title_o.png":
        return "메인 메뉴/타이틀 화면", "단일"

    if path == "office.png":
        return "오프닝 사무실 관련 단일 에셋(루트)", "단일"

    # default
    return "씬 프롭(폴더·파일명 기준 추정)", state


SECTION_INTRO: dict[str, str] = {
    "__root__": (
        "`Assets/Sprite` 바로 아래에 두 파일만 둔 특수 케이스입니다. "
        "빌드 씬과의 매핑은 아래 교차 참조와 실제 프리팹 참조를 함께 확인하세요."
    ),
    "2floorHall": (
        "2층 중앙 메인 홀(`2floorMainHall` 등)용입니다. 배경·양쪽 문·중앙 장식(성화)으로 층 전체의 축을 잡습니다."
    ),
    "2floorLeftHallway": (
        "2층 좌측 복도(`2floorHallway_Left`, `2floorLeft` 등)입니다. 정면/후면 시점과 `_불` 접미 목업으로 조명 연출을 구분합니다."
    ),
    "2floorRightHallway": (
        "2층 우측 복도입니다. 성화·성모 등 액자 네이밍으로 좌우 벽 연출을 구분합니다."
    ),
    "Charactor": "주인공·앵무 등 캐릭터 2D 리소스입니다.",
    "ChildSP": (
        "아이 방(`ChildRoom`)입니다. `ChildBG` 위에 가구 레이어를 얹고, `말/` 폴더는 4기사 테마 목마 일러스트입니다."
    ),
    "Hall": (
        "1층 현관 홀(`Hall_playerble`, `Hall_animate`)입니다. 손전등·바닥 조명·앵무 대화 전용 BG/스탠드가 묶여 있습니다."
    ),
    "Hallway": "1층 복도 공통 — 분기 연출용 `TwoRoot` 한 장이 있습니다.",
    "Intro": "`IntroScene` 계열 오프닝. 체인소 프레임 시퀀스와 배경이 쌍을 이룹니다.",
    "Item": "인벤토리에 올라가는 소형 아이템 스프라이트입니다.",
    "Jail": (
        "`Prison`, `PrisonEntrance` 등 감옥 씬. `jail_inside_inside_*` 계층 네이밍은 ‘감옥 안의 초점 오브젝트’를 뜻합니다."
    ),
    "kitchen": "`Kitchen` 씬. 싱크 호스 open/close, 육포 조리 전/후, 연기 등 상태 분기가 명확합니다.",
    "Lab": "`Basement` 등 지하 실험실 배경입니다.",
    "LeftHallway": (
        "1층 좌측 복도(`Hallway_Left`, `Hall_Left` 등). 조명 On/Off가 씬 분위기 전환에 쓰입니다."
    ),
    "made_room": "`MaidRoom` — 서랍 열림/닫힘, 열쇠 선반(열쇠 유무) 등 퍼즐 연출과 직결됩니다.",
    "Mention": (
        "`Opening_Mention` — 저택 외경을 레이어로 쪼개어 문·울타리·하늘을 합성합니다. 해시/생성기 파일명은 정식 명명으로 갈아타는 것을 권장합니다."
    ),
    "Office": "`Opening_Office` — 통화 중/없음 두 상태로 나뉩니다.",
    "Puzzle": "서고 등 퍼즐에서 쓰는 카드 UI입니다.",
    "RightHallway": "1층 우측 복도 정면·2배 확대 뷰입니다.",
    "RightHallwayBack": "1층 우측 복도 후면(뒤집힌 시점) 레이어입니다.",
    "SaveImage": "세이브 슬롯에 표시할 장소별 썸네일입니다.",
    "StudyRoom": (
        "`StudyRoom`, `BookCase*`, `StudyEntrance` 등 서재 일련. 책·일기 잠금/오픈, 숨은 칸 등 상태 분기가 많습니다."
    ),
    "TutorRoom": "`TutorRoom` — 2층 과외방. 칠판 확장(`whiteboard_extend`)은 클로즈업/상호작용용입니다.",
    "UI": "대화·이동 버튼 등 전역 UI입니다.",
    "Uti_room": "`UtilityRoom` — 세탁기·전기 패널·스위치 On/Off.",
    "WifeRoom": "`WifeRoom`/`BedRoom` 계열 — 안방 가구·창·문 열림 등.",
}


def main() -> None:
    lines = load_lines()
    by_top: dict[str, list[str]] = defaultdict(list)
    for line in lines:
        top = line.split("/")[0] if "/" in line else "__root__"
        by_top[top].append(line)

    order = sorted(by_top.keys(), key=lambda x: (x != "__root__", x.lower()))

    parts: list[str] = []
    parts.append("# 그래픽 기획 — `Assets/Sprite` 2D 카탈로그\n")
    parts.append(
        "본 문서는 `disputatio/Assets/Sprite` 이하 PNG(메타 기준 **253개**)에 대해 "
        "폴더(공간·기능) 단위로 **용도 추정**과 **상태·분기**를 정리한 기획 참고용 자료입니다. "
        "워크스페이스에 PNG 바이너리가 없을 수 있으므로, 시각 확인은 로컬 에디터/Unity에서 경로를 열어 검증하세요.\n"
    )
    parts.append("## 1. 개요\n")
    parts.append(
        "- **구성 원칙**: 상위 폴더 = 씬/공간 또는 시스템(UI, 아이템, 오프닝) 단위.\n"
        "- **네이밍**: 접미사 `_BG`(배경), `_open`/`_closed`, `Light_On`/`Off`, `_extend`(확대), "
        "`jail_inside_inside_*`(공간 안의 포커스 오브젝트) 등으로 레이어·상태를 구분합니다.\n"
        "- **목업(`*예시*.png`)**: 최종 인게임이 아니라 합성·타이밍 참고용일 수 있으므로 빌드 연결 여부는 씬에서 확인합니다.\n"
    )
    parts.append("## 2. 빌드 씬(Mokotan) ↔ 스프라이트 폴더 교차 참조\n")
    parts.append(
        "| 씬(요약) | 연관 `Assets/Sprite` 폴더 |\n"
        "|-----------|---------------------------|\n"
        "| `Opening_Office` | `Office/`, 루트 `office.png` |\n"
        "| `Opening_Mention`, `Opening_Mention _open` | `Mention/` |\n"
        "| `Hall_animate`, `Hall_playerble` | `Hall/` |\n"
        "| `Hall_Left`, `Hall_Left2`, `Hallway_Left`, `Hallway_Left2` | `LeftHallway/` |\n"
        "| `Hall_Right`, `Hall_Right2`, `Hall_RightCross`, `Hallway_Right`, `Hallway_Right2` | `RightHallway/`, `RightHallwayBack/` |\n"
        "| `Kitchen` | `kitchen/` |\n"
        "| `UtilityRoom` | `Uti_room/` |\n"
        "| `MaidRoom`, `MaidEntrance` | `made_room/` |\n"
        "| `StudyRoom`, `StudyEntrance`, `BookCase*`, `BookCase2Back` | `StudyRoom/` |\n"
        "| `PrisonEntrance`, `Prison`, `GoPrisonAnimation` | `Jail/` |\n"
        "| `2floorMainHall` | `2floorHall/` |\n"
        "| `2floorLeft`, `2floorLeftCross`, `2floorHallway_Left` | `2floorLeftHallway/` |\n"
        "| `2floorRight`, `2floorRightCross`, `2floorHallway_Right` | `2floorRightHallway/` |\n"
        "| `TutorRoom`, `TutorEntrance` | `TutorRoom/` |\n"
        "| `ChildRoom`, `ChildEntrance` | `ChildSP/` |\n"
        "| `BedRoom`, `BedEntrance`, `WifeRoom`, `WifeEntrance`, `DressingRoom` | `WifeRoom/` |\n"
        "| `Basement` | `Lab/` |\n"
        "| `IntroScene`(공통 인트로) | `Intro/` |\n"
        "| (공통) | `UI/`, `Item/`, `Puzzle/`, `Charactor/`, `SaveImage/` |\n"
    )
    parts.append("## 3. 공간·기능별 카탈로그\n")

    for top in order:
        intro = SECTION_INTRO.get(top, "해당 폴더는 파일명 패턴으로 용도를 추정합니다.")
        title = "Sprite 루트" if top == "__root__" else top
        parts.append(f"### 3.{order.index(top) + 1} `{title}`")
        parts.append(intro)
        parts.append("")
        parts.append("| 상대 경로 | 용도(추정) | 상태·비고 |")
        parts.append("|-----------|------------|-----------|")
        for path in sorted(by_top[top], key=str.lower):
            pur, st = infer(path)
            ap = f"Assets/Sprite/{path}"
            parts.append(f"| `{ap}` | {pur} | {st} |")
        parts.append("")

    parts.append(
        "---\n*자동 생성: `disputatio/docs/generate_graphics_gdd.py` — 인벤토리: `sprite-png-inventory.txt`*\n"
    )
    OUT_PATH.write_text("\n".join(parts), encoding="utf-8")


if __name__ == "__main__":
    main()
