# 그래픽 기획 — `Assets/Sprite` 2D 카탈로그

본 문서는 `disputatio/Assets/Sprite` 이하 PNG(메타 기준 **253개**)에 대해 폴더(공간·기능) 단위로 **용도 추정**과 **상태·분기**를 정리한 기획 참고용 자료입니다. 워크스페이스에 PNG 바이너리가 없을 수 있으므로, 시각 확인은 로컬 에디터/Unity에서 경로를 열어 검증하세요.

## 1. 개요

- **구성 원칙**: 상위 폴더 = 씬/공간 또는 시스템(UI, 아이템, 오프닝) 단위.
- **네이밍**: 접미사 `_BG`(배경), `_open`/`_closed`, `Light_On`/`Off`, `_extend`(확대), `jail_inside_inside_*`(공간 안의 포커스 오브젝트) 등으로 레이어·상태를 구분합니다.
- **목업(`*예시*.png`)**: 최종 인게임이 아니라 합성·타이밍 참고용일 수 있으므로 빌드 연결 여부는 씬에서 확인합니다.

## 2. 빌드 씬(Mokotan) ↔ 스프라이트 폴더 교차 참조

| 씬(요약) | 연관 `Assets/Sprite` 폴더 |
|-----------|---------------------------|
| `Opening_Office` | `Office/`, 루트 `office.png` |
| `Opening_Mention`, `Opening_Mention _open` | `Mention/` |
| `Hall_animate`, `Hall_playerble` | `Hall/` |
| `Hall_Left`, `Hall_Left2`, `Hallway_Left`, `Hallway_Left2` | `LeftHallway/` |
| `Hall_Right`, `Hall_Right2`, `Hall_RightCross`, `Hallway_Right`, `Hallway_Right2` | `RightHallway/`, `RightHallwayBack/` |
| `Kitchen` | `kitchen/` |
| `UtilityRoom` | `Uti_room/` |
| `MaidRoom`, `MaidEntrance` | `made_room/` |
| `StudyRoom`, `StudyEntrance`, `BookCase*`, `BookCase2Back` | `StudyRoom/` |
| `PrisonEntrance`, `Prison`, `GoPrisonAnimation` | `Jail/` |
| `2floorMainHall` | `2floorHall/` |
| `2floorLeft`, `2floorLeftCross`, `2floorHallway_Left` | `2floorLeftHallway/` |
| `2floorRight`, `2floorRightCross`, `2floorHallway_Right` | `2floorRightHallway/` |
| `TutorRoom`, `TutorEntrance` | `TutorRoom/` |
| `ChildRoom`, `ChildEntrance` | `ChildSP/` |
| `BedRoom`, `BedEntrance`, `WifeRoom`, `WifeEntrance`, `DressingRoom` | `WifeRoom/` |
| `Basement` | `Lab/` |
| `IntroScene`(공통 인트로) | `Intro/` |
| (공통) | `UI/`, `Item/`, `Puzzle/`, `Charactor/`, `SaveImage/` |

## 3. 공간·기능별 카탈로그

### 3.1 `Sprite 루트`
`Assets/Sprite` 바로 아래에 두 파일만 둔 특수 케이스입니다. 빌드 씬과의 매핑은 아래 교차 참조와 실제 프리팹 참조를 함께 확인하세요.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/Main_Title_o.png` | 메인 메뉴/타이틀 화면 | 단일 |
| `Assets/Sprite/office.png` | 오프닝 사무실 관련 단일 에셋(루트) | 단일 |

### 3.2 `2floorHall`
2층 중앙 메인 홀(`2floorMainHall` 등)용입니다. 배경·양쪽 문·중앙 장식(성화)으로 층 전체의 축을 잡습니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/2floorHall/2f_mainhall_BG.png` | 2층 메인 홀 | 배경 레이어 |
| `Assets/Sprite/2floorHall/2f_mainhall_jesus.png` | 2층 메인 홀 · 중앙 성화/장식 | 전경 |
| `Assets/Sprite/2floorHall/2f_mainhall_leftdoor.png` | 2층 메인 홀 · 좌측 출입문 | 전경 |
| `Assets/Sprite/2floorHall/2f_mainhall_rightdoor.png` | 2층 메인 홀 · 우측 출입문 | 전경 |
| `Assets/Sprite/2floorHall/2층메인홀예시.png` | 2층 메인 홀 | 목업(참고) |

### 3.3 `2floorLeftHallway`
2층 좌측 복도(`2floorHallway_Left`, `2floorLeft` 등)입니다. 정면/후면 시점과 `_불` 접미 목업으로 조명 연출을 구분합니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/2floorLeftHallway/2f_hallway_left_back_BG.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 | 배경 레이어 |
| `Assets/Sprite/2floorLeftHallway/2f_hallway_left_back_case_big.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 · 유리 진열장 | 전경 오브젝트(진열장·액자 등) |
| `Assets/Sprite/2floorLeftHallway/2f_hallway_left_back_case_middle.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 · 유리 진열장 | 전경 오브젝트(진열장·액자 등) |
| `Assets/Sprite/2floorLeftHallway/2f_hallway_left_back_case_small.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 · 유리 진열장 | 전경 오브젝트(진열장·액자 등) |
| `Assets/Sprite/2floorLeftHallway/2f_hallway_left_back_frame.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 · 벽 액자(성화·성모 등 테마 구분) | 전경 오브젝트(진열장·액자 등) |
| `Assets/Sprite/2floorLeftHallway/2f_hallway_left_BG.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 | 배경 레이어 |
| `Assets/Sprite/2floorLeftHallway/2f_hallway_left_case_big.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 · 유리 진열장 | 전경 오브젝트(진열장·액자 등) |
| `Assets/Sprite/2floorLeftHallway/2f_hallway_left_case_middle.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 · 유리 진열장 | 전경 오브젝트(진열장·액자 등) |
| `Assets/Sprite/2floorLeftHallway/2f_hallway_left_case_small.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 · 유리 진열장 | 전경 오브젝트(진열장·액자 등) |
| `Assets/Sprite/2floorLeftHallway/2f_hallway_left_frame.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 · 벽 액자(성화·성모 등 테마 구분) | 전경 오브젝트(진열장·액자 등) |
| `Assets/Sprite/2floorLeftHallway/2층왼쪽복도 예시.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 | 목업(참고) |
| `Assets/Sprite/2floorLeftHallway/2층왼쪽복도 예시_불.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 · 조명 연출 변형 | 목업(참고) |
| `Assets/Sprite/2floorLeftHallway/2층왼쪽복도역방향예시.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 | 목업(참고) |
| `Assets/Sprite/2floorLeftHallway/2층왼쪽복도역방향예시_불.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 · 조명 연출 변형 | 목업(참고) |

### 3.4 `2floorRightHallway`
2층 우측 복도입니다. 성화·성모 등 액자 네이밍으로 좌우 벽 연출을 구분합니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/2floorRightHallway/2f_hallway_right_back_BG.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 | 배경 레이어 |
| `Assets/Sprite/2floorRightHallway/2f_hallway_right_back_x2_frame_jesus.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 · 벽 액자(성화·성모 등 테마 구분) | 전경 오브젝트(진열장·액자 등) |
| `Assets/Sprite/2floorRightHallway/2f_hallway_right_back_x2_frame_marry.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 · 벽 액자(성화·성모 등 테마 구분) | 전경 오브젝트(진열장·액자 등) |
| `Assets/Sprite/2floorRightHallway/2f_hallway_right_BG.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 | 배경 레이어 |
| `Assets/Sprite/2floorRightHallway/2f_hallway_right_frame_jesus.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 · 벽 액자(성화·성모 등 테마 구분) | 전경 오브젝트(진열장·액자 등) |
| `Assets/Sprite/2floorRightHallway/2f_hallway_right_frame_marry.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 · 벽 액자(성화·성모 등 테마 구분) | 전경 오브젝트(진열장·액자 등) |
| `Assets/Sprite/2floorRightHallway/2층오른쪽복도뒤로예시.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 | 목업(참고) |
| `Assets/Sprite/2floorRightHallway/2층오른쪽복도예시.png` | 2층 복도(좌/우 및 후면·2배 확대 뷰) 프롭 | 목업(참고) |

### 3.5 `Charactor`
주인공·앵무 등 캐릭터 2D 리소스입니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/Charactor/CharEX1.png` | 플레이어/앵무 등 캐릭터 스프라이트 | 단일 |
| `Assets/Sprite/Charactor/MainCharEX1-1.png` | 플레이어/앵무 등 캐릭터 스프라이트 | 단일 |
| `Assets/Sprite/Charactor/MainCharEX1.png` | 플레이어/앵무 등 캐릭터 스프라이트 | 단일 |
| `Assets/Sprite/Charactor/parrot.png` | 플레이어/앵무 등 캐릭터 스프라이트 | 단일 |

### 3.6 `ChildSP`
아이 방(`ChildRoom`)입니다. `ChildBG` 위에 가구 레이어를 얹고, `말/` 폴더는 4기사 테마 목마 일러스트입니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/ChildSP/ChildBG.png` | 아이 방 전체 배경 | 배경 레이어 |
| `Assets/Sprite/ChildSP/Funiture/상자.png` | 아이 방 가구·소품(상자·침대·탁자·서랍 등) | 단일/확대 뷰(파일명 참고) |
| `Assets/Sprite/ChildSP/Funiture/상자_수정.png` | 아이 방 가구·소품(상자·침대·탁자·서랍 등) | 단일/확대 뷰(파일명 참고) |
| `Assets/Sprite/ChildSP/Funiture/서랍_확대_수정3.png` | 아이 방 가구·소품(상자·침대·탁자·서랍 등) | 단일/확대 뷰(파일명 참고) |
| `Assets/Sprite/ChildSP/Funiture/설랍.png` | 아이 방 가구·소품(상자·침대·탁자·서랍 등) | 단일/확대 뷰(파일명 참고) |
| `Assets/Sprite/ChildSP/Funiture/침대.png` | 아이 방 가구·소품(상자·침대·탁자·서랍 등) | 단일/확대 뷰(파일명 참고) |
| `Assets/Sprite/ChildSP/Funiture/탁자_수정1.png` | 아이 방 가구·소품(상자·침대·탁자·서랍 등) | 단일/확대 뷰(파일명 참고) |
| `Assets/Sprite/ChildSP/Funiture/협탁 확대 1.png` | 아이 방 가구·소품(상자·침대·탁자·서랍 등) | 단일/확대 뷰(파일명 참고) |
| `Assets/Sprite/ChildSP/말/Sprite-0006.png` | 4기사 목마 일러스트(4번째) | 확인 필요(임시 파일명) |
| `Assets/Sprite/ChildSP/말/역병.png` | 4기사 목마(정복·전쟁·역병 등) 테마 컷 | 단일 |
| `Assets/Sprite/ChildSP/말/전쟁.png` | 4기사 목마(정복·전쟁·역병 등) 테마 컷 | 단일 |
| `Assets/Sprite/ChildSP/말/정복.png` | 4기사 목마(정복·전쟁·역병 등) 테마 컷 | 단일 |

### 3.7 `Hall`
1층 현관 홀(`Hall_playerble`, `Hall_animate`)입니다. 손전등·바닥 조명·앵무 대화 전용 BG/스탠드가 묶여 있습니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/Hall/door.png` | 1층 저택 현관(홀) · 문(기본) | 단일 |
| `Assets/Sprite/Hall/Floor_door_Flashlight_Off.png` | 1층 저택 현관(홀) · 손전등 조명 | 문 + 손전등 On/Off |
| `Assets/Sprite/Hall/Floor_door_Flashlight_On.png` | 1층 저택 현관(홀) · 손전등 조명 | 문 + 손전등 On/Off |
| `Assets/Sprite/Hall/Floor_Flashlight_off.png` | 1층 저택 현관(홀) · 손전등 조명 | On/Off |
| `Assets/Sprite/Hall/Floor_Flashlight_On.png` | 1층 저택 현관(홀) · 손전등 조명 | On/Off |
| `Assets/Sprite/Hall/Floor_light_on.png` | 1층 저택 현관(홀) · 바닥 조명 | 켜짐 |
| `Assets/Sprite/Hall/Floor_parrot_talk_BG.png` | 1층 저택 현관(홀) · 앵무 대화 연출 전용 프롭 | 배경 레이어 |
| `Assets/Sprite/Hall/Floor_parrot_talk_sheet_big.png` | 1층 저택 현관(홀) · 앵무 대화 연출 전용 프롭 | 단일 |
| `Assets/Sprite/Hall/Floor_parrot_talk_stand.png` | 1층 저택 현관(홀) · 앵무 대화 연출 전용 프롭 | 단일 |
| `Assets/Sprite/Hall/Hallway_door_Light_On.png` | 1층 저택 현관(홀) · 복도 쪽 문 조명 | 켜짐 |
| `Assets/Sprite/Hall/저택현관.png` | 1층 저택 현관(홀) · 전경 구도 참고 | 단일 |

### 3.8 `Hallway`
1층 복도 공통 — 분기 연출용 `TwoRoot` 한 장이 있습니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/Hallway/TwoRoot.png` | 복도 분기(두 갈래) 시각 | 단일 |

### 3.9 `Intro`
`IntroScene` 계열 오프닝. 체인소 프레임 시퀀스와 배경이 쌍을 이룹니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/Intro/first_BG.png` | 인트로 배경 | 배경 레이어 |
| `Assets/Sprite/Intro/first_chainsaw_1.png` | 오프닝 체인소 연출(프레임 시퀀스) | 프레임 1/4 |
| `Assets/Sprite/Intro/first_chainsaw_2.png` | 오프닝 체인소 연출(프레임 시퀀스) | 프레임 2/4 |
| `Assets/Sprite/Intro/first_chainsaw_3.png` | 오프닝 체인소 연출(프레임 시퀀스) | 프레임 3/4 |
| `Assets/Sprite/Intro/first_chainsaw_4.png` | 오프닝 체인소 연출(프레임 시퀀스) | 프레임 4/4 |
| `Assets/Sprite/Intro/첫장면예시.png` | 인트로 목업 | 목업(참고) |

### 3.10 `Item`
인벤토리에 올라가는 소형 아이템 스프라이트입니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/Item/normal_key.png` | 획득·사용 아이템(열쇠·물병 등) | 단일 |
| `Assets/Sprite/Item/water_bottle.png` | 획득·사용 아이템(열쇠·물병 등) | 단일 |

### 3.11 `Jail`
`Prison`, `PrisonEntrance` 등 감옥 씬. `jail_inside_inside_*` 계층 네이밍은 ‘감옥 안의 초점 오브젝트’를 뜻합니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/Jail/jail_fullBG.png` | 감옥 외부/전체 배경 | 배경 레이어 |
| `Assets/Sprite/Jail/jail_inside_BG.png` | 감옥 내부 배경 | 배경 레이어 |
| `Assets/Sprite/Jail/jail_inside_desk.png` | 감옥(전경·내부·내부의 내부 계층) | 단일 |
| `Assets/Sprite/Jail/jail_inside_fence.png` | 감옥(전경·내부·내부의 내부 계층) | 단일 |
| `Assets/Sprite/Jail/jail_inside_inside_bed.png` | 감옥 내부 속 상세(침대·편지·열쇠) | 단일 |
| `Assets/Sprite/Jail/jail_inside_inside_key.png` | 감옥 내부 속 상세(침대·편지·열쇠) | 단일 |
| `Assets/Sprite/Jail/jail_inside_inside_letter.png` | 감옥 내부 속 상세(침대·편지·열쇠) | 단일 |
| `Assets/Sprite/Jail/jail_inside_skeleton.png` | 감옥(전경·내부·내부의 내부 계층) | 단일 |
| `Assets/Sprite/Jail/jail_locker 1.png` | 사물함 | 단일/변형 |
| `Assets/Sprite/Jail/jail_locker.png` | 사물함 | 단일/변형 |
| `Assets/Sprite/Jail/jail_locker_key.png` | 사물함 | 열쇠 포함 |
| `Assets/Sprite/Jail/감옥안쪽예시.png` | 감옥 목업 | 목업(참고) |
| `Assets/Sprite/Jail/감옥예시.png` | 감옥 목업 | 목업(참고) |
| `Assets/Sprite/Jail/감옥자물쇠예시.png` | 감옥 목업 | 목업(참고) |

### 3.12 `kitchen`
`Kitchen` 씬. 싱크 호스 open/close, 육포 조리 전/후, 연기 등 상태 분기가 명확합니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/kitchen/kitchen_BG.png` | 주방 프롭(싱크·냉장고·버너·문·쓰레기통 등) | 단일 |
| `Assets/Sprite/kitchen/Kitchen_Burner.png` | 주방 프롭(싱크·냉장고·버너·문·쓰레기통 등) | 단일 |
| `Assets/Sprite/kitchen/Kitchen_burner_extend.png` | 주방 프롭(싱크·냉장고·버너·문·쓰레기통 등) | 단일 |
| `Assets/Sprite/kitchen/Kitchen_fridge.png` | 주방 프롭(싱크·냉장고·버너·문·쓰레기통 등) | 단일 |
| `Assets/Sprite/kitchen/Kitchen_frypan.png` | 주방 프롭(싱크·냉장고·버너·문·쓰레기통 등) | 단일 |
| `Assets/Sprite/kitchen/Kitchen_jerky_cooked.png` | 육포 조리 연출 | 조리 후 |
| `Assets/Sprite/kitchen/Kitchen_jerky_no_cooked.png` | 육포 조리 연출 | 조리 전/후 |
| `Assets/Sprite/kitchen/Kitchen_leftdoor.png` | 주방 프롭(싱크·냉장고·버너·문·쓰레기통 등) | 단일 |
| `Assets/Sprite/kitchen/Kitchen_newsink_BG.png` | 주방 프롭(싱크·냉장고·버너·문·쓰레기통 등) | 단일 |
| `Assets/Sprite/kitchen/Kitchen_newsink_hose_close.png` | 싱크 호스 | 수납 |
| `Assets/Sprite/kitchen/Kitchen_newsink_hose_open.png` | 싱크 호스 | 펼침 |
| `Assets/Sprite/kitchen/Kitchen_Pan.png` | 주방 프롭(싱크·냉장고·버너·문·쓰레기통 등) | 단일 |
| `Assets/Sprite/kitchen/Kitchen_Rightdoor.png` | 주방 프롭(싱크·냉장고·버너·문·쓰레기통 등) | 단일 |
| `Assets/Sprite/kitchen/Kitchen_sink.png` | 주방 프롭(싱크·냉장고·버너·문·쓰레기통 등) | 단일 |
| `Assets/Sprite/kitchen/Kitchen_smoke.png` | 가열·연기 이펙트 | 단일 |
| `Assets/Sprite/kitchen/Kitchen_trashbin.png` | 주방 프롭(싱크·냉장고·버너·문·쓰레기통 등) | 단일 |
| `Assets/Sprite/kitchen/주방예시.png` | 주방 합성 목업 | 목업(참고) |

### 3.13 `Lab`
`Basement` 등 지하 실험실 배경입니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/Lab/Lab_BG.png` | 실험실(지하 Lab) 배경 | 배경 레이어 |

### 3.14 `LeftHallway`
1층 좌측 복도(`Hallway_Left`, `Hall_Left` 등). 조명 On/Off가 씬 분위기 전환에 쓰입니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/LeftHallway/Hallway_door_Light_Off.png` | 1층 좌측 복도(Hall_Left 계열) · 조명 On/Off 분기 · 문 조명 | 조명 On/Off |
| `Assets/Sprite/LeftHallway/hallway_door_light_on.png` | 1층 좌측 복도(Hall_Left 계열) · 조명 On/Off 분기 · 문 조명 | 조명 On/Off |
| `Assets/Sprite/LeftHallway/hallway_left1_back_light_off.png` | 1층 좌측 복도(Hall_Left 계열) · 조명 On/Off 분기 | 조명 On/Off |
| `Assets/Sprite/LeftHallway/hallway_left1_back_light_on.png` | 1층 좌측 복도(Hall_Left 계열) · 조명 On/Off 분기 | 조명 On/Off |
| `Assets/Sprite/LeftHallway/hallway_left2_back_light_off.png` | 1층 좌측 복도(Hall_Left 계열) · 조명 On/Off 분기 | 조명 On/Off |
| `Assets/Sprite/LeftHallway/hallway_left2_back_light_on.png` | 1층 좌측 복도(Hall_Left 계열) · 조명 On/Off 분기 | 조명 On/Off |
| `Assets/Sprite/LeftHallway/hallway_left2_light_off.png` | 1층 좌측 복도(Hall_Left 계열) · 조명 On/Off 분기 | 조명 On/Off |
| `Assets/Sprite/LeftHallway/hallway_left2_light_on.png` | 1층 좌측 복도(Hall_Left 계열) · 조명 On/Off 분기 | 조명 On/Off |
| `Assets/Sprite/LeftHallway/hallway_left_light_off.png` | 1층 좌측 복도(Hall_Left 계열) · 조명 On/Off 분기 | 조명 On/Off |
| `Assets/Sprite/LeftHallway/hallway_left_light_on.png` | 1층 좌측 복도(Hall_Left 계열) · 조명 On/Off 분기 | 조명 On/Off |

### 3.15 `made_room`
`MaidRoom` — 서랍 열림/닫힘, 열쇠 선반(열쇠 유무) 등 퍼즐 연출과 직결됩니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/made_room/MaidRoom_bed.png` | 메이드룸(가정부실) 가구·책·수납 | 단일 |
| `Assets/Sprite/made_room/MaidRoom_BG.png` | 메이드룸(가정부실) 가구·책·수납 | 단일 |
| `Assets/Sprite/made_room/MaidRoom_bookshelf.png` | 메이드룸(가정부실) 가구·책·수납 | 단일 |
| `Assets/Sprite/made_room/MaidRoom_Chair.png` | 메이드룸(가정부실) 가구·책·수납 | 단일 |
| `Assets/Sprite/made_room/MaidRoom_cookbook.png` | 메이드룸(가정부실) 가구·책·수납 | 단일 |
| `Assets/Sprite/made_room/MaidRoom_Desk.png` | 메이드룸(가정부실) 가구·책·수납 | 단일 |
| `Assets/Sprite/made_room/MaidRoom_drawer.png` | 서랍 | 기본 |
| `Assets/Sprite/made_room/MaidRoom_drawer_closed.png` | 서랍 | 닫힘 |
| `Assets/Sprite/made_room/MaidRoom_drawer_open.png` | 서랍 | 열림 |
| `Assets/Sprite/made_room/MaidRoom_Keyshelf.png` | 열쇠 선반 | 단일 |
| `Assets/Sprite/made_room/MaidRoom_keyshelf_extend_nokey.png` | 열쇠 선반 | 열쇠 없음(확장) |
| `Assets/Sprite/made_room/MaidRoom_locker.png` | 메이드룸(가정부실) 가구·책·수납 | 단일 |
| `Assets/Sprite/made_room/Maidroom_parrotfood.png` | 메이드룸(가정부실) 가구·책·수납 | 단일 |
| `Assets/Sprite/made_room/MaidRoom_puzzlebook.png` | 메이드룸(가정부실) 가구·책·수납 | 단일 |
| `Assets/Sprite/made_room/가정부실 예시.png` | 메이드룸 목업 | 목업(참고) |

### 3.16 `Mention`
`Opening_Mention` — 저택 외경을 레이어로 쪼개어 문·울타리·하늘을 합성합니다. 해시/생성기 파일명은 정식 명명으로 갈아타는 것을 권장합니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/Mention/312c5a41e3e949b9.png` | 멘션용 임시 이미지 | 확인 필요 |
| `Assets/Sprite/Mention/affter/Gemini_Generated_Image_hjg205hjg205hjg2.png` | 오프닝 멘션 — 저택 외경 레이어(문·울타리·달·별·그림자 등) | 단일 |
| `Assets/Sprite/Mention/affter/mansion_BG.png` | 오프닝 멘션 — 저택 외경 레이어(문·울타리·달·별·그림자 등) | 단일 |
| `Assets/Sprite/Mention/affter/mansion_door.png` | 오프닝 멘션 — 저택 외경 레이어(문·울타리·달·별·그림자 등) | 단일 |
| `Assets/Sprite/Mention/affter/mansion_fence_fullopened.png` | 오프닝 멘션 — 저택 외경 레이어(문·울타리·달·별·그림자 등) | 잠금/오픈 분기 |
| `Assets/Sprite/Mention/affter/mansion_fence_locked (2).png` | 오프닝 멘션 — 저택 외경 레이어(문·울타리·달·별·그림자 등) | 잠금/오픈 분기 |
| `Assets/Sprite/Mention/affter/mansion_lights.png` | 오프닝 멘션 — 저택 외경 레이어(문·울타리·달·별·그림자 등) | 단일 |
| `Assets/Sprite/Mention/affter/mansion_mansion.png` | 오프닝 멘션 — 저택 외경 레이어(문·울타리·달·별·그림자 등) | 단일 |
| `Assets/Sprite/Mention/affter/mansion_moon.png` | 오프닝 멘션 — 저택 외경 레이어(문·울타리·달·별·그림자 등) | 단일 |
| `Assets/Sprite/Mention/affter/mansion_road.png` | 오프닝 멘션 — 저택 외경 레이어(문·울타리·달·별·그림자 등) | 단일 |
| `Assets/Sprite/Mention/affter/mansion_shadow.png` | 오프닝 멘션 — 저택 외경 레이어(문·울타리·달·별·그림자 등) | 단일 |
| `Assets/Sprite/Mention/affter/mansion_stars.png` | 오프닝 멘션 — 저택 외경 레이어(문·울타리·달·별·그림자 등) | 단일 |
| `Assets/Sprite/Mention/bell.png` | 오프닝 멘션 UI/소품 | 단일 |
| `Assets/Sprite/Mention/ca830ccf54cd10e3.png` | 멘션용 임시 이미지 | 확인 필요 |
| `Assets/Sprite/Mention/door_lock_light_o.png` | 오프닝 멘션 UI/소품 | 단일 |
| `Assets/Sprite/Mention/door_lock_light_x 1.png` | 오프닝 멘션 UI/소품 | 단일 |
| `Assets/Sprite/Mention/door_lock_light_x.png` | 오프닝 멘션 UI/소품 | 단일 |
| `Assets/Sprite/Mention/fence_lock.png` | 오프닝 멘션 UI/소품 | 단일 |
| `Assets/Sprite/Mention/fence_locked.png` | 오프닝 멘션 UI/소품 | 단일 |
| `Assets/Sprite/Mention/fence_opened.png` | 오프닝 멘션 UI/소품 | 단일 |
| `Assets/Sprite/Mention/mansion_bell.png` | 오프닝 멘션 — 저택 외경 레이어(문·울타리·달·별·그림자 등) | 단일 |
| `Assets/Sprite/Mention/mansion_lightoff.png` | 오프닝 멘션 — 저택 외경 레이어(문·울타리·달·별·그림자 등) | 단일 |
| `Assets/Sprite/Mention/mansion_squarelight.png` | 오프닝 멘션 — 저택 외경 레이어(문·울타리·달·별·그림자 등) | 단일 |

### 3.17 `Office`
`Opening_Office` — 통화 중/없음 두 상태로 나뉩니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/Office/new_office_calling.png` | 사무실 — 전화 통화 중 연출 | 통화 중 |
| `Assets/Sprite/Office/new_office_nothing.png` | 사무실 — 통화 없음 | 대기 |
| `Assets/Sprite/Office/뉴-사무실 예시.png` | 사무실 목업 | 목업(참고) |

### 3.18 `Puzzle`
서고 등 퍼즐에서 쓰는 카드 UI입니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/Puzzle/FilterCard.png` | 단어/필터 카드 퍼즐 UI | 단일 |
| `Assets/Sprite/Puzzle/WordCard.png` | 단어/필터 카드 퍼즐 UI | 단일 |
| `Assets/Sprite/Puzzle/WordCard2.png` | 단어/필터 카드 퍼즐 UI | 단일 |

### 3.19 `RightHallway`
1층 우측 복도 정면·2배 확대 뷰입니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/RightHallway/1층 오른쪽 복도 예시.png` | 1층 우측 복도 정면 | 목업(참고) |
| `Assets/Sprite/RightHallway/1층오른쪽복도2배확대.png` | 1층 우측 복도 정면 | 단일 |
| `Assets/Sprite/RightHallway/hallway_right_BG.png` | 1층 우측 복도 정면 | 단일 |
| `Assets/Sprite/RightHallway/hallway_right_framebig.png` | 1층 우측 복도 정면 · 액자 | 단일 |
| `Assets/Sprite/RightHallway/hallway_right_framemiddle.png` | 1층 우측 복도 정면 · 액자 | 단일 |
| `Assets/Sprite/RightHallway/hallway_right_framesmall.png` | 1층 우측 복도 정면 · 액자 | 단일 |
| `Assets/Sprite/RightHallway/hallway_right_rightdoor.png` | 1층 우측 복도 정면 | 단일 |
| `Assets/Sprite/RightHallway/hallway_right_showcase.png` | 1층 우측 복도 정면 · 진열장 | 단일 |
| `Assets/Sprite/RightHallway/hallway_right_x2_BG.png` | 1층 우측 복도 정면 · 2배 확대 연출용 레이어 | 단일 |
| `Assets/Sprite/RightHallway/hallway_right_x2_bigframe.png` | 1층 우측 복도 정면 · 2배 확대 연출용 레이어 · 액자 | 단일 |
| `Assets/Sprite/RightHallway/hallway_right_x2_showcase.png` | 1층 우측 복도 정면 · 2배 확대 연출용 레이어 · 진열장 | 단일 |
| `Assets/Sprite/RightHallway/hallway_right_x2_smallframe.png` | 1층 우측 복도 정면 · 2배 확대 연출용 레이어 · 액자 | 단일 |

### 3.20 `RightHallwayBack`
1층 우측 복도 후면(뒤집힌 시점) 레이어입니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/RightHallwayBack/1층오른쪽복도2배뒤로예시.png` | 1층 우측 복도 후면(뒤를 본 시점) | 목업(참고) |
| `Assets/Sprite/RightHallwayBack/1층오른쪽복도뒤로예시.png` | 1층 우측 복도 후면(뒤를 본 시점) | 목업(참고) |
| `Assets/Sprite/RightHallwayBack/hallway_right_back_BG.png` | 1층 우측 복도 후면(뒤를 본 시점) | 전경 레이어 |
| `Assets/Sprite/RightHallwayBack/hallway_right_back_bigframe.png` | 1층 우측 복도 후면(뒤를 본 시점) · 액자 | 전경 레이어 |
| `Assets/Sprite/RightHallwayBack/hallway_right_back_frame1.png` | 1층 우측 복도 후면(뒤를 본 시점) · 액자 | 전경 레이어 |
| `Assets/Sprite/RightHallwayBack/hallway_right_back_frame2.png` | 1층 우측 복도 후면(뒤를 본 시점) · 액자 | 전경 레이어 |
| `Assets/Sprite/RightHallwayBack/hallway_right_back_frame3.png` | 1층 우측 복도 후면(뒤를 본 시점) · 액자 | 전경 레이어 |
| `Assets/Sprite/RightHallwayBack/hallway_right_back_frame4.png` | 1층 우측 복도 후면(뒤를 본 시점) · 액자 | 전경 레이어 |
| `Assets/Sprite/RightHallwayBack/hallway_right_back_frame5.png` | 1층 우측 복도 후면(뒤를 본 시점) · 액자 | 전경 레이어 |
| `Assets/Sprite/RightHallwayBack/hallway_right_back_showcase.png` | 1층 우측 복도 후면(뒤를 본 시점) · 진열장 | 전경 레이어 |
| `Assets/Sprite/RightHallwayBack/hallway_right_back_x2_BG.png` | 1층 우측 복도 후면(뒤를 본 시점) · 2배 확대 | 전경 레이어 |
| `Assets/Sprite/RightHallwayBack/hallway_right_back_x2_bigframe.png` | 1층 우측 복도 후면(뒤를 본 시점) · 2배 확대 · 액자 | 전경 레이어 |
| `Assets/Sprite/RightHallwayBack/hallway_right_back_x2_showcase.png` | 1층 우측 복도 후면(뒤를 본 시점) · 2배 확대 · 진열장 | 전경 레이어 |
| `Assets/Sprite/RightHallwayBack/hallway_right_back_x2_smallframe.png` | 1층 우측 복도 후면(뒤를 본 시점) · 2배 확대 · 액자 | 전경 레이어 |

### 3.21 `SaveImage`
세이브 슬롯에 표시할 장소별 썸네일입니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/SaveImage/1.office.png` | 세이브 데이터 슬롯 썸네일(장소별) | 단일 |
| `Assets/Sprite/SaveImage/10.Hallway Left2.png` | 세이브 데이터 슬롯 썸네일(장소별) | 단일 |
| `Assets/Sprite/SaveImage/2.Opening Mention.png` | 세이브 데이터 슬롯 썸네일(장소별) | 단일 |
| `Assets/Sprite/SaveImage/3.Opening Mention open.png` | 세이브 데이터 슬롯 썸네일(장소별) | 단일 |
| `Assets/Sprite/SaveImage/4.Hall.png` | 세이브 데이터 슬롯 썸네일(장소별) | 단일 |
| `Assets/Sprite/SaveImage/5.Hall Left.png` | 세이브 데이터 슬롯 썸네일(장소별) | 단일 |
| `Assets/Sprite/SaveImage/6.Hall Left2.png` | 세이브 데이터 슬롯 썸네일(장소별) | 단일 |
| `Assets/Sprite/SaveImage/7.Kitchen.png` | 세이브 데이터 슬롯 썸네일(장소별) | 단일 |
| `Assets/Sprite/SaveImage/8.Utility Room.png` | 세이브 데이터 슬롯 썸네일(장소별) | 단일 |
| `Assets/Sprite/SaveImage/9.Hallway Left.png` | 세이브 데이터 슬롯 썸네일(장소별) | 단일 |

### 3.22 `StudyRoom`
`StudyRoom`, `BookCase*`, `StudyEntrance` 등 서재 일련. 책·일기 잠금/오픈, 숨은 칸 등 상태 분기가 많습니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/StudyRoom/bookshelf2_book.png` | 서재(1층) 책장·일기·카드·전면/우측 뷰 | 단일 |
| `Assets/Sprite/StudyRoom/Bookshelf_Book_Example.png` | 서재(1층) 책장·일기·카드·전면/우측 뷰 | 단일 |
| `Assets/Sprite/StudyRoom/Bookshelf_Extend_1.png` | 서재(1층) 책장·일기·카드·전면/우측 뷰 | 단일 |
| `Assets/Sprite/StudyRoom/Bookshelf_Extend_2.png` | 서재(1층) 책장·일기·카드·전면/우측 뷰 | 단일 |
| `Assets/Sprite/StudyRoom/Bookshelf_Extend_3.png` | 서재(1층) 책장·일기·카드·전면/우측 뷰 | 단일 |
| `Assets/Sprite/StudyRoom/Bookshelf_Extend_4.png` | 서재(1층) 책장·일기·카드·전면/우측 뷰 | 단일 |
| `Assets/Sprite/StudyRoom/Bookshelf_Extend_Example.png` | 서재(1층) 책장·일기·카드·전면/우측 뷰 | 단일 |
| `Assets/Sprite/StudyRoom/Bookshelf_hidden.png` | 숨겨진 칸/책 | 오픈 분기 가능 |
| `Assets/Sprite/StudyRoom/Bookshelf_hidden_book.png` | 숨겨진 칸/책 | 오픈 분기 가능 |
| `Assets/Sprite/StudyRoom/Library_card.png` | 서재(1층) 책장·일기·카드·전면/우측 뷰 | 단일 |
| `Assets/Sprite/StudyRoom/library_diary_open.png` | 일기장 | 잠금/오픈 |
| `Assets/Sprite/StudyRoom/library_entrance.png` | 서재 입구 | 단일 |
| `Assets/Sprite/StudyRoom/library_entrance_door.png` | 서재 입구 | 문 포함 |
| `Assets/Sprite/StudyRoom/library_Front 1.png` | 서재(1층) 책장·일기·카드·전면/우측 뷰 | 단일 |
| `Assets/Sprite/StudyRoom/Library_Front.png` | 서재(1층) 책장·일기·카드·전면/우측 뷰 | 단일 |
| `Assets/Sprite/StudyRoom/Library_Front_BG.png` | 서재(1층) 책장·일기·카드·전면/우측 뷰 | 단일 |
| `Assets/Sprite/StudyRoom/Library_Front_Book_Lock.png` | 책(서고 퍼즐) | 잠금/오픈 분기 |
| `Assets/Sprite/StudyRoom/Library_Front_Book_Open.png` | 책(서고 퍼즐) | 잠금/오픈 분기 |
| `Assets/Sprite/StudyRoom/Library_Front_Card.png` | 서재(1층) 책장·일기·카드·전면/우측 뷰 | 단일 |
| `Assets/Sprite/StudyRoom/Library_Front_diary_Lock.png` | 일기장 | 잠금/오픈 |
| `Assets/Sprite/StudyRoom/Library_Front_diary_Open.png` | 일기장 | 잠금/오픈 |
| `Assets/Sprite/StudyRoom/library_Front_hidden_open.png` | 숨겨진 칸/책 | 오픈 분기 가능 |
| `Assets/Sprite/StudyRoom/Library_Rightside_View.png` | 서재(1층) 책장·일기·카드·전면/우측 뷰 | 단일 |
| `Assets/Sprite/StudyRoom/StudyRoom.png` | 서재(1층) 책장·일기·카드·전면/우측 뷰 | 단일 |
| `Assets/Sprite/StudyRoom/서재예시.png` | 서재 목업 | 목업(참고) |
| `Assets/Sprite/StudyRoom/서재입구.png` | 서재(1층) 책장·일기·카드·전면/우측 뷰 | 단일 |
| `Assets/Sprite/StudyRoom/서재책상예시 1.png` | 서재 목업 | 목업(참고) |
| `Assets/Sprite/StudyRoom/서재책상예시.png` | 서재 목업 | 목업(참고) |
| `Assets/Sprite/StudyRoom/서재책오픈예시.png` | 서재 목업 | 목업(참고) |

### 3.23 `TutorRoom`
`TutorRoom` — 2층 과외방. 칠판 확장(`whiteboard_extend`)은 클로즈업/상호작용용입니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/TutorRoom/2f_studyroom_BG.png` | 2층 과외방(공부방) 창·책상·칠판·커튼·둥지 등 | 단일 |
| `Assets/Sprite/TutorRoom/2f_studyroom_bookshelf.png` | 2층 과외방(공부방) 창·책상·칠판·커튼·둥지 등 | 단일 |
| `Assets/Sprite/TutorRoom/2f_studyroom_chair.png` | 2층 과외방(공부방) 창·책상·칠판·커튼·둥지 등 | 단일 |
| `Assets/Sprite/TutorRoom/2f_studyroom_curtain.png` | 2층 과외방(공부방) 창·책상·칠판·커튼·둥지 등 | 단일 |
| `Assets/Sprite/TutorRoom/2f_studyroom_desk.png` | 2층 과외방(공부방) 창·책상·칠판·커튼·둥지 등 | 단일 |
| `Assets/Sprite/TutorRoom/2f_studyroom_key.png` | 2층 과외방(공부방) 창·책상·칠판·커튼·둥지 등 | 단일 |
| `Assets/Sprite/TutorRoom/2f_studyroom_nest.png` | 2층 과외방(공부방) 창·책상·칠판·커튼·둥지 등 | 단일 |
| `Assets/Sprite/TutorRoom/2f_studyroom_outside.png` | 2층 과외방(공부방) 창·책상·칠판·커튼·둥지 등 | 단일 |
| `Assets/Sprite/TutorRoom/2f_studyroom_whiteboard.png` | 2층 과외방(공부방) 창·책상·칠판·커튼·둥지 등 | 단일 |
| `Assets/Sprite/TutorRoom/2f_studyroom_whiteboard_extend.png` | 칠판 확대(상호작용/확대 뷰) | 확장 |
| `Assets/Sprite/TutorRoom/2f_studyroom_window.png` | 2층 과외방(공부방) 창·책상·칠판·커튼·둥지 등 | 단일 |
| `Assets/Sprite/TutorRoom/공부방예시.png` | 과외방 목업 | 목업(참고) |

### 3.24 `UI`
대화·이동 버튼 등 전역 UI입니다.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/UI/button.png` | 공통 UI(대화창·방향 버튼·메인 버튼) | 단일 |
| `Assets/Sprite/UI/dialogue.png` | 공통 UI(대화창·방향 버튼·메인 버튼) | 단일 |
| `Assets/Sprite/UI/downarrow.png` | 공통 UI(대화창·방향 버튼·메인 버튼) | 단일 |
| `Assets/Sprite/UI/leftarrow.png` | 공통 UI(대화창·방향 버튼·메인 버튼) | 단일 |
| `Assets/Sprite/UI/main_button.png` | 공통 UI(대화창·방향 버튼·메인 버튼) | 단일 |
| `Assets/Sprite/UI/rightarrow.png` | 공통 UI(대화창·방향 버튼·메인 버튼) | 단일 |
| `Assets/Sprite/UI/uparrow 1.png` | 공통 UI(대화창·방향 버튼·메인 버튼) | 단일 |
| `Assets/Sprite/UI/uparrow.png` | 공통 UI(대화창·방향 버튼·메인 버튼) | 단일 |

### 3.25 `Uti_room`
`UtilityRoom` — 세탁기·전기 패널·스위치 On/Off.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/Uti_room/Laundry_Room.png` | 다용도실(세탁실) 세탁기·전기 패널·조명 | 단일 |
| `Assets/Sprite/Uti_room/Laundry_Room_Flashlight_On.png` | 다용도실(세탁실) 세탁기·전기 패널·조명 | 손전등/조명 On |
| `Assets/Sprite/Uti_room/Panel.png` | 다용도실(세탁실) 세탁기·전기 패널·조명 | 단일 |
| `Assets/Sprite/Uti_room/panel_inside 1.png` | 패널 내부 클로즈업 | 변형(파일명 번호) |
| `Assets/Sprite/Uti_room/panel_inside.png` | 패널 내부 클로즈업 | 변형(파일명 번호) |
| `Assets/Sprite/Uti_room/Panel_Light.png` | 다용도실(세탁실) 세탁기·전기 패널·조명 | 단일 |
| `Assets/Sprite/Uti_room/panel_switch_off.png` | 패널 스위치 | On/Off |
| `Assets/Sprite/Uti_room/panel_switch_on.png` | 패널 스위치 | On/Off |
| `Assets/Sprite/Uti_room/Washing_Machine.png` | 다용도실(세탁실) 세탁기·전기 패널·조명 | 단일 |
| `Assets/Sprite/Uti_room/Washing_Machine_Light.png` | 다용도실(세탁실) 세탁기·전기 패널·조명 | 단일 |

### 3.26 `WifeRoom`
`WifeRoom`/`BedRoom` 계열 — 안방 가구·창·문 열림 등.

| 상대 경로 | 용도(추정) | 상태·비고 |
|-----------|------------|-----------|
| `Assets/Sprite/WifeRoom/2f_wiferoom_BG.png` | 2층 부인 방(안방) 창·화장대·서랍·액자·시계·문 | 단일 |
| `Assets/Sprite/WifeRoom/2f_wiferoom_clock.png` | 2층 부인 방(안방) 창·화장대·서랍·액자·시계·문 | 단일 |
| `Assets/Sprite/WifeRoom/2f_wiferoom_door_open.png` | 2층 부인 방(안방) 창·화장대·서랍·액자·시계·문 | 문 열림 |
| `Assets/Sprite/WifeRoom/2f_wiferoom_drawer.png` | 2층 부인 방(안방) 창·화장대·서랍·액자·시계·문 | 단일 |
| `Assets/Sprite/WifeRoom/2f_wiferoom_dressingtable.png` | 2층 부인 방(안방) 창·화장대·서랍·액자·시계·문 | 단일 |
| `Assets/Sprite/WifeRoom/2f_wiferoom_frame.png` | 2층 부인 방(안방) 창·화장대·서랍·액자·시계·문 | 단일 |
| `Assets/Sprite/WifeRoom/2f_wiferoom_window.png` | 2층 부인 방(안방) 창·화장대·서랍·액자·시계·문 | 단일 |

---
*자동 생성: `disputatio/docs/generate_graphics_gdd.py` — 인벤토리: `sprite-png-inventory.txt`*
