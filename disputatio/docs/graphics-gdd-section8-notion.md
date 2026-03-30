## 8. 그래픽 기획 (2D 스프라이트)
### 8-1. 개요
- Unity 프로젝트 **`Assets/Sprite`** 이하 PNG **253종**(2026-03 기준, `.meta` 기준 집계)은 공간·기능 단위 하위 폴더로 정리되어 있다.
- 네이밍: **`_BG`** 배경, **`_open` / `_closed` / `close`** 상태 분기, **`Light_On` / `Off`** 조명, **`Kitchen_`**, **`2f_`**, **`MaidRoom_`**, **`jail_`** 등 접두로 씬 배치 시 식별이 쉽다.
- 본 절은 **레포 내 상대 경로**를 기준으로 기획·아트·씬 배치 담당이 동일 용어로 소통하도록 한다. (노션 이미지 업로드 없이 경로만으로 추적 가능)
### 8-2. 씬·공간과 스프라이트 폴더 대응 (요약)
<table header-row="true" fit-page-width="true">
<tr>
<td>Mokotan 씬(빌드)</td>
<td>주요 스프라이트 폴더</td>
</tr>
<tr>
<td>Opening_Office, Office 관련</td>
<td>Office/</td>
</tr>
<tr>
<td>Opening_Mention, Mention</td>
<td>Mention/</td>
</tr>
<tr>
<td>Hall_playerble, CentralHall류</td>
<td>Hall/, Hallway/</td>
</tr>
<tr>
<td>Kitchen</td>
<td>kitchen/</td>
</tr>
<tr>
<td>UtilityRoom</td>
<td>Uti_room/</td>
</tr>
<tr>
<td>Hall_Left / Hallway_Left*</td>
<td>LeftHallway/</td>
</tr>
<tr>
<td>Hall_Right / Hallway_Right*</td>
<td>RightHallway/, RightHallwayBack/</td>
</tr>
<tr>
<td>MaidRoom</td>
<td>made_room/</td>
</tr>
<tr>
<td>StudyRoom, BookCase*</td>
<td>StudyRoom/</td>
</tr>
<tr>
<td>Prison, PrisonEntrance</td>
<td>Jail/</td>
</tr>
<tr>
<td>2floorMainHall</td>
<td>2floorHall/</td>
</tr>
<tr>
<td>2floorHallway_Left, 2floorLeft*</td>
<td>2floorLeftHallway/</td>
</tr>
<tr>
<td>2floorHallway_Right, 2floorRight*</td>
<td>2floorRightHallway/</td>
</tr>
<tr>
<td>TutorRoom</td>
<td>TutorRoom/</td>
</tr>
<tr>
<td>ChildRoom</td>
<td>ChildSP/</td>
</tr>
<tr>
<td>WifeRoom, DressingRoom</td>
<td>WifeRoom/</td>
</tr>
<tr>
<td>BedRoom</td>
<td>WifeRoom/ 또는 안방 전용 에셋 혼용 시 확인</td>
</tr>
<tr>
<td>IntroScene</td>
<td>Intro/</td>
</tr>
<tr>
<td>메인 메뉴·UI</td>
<td>UI/, SaveImage/</td>
</tr>
<tr>
<td>플레이어/앵무</td>
<td>Charactor/</td>
</tr>
</table>
### 8-3. 폴더별 스프라이트 카탈로그
각 항목 **용도** 열은 파일명·폴더 관례에 따른 **기획 추정**이며, 실씬 배치와 다를 경우 인게임 기준으로 수정한다.
<details>
<summary><strong>2floorHall</strong> — 2층 메인 홀 (5개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/2floorHall/2f_mainhall_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorHall/2f_mainhall_jesus.png`</td>
<td>복도 종교 도상/액자</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorHall/2f_mainhall_leftdoor.png`</td>
<td>문 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorHall/2f_mainhall_rightdoor.png`</td>
<td>문 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorHall/2층메인홀예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
</table>
</details>
<details>
<summary><strong>2floorLeftHallway</strong> — 2층 왼쪽 복도 (14개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/2floorLeftHallway/2f_hallway_left_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorLeftHallway/2f_hallway_left_back_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorLeftHallway/2f_hallway_left_back_case_big.png`</td>
<td>복도 구성 요소</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorLeftHallway/2f_hallway_left_back_case_middle.png`</td>
<td>복도 구성 요소</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorLeftHallway/2f_hallway_left_back_case_small.png`</td>
<td>복도 구성 요소</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorLeftHallway/2f_hallway_left_back_frame.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorLeftHallway/2f_hallway_left_case_big.png`</td>
<td>복도 구성 요소</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorLeftHallway/2f_hallway_left_case_middle.png`</td>
<td>복도 구성 요소</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorLeftHallway/2f_hallway_left_case_small.png`</td>
<td>복도 구성 요소</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorLeftHallway/2f_hallway_left_frame.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorLeftHallway/2층왼쪽복도 예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorLeftHallway/2층왼쪽복도 예시_불.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorLeftHallway/2층왼쪽복도역방향예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorLeftHallway/2층왼쪽복도역방향예시_불.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
</table>
</details>
<details>
<summary><strong>2floorRightHallway</strong> — 2층 오른쪽 복도 (8개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/2floorRightHallway/2f_hallway_right_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorRightHallway/2f_hallway_right_back_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorRightHallway/2f_hallway_right_back_x2_frame_jesus.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorRightHallway/2f_hallway_right_back_x2_frame_marry.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorRightHallway/2f_hallway_right_frame_jesus.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorRightHallway/2f_hallway_right_frame_marry.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorRightHallway/2층오른쪽복도뒤로예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/2floorRightHallway/2층오른쪽복도예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
</table>
</details>
<details>
<summary><strong>_SpriteRoot</strong> — Sprite 폴더 직하위(루트) (2개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/Main_Title_o.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/office.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
</table>
</details>
<details>
<summary><strong>Charactor</strong> — 캐릭터 (4개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/Charactor/CharEX1.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Charactor/MainCharEX1-1.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Charactor/MainCharEX1.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Charactor/parrot.png`</td>
<td>캐릭터/앵무 스프라이트</td>
</tr>
</table>
</details>
<details>
<summary><strong>ChildSP</strong> — 아이 방 (ChildRoom) (12개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/ChildSP/ChildBG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/ChildSP/Funiture/상자.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/ChildSP/Funiture/상자_수정.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/ChildSP/Funiture/서랍_확대_수정3.png`</td>
<td>서랍/수납</td>
</tr>
<tr>
<td>`Assets/Sprite/ChildSP/Funiture/설랍.png`</td>
<td>서랍/수납</td>
</tr>
<tr>
<td>`Assets/Sprite/ChildSP/Funiture/침대.png`</td>
<td>침대</td>
</tr>
<tr>
<td>`Assets/Sprite/ChildSP/Funiture/탁자_수정1.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/ChildSP/Funiture/협탁 확대 1.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/ChildSP/말/Sprite-0006.png`</td>
<td>목마/기사 (용도 확인 필요)</td>
</tr>
<tr>
<td>`Assets/Sprite/ChildSP/말/역병.png`</td>
<td>네 기사/목마 퍼즐 조각</td>
</tr>
<tr>
<td>`Assets/Sprite/ChildSP/말/전쟁.png`</td>
<td>네 기사/목마 퍼즐 조각</td>
</tr>
<tr>
<td>`Assets/Sprite/ChildSP/말/정복.png`</td>
<td>네 기사/목마 퍼즐 조각</td>
</tr>
</table>
</details>
<details>
<summary><strong>Hall</strong> — 1층 홀 (11개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/Hall/Floor_Flashlight_On.png`</td>
<td>조명 켜짐</td>
</tr>
<tr>
<td>`Assets/Sprite/Hall/Floor_Flashlight_off.png`</td>
<td>조명 꺼짐</td>
</tr>
<tr>
<td>`Assets/Sprite/Hall/Floor_door_Flashlight_Off.png`</td>
<td>조명 꺼짐</td>
</tr>
<tr>
<td>`Assets/Sprite/Hall/Floor_door_Flashlight_On.png`</td>
<td>조명 켜짐</td>
</tr>
<tr>
<td>`Assets/Sprite/Hall/Floor_light_on.png`</td>
<td>조명 켜짐</td>
</tr>
<tr>
<td>`Assets/Sprite/Hall/Floor_parrot_talk_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/Hall/Floor_parrot_talk_sheet_big.png`</td>
<td>캐릭터/앵무 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Hall/Floor_parrot_talk_stand.png`</td>
<td>캐릭터/앵무 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Hall/Hallway_door_Light_On.png`</td>
<td>조명 켜짐</td>
</tr>
<tr>
<td>`Assets/Sprite/Hall/door.png`</td>
<td>문 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/Hall/저택현관.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
</table>
</details>
<details>
<summary><strong>Hallway</strong> — 복도 (공통) (1개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/Hallway/TwoRoot.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
</table>
</details>
<details>
<summary><strong>Intro</strong> — 인트로/오프닝 (6개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/Intro/first_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/Intro/first_chainsaw_1.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Intro/first_chainsaw_2.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Intro/first_chainsaw_3.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Intro/first_chainsaw_4.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Intro/첫장면예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
</table>
</details>
<details>
<summary><strong>Item</strong> — 아이템 아이콘 (2개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/Item/normal_key.png`</td>
<td>열쇠/키 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/Item/water_bottle.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
</table>
</details>
<details>
<summary><strong>Jail</strong> — 감옥 (Prison) (14개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/Jail/jail_fullBG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/Jail/jail_inside_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/Jail/jail_inside_desk.png`</td>
<td>책상</td>
</tr>
<tr>
<td>`Assets/Sprite/Jail/jail_inside_fence.png`</td>
<td>울타리/철창</td>
</tr>
<tr>
<td>`Assets/Sprite/Jail/jail_inside_inside_bed.png`</td>
<td>침대</td>
</tr>
<tr>
<td>`Assets/Sprite/Jail/jail_inside_inside_key.png`</td>
<td>열쇠/키 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/Jail/jail_inside_inside_letter.png`</td>
<td>편지</td>
</tr>
<tr>
<td>`Assets/Sprite/Jail/jail_inside_skeleton.png`</td>
<td>해골 (연출)</td>
</tr>
<tr>
<td>`Assets/Sprite/Jail/jail_locker 1.png`</td>
<td>사물함</td>
</tr>
<tr>
<td>`Assets/Sprite/Jail/jail_locker.png`</td>
<td>사물함</td>
</tr>
<tr>
<td>`Assets/Sprite/Jail/jail_locker_key.png`</td>
<td>사물함</td>
</tr>
<tr>
<td>`Assets/Sprite/Jail/감옥안쪽예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/Jail/감옥예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/Jail/감옥자물쇠예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
</table>
</details>
<details>
<summary><strong>kitchen</strong> — 주방 (17개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/kitchen/Kitchen_Burner.png`</td>
<td>주방 조리/설비 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/kitchen/Kitchen_Pan.png`</td>
<td>주방 조리/설비 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/kitchen/Kitchen_Rightdoor.png`</td>
<td>문 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/kitchen/Kitchen_burner_extend.png`</td>
<td>주방 조리/설비 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/kitchen/Kitchen_fridge.png`</td>
<td>주방 조리/설비 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/kitchen/Kitchen_frypan.png`</td>
<td>주방 조리/설비 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/kitchen/Kitchen_jerky_cooked.png`</td>
<td>육포 조리 상태</td>
</tr>
<tr>
<td>`Assets/Sprite/kitchen/Kitchen_jerky_no_cooked.png`</td>
<td>육포 조리 상태</td>
</tr>
<tr>
<td>`Assets/Sprite/kitchen/Kitchen_leftdoor.png`</td>
<td>문 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/kitchen/Kitchen_newsink_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/kitchen/Kitchen_newsink_hose_close.png`</td>
<td>상태: 닫힘</td>
</tr>
<tr>
<td>`Assets/Sprite/kitchen/Kitchen_newsink_hose_open.png`</td>
<td>상태: 열림/펼침</td>
</tr>
<tr>
<td>`Assets/Sprite/kitchen/Kitchen_sink.png`</td>
<td>주방 조리/설비 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/kitchen/Kitchen_smoke.png`</td>
<td>이펙트 (연기)</td>
</tr>
<tr>
<td>`Assets/Sprite/kitchen/Kitchen_trashbin.png`</td>
<td>주방 조리/설비 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/kitchen/kitchen_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/kitchen/주방예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
</table>
</details>
<details>
<summary><strong>Lab</strong> — 실험실/연구 (Lab) (1개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/Lab/Lab_BG.png`</td>
<td>배경 레이어</td>
</tr>
</table>
</details>
<details>
<summary><strong>LeftHallway</strong> — 1층 왼쪽 복도 (10개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/LeftHallway/Hallway_door_Light_Off.png`</td>
<td>조명 꺼짐</td>
</tr>
<tr>
<td>`Assets/Sprite/LeftHallway/hallway_door_light_on.png`</td>
<td>조명 켜짐</td>
</tr>
<tr>
<td>`Assets/Sprite/LeftHallway/hallway_left1_back_light_off.png`</td>
<td>조명 꺼짐</td>
</tr>
<tr>
<td>`Assets/Sprite/LeftHallway/hallway_left1_back_light_on.png`</td>
<td>조명 켜짐</td>
</tr>
<tr>
<td>`Assets/Sprite/LeftHallway/hallway_left2_back_light_off.png`</td>
<td>조명 꺼짐</td>
</tr>
<tr>
<td>`Assets/Sprite/LeftHallway/hallway_left2_back_light_on.png`</td>
<td>조명 켜짐</td>
</tr>
<tr>
<td>`Assets/Sprite/LeftHallway/hallway_left2_light_off.png`</td>
<td>조명 꺼짐</td>
</tr>
<tr>
<td>`Assets/Sprite/LeftHallway/hallway_left2_light_on.png`</td>
<td>조명 켜짐</td>
</tr>
<tr>
<td>`Assets/Sprite/LeftHallway/hallway_left_light_off.png`</td>
<td>조명 꺼짐</td>
</tr>
<tr>
<td>`Assets/Sprite/LeftHallway/hallway_left_light_on.png`</td>
<td>조명 켜짐</td>
</tr>
</table>
</details>
<details>
<summary><strong>made_room</strong> — 가정부 방 (MaidRoom) (15개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/made_room/MaidRoom_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/made_room/MaidRoom_Chair.png`</td>
<td>의자</td>
</tr>
<tr>
<td>`Assets/Sprite/made_room/MaidRoom_Desk.png`</td>
<td>책상</td>
</tr>
<tr>
<td>`Assets/Sprite/made_room/MaidRoom_Keyshelf.png`</td>
<td>열쇠/키 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/made_room/MaidRoom_bed.png`</td>
<td>침대</td>
</tr>
<tr>
<td>`Assets/Sprite/made_room/MaidRoom_bookshelf.png`</td>
<td>책장</td>
</tr>
<tr>
<td>`Assets/Sprite/made_room/MaidRoom_cookbook.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/made_room/MaidRoom_drawer.png`</td>
<td>서랍/수납</td>
</tr>
<tr>
<td>`Assets/Sprite/made_room/MaidRoom_drawer_closed.png`</td>
<td>상태: 닫힘</td>
</tr>
<tr>
<td>`Assets/Sprite/made_room/MaidRoom_drawer_open.png`</td>
<td>상태: 열림/펼침</td>
</tr>
<tr>
<td>`Assets/Sprite/made_room/MaidRoom_keyshelf_extend_nokey.png`</td>
<td>열쇠/키 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/made_room/MaidRoom_locker.png`</td>
<td>사물함</td>
</tr>
<tr>
<td>`Assets/Sprite/made_room/MaidRoom_puzzlebook.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/made_room/Maidroom_parrotfood.png`</td>
<td>캐릭터/앵무 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/made_room/가정부실 예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
</table>
</details>
<details>
<summary><strong>Mention</strong> — 멘션/자막 씬 (23개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/Mention/312c5a41e3e949b9.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/affter/Gemini_Generated_Image_hjg205hjg205hjg2.png`</td>
<td>생성 이미지 (멘션 씬 등)</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/affter/mansion_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/affter/mansion_door.png`</td>
<td>문 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/affter/mansion_fence_fullopened.png`</td>
<td>울타리/철창</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/affter/mansion_fence_locked (2).png`</td>
<td>울타리/철창</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/affter/mansion_lights.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/affter/mansion_mansion.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/affter/mansion_moon.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/affter/mansion_road.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/affter/mansion_shadow.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/affter/mansion_stars.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/bell.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/ca830ccf54cd10e3.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/door_lock_light_o.png`</td>
<td>문 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/door_lock_light_x 1.png`</td>
<td>문 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/door_lock_light_x.png`</td>
<td>문 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/fence_lock.png`</td>
<td>울타리/철창</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/fence_locked.png`</td>
<td>울타리/철창</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/fence_opened.png`</td>
<td>상태: 열림/펼침</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/mansion_bell.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/mansion_lightoff.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Mention/mansion_squarelight.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
</table>
</details>
<details>
<summary><strong>Office</strong> — 사무실 (오프닝) (3개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/Office/new_office_calling.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Office/new_office_nothing.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Office/뉴-사무실 예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
</table>
</details>
<details>
<summary><strong>Puzzle</strong> — Puzzle (3개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/Puzzle/FilterCard.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Puzzle/WordCard.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/Puzzle/WordCard2.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
</table>
</details>
<details>
<summary><strong>RightHallway</strong> — 1층 오른쪽 복도 (12개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/RightHallway/1층 오른쪽 복도 예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallway/1층오른쪽복도2배확대.png`</td>
<td>복도 구성 요소</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallway/hallway_right_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallway/hallway_right_framebig.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallway/hallway_right_framemiddle.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallway/hallway_right_framesmall.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallway/hallway_right_rightdoor.png`</td>
<td>문 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallway/hallway_right_showcase.png`</td>
<td>진열장</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallway/hallway_right_x2_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallway/hallway_right_x2_bigframe.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallway/hallway_right_x2_showcase.png`</td>
<td>진열장</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallway/hallway_right_x2_smallframe.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
</table>
</details>
<details>
<summary><strong>RightHallwayBack</strong> — 1층 오른쪽 복도(뒤) (14개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/RightHallwayBack/1층오른쪽복도2배뒤로예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallwayBack/1층오른쪽복도뒤로예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallwayBack/hallway_right_back_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallwayBack/hallway_right_back_bigframe.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallwayBack/hallway_right_back_frame1.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallwayBack/hallway_right_back_frame2.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallwayBack/hallway_right_back_frame3.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallwayBack/hallway_right_back_frame4.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallwayBack/hallway_right_back_frame5.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallwayBack/hallway_right_back_showcase.png`</td>
<td>진열장</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallwayBack/hallway_right_back_x2_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallwayBack/hallway_right_back_x2_bigframe.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallwayBack/hallway_right_back_x2_showcase.png`</td>
<td>진열장</td>
</tr>
<tr>
<td>`Assets/Sprite/RightHallwayBack/hallway_right_back_x2_smallframe.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
</table>
</details>
<details>
<summary><strong>SaveImage</strong> — 세이브 슬롯 썸네일 (10개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/SaveImage/1.office.png`</td>
<td>세이브 슬롯 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/SaveImage/10.Hallway Left2.png`</td>
<td>복도 구성 요소</td>
</tr>
<tr>
<td>`Assets/Sprite/SaveImage/2.Opening Mention.png`</td>
<td>세이브 슬롯 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/SaveImage/3.Opening Mention open.png`</td>
<td>상태: 열림/펼침</td>
</tr>
<tr>
<td>`Assets/Sprite/SaveImage/4.Hall.png`</td>
<td>세이브 슬롯 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/SaveImage/5.Hall Left.png`</td>
<td>세이브 슬롯 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/SaveImage/6.Hall Left2.png`</td>
<td>세이브 슬롯 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/SaveImage/7.Kitchen.png`</td>
<td>세이브 슬롯 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/SaveImage/8.Utility Room.png`</td>
<td>세이브 슬롯 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/SaveImage/9.Hallway Left.png`</td>
<td>복도 구성 요소</td>
</tr>
</table>
</details>
<details>
<summary><strong>StudyRoom</strong> — 서재 (StudyRoom) (29개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/StudyRoom/Bookshelf_Book_Example.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/Bookshelf_Extend_1.png`</td>
<td>책장</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/Bookshelf_Extend_2.png`</td>
<td>책장</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/Bookshelf_Extend_3.png`</td>
<td>책장</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/Bookshelf_Extend_4.png`</td>
<td>책장</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/Bookshelf_Extend_Example.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/Bookshelf_hidden.png`</td>
<td>책장</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/Bookshelf_hidden_book.png`</td>
<td>책장</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/Library_Front.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/Library_Front_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/Library_Front_Book_Lock.png`</td>
<td>서재 책/책장 변형</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/Library_Front_Book_Open.png`</td>
<td>상태: 열림/펼침</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/Library_Front_Card.png`</td>
<td>서재 카드</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/Library_Front_diary_Lock.png`</td>
<td>일기장</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/Library_Front_diary_Open.png`</td>
<td>상태: 열림/펼침</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/Library_Rightside_View.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/Library_card.png`</td>
<td>서재 카드</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/StudyRoom.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/bookshelf2_book.png`</td>
<td>책장</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/library_Front 1.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/library_Front_hidden_open.png`</td>
<td>상태: 열림/펼침</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/library_diary_open.png`</td>
<td>상태: 열림/펼침</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/library_entrance.png`</td>
<td>입구 연출</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/library_entrance_door.png`</td>
<td>문 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/서재예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/서재입구.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/서재책상예시 1.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/서재책상예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
<tr>
<td>`Assets/Sprite/StudyRoom/서재책오픈예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
</table>
</details>
<details>
<summary><strong>TutorRoom</strong> — 가정교사 방 (12개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/TutorRoom/2f_studyroom_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/TutorRoom/2f_studyroom_bookshelf.png`</td>
<td>책장</td>
</tr>
<tr>
<td>`Assets/Sprite/TutorRoom/2f_studyroom_chair.png`</td>
<td>의자</td>
</tr>
<tr>
<td>`Assets/Sprite/TutorRoom/2f_studyroom_curtain.png`</td>
<td>커튼</td>
</tr>
<tr>
<td>`Assets/Sprite/TutorRoom/2f_studyroom_desk.png`</td>
<td>책상</td>
</tr>
<tr>
<td>`Assets/Sprite/TutorRoom/2f_studyroom_key.png`</td>
<td>열쇠/키 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/TutorRoom/2f_studyroom_nest.png`</td>
<td>앵무새 둥지</td>
</tr>
<tr>
<td>`Assets/Sprite/TutorRoom/2f_studyroom_outside.png`</td>
<td>씬 구성 스프라이트</td>
</tr>
<tr>
<td>`Assets/Sprite/TutorRoom/2f_studyroom_whiteboard.png`</td>
<td>칠판</td>
</tr>
<tr>
<td>`Assets/Sprite/TutorRoom/2f_studyroom_whiteboard_extend.png`</td>
<td>칠판</td>
</tr>
<tr>
<td>`Assets/Sprite/TutorRoom/2f_studyroom_window.png`</td>
<td>창문</td>
</tr>
<tr>
<td>`Assets/Sprite/TutorRoom/공부방예시.png`</td>
<td>기획/합성 참고용 썸네일</td>
</tr>
</table>
</details>
<details>
<summary><strong>UI</strong> — UI 공통 (8개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/UI/button.png`</td>
<td>UI 공통</td>
</tr>
<tr>
<td>`Assets/Sprite/UI/dialogue.png`</td>
<td>UI 공통</td>
</tr>
<tr>
<td>`Assets/Sprite/UI/downarrow.png`</td>
<td>UI 방향 화살표</td>
</tr>
<tr>
<td>`Assets/Sprite/UI/leftarrow.png`</td>
<td>UI 방향 화살표</td>
</tr>
<tr>
<td>`Assets/Sprite/UI/main_button.png`</td>
<td>UI 공통</td>
</tr>
<tr>
<td>`Assets/Sprite/UI/rightarrow.png`</td>
<td>UI 방향 화살표</td>
</tr>
<tr>
<td>`Assets/Sprite/UI/uparrow 1.png`</td>
<td>UI 방향 화살표</td>
</tr>
<tr>
<td>`Assets/Sprite/UI/uparrow.png`</td>
<td>UI 방향 화살표</td>
</tr>
</table>
</details>
<details>
<summary><strong>Uti_room</strong> — 유틸리티 룸 (10개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/Uti_room/Laundry_Room.png`</td>
<td>다용도실(세탁/전기 패널)</td>
</tr>
<tr>
<td>`Assets/Sprite/Uti_room/Laundry_Room_Flashlight_On.png`</td>
<td>조명 켜짐</td>
</tr>
<tr>
<td>`Assets/Sprite/Uti_room/Panel.png`</td>
<td>주방 조리/설비 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/Uti_room/Panel_Light.png`</td>
<td>주방 조리/설비 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/Uti_room/Washing_Machine.png`</td>
<td>다용도실(세탁/전기 패널)</td>
</tr>
<tr>
<td>`Assets/Sprite/Uti_room/Washing_Machine_Light.png`</td>
<td>다용도실(세탁/전기 패널)</td>
</tr>
<tr>
<td>`Assets/Sprite/Uti_room/panel_inside 1.png`</td>
<td>주방 조리/설비 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/Uti_room/panel_inside.png`</td>
<td>주방 조리/설비 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/Uti_room/panel_switch_off.png`</td>
<td>주방 조리/설비 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/Uti_room/panel_switch_on.png`</td>
<td>주방 조리/설비 오브젝트</td>
</tr>
</table>
</details>
<details>
<summary><strong>WifeRoom</strong> — 아내의 방 (7개)</summary>
<table header-row="true" fit-page-width="true">
<tr><td>상대 경로</td><td>용도(기획 추정)</td></tr>
<tr>
<td>`Assets/Sprite/WifeRoom/2f_wiferoom_BG.png`</td>
<td>배경 레이어</td>
</tr>
<tr>
<td>`Assets/Sprite/WifeRoom/2f_wiferoom_clock.png`</td>
<td>시계</td>
</tr>
<tr>
<td>`Assets/Sprite/WifeRoom/2f_wiferoom_door_open.png`</td>
<td>문/개폐 연출 (열림)</td>
</tr>
<tr>
<td>`Assets/Sprite/WifeRoom/2f_wiferoom_drawer.png`</td>
<td>서랍/수납</td>
</tr>
<tr>
<td>`Assets/Sprite/WifeRoom/2f_wiferoom_dressingtable.png`</td>
<td>화장대</td>
</tr>
<tr>
<td>`Assets/Sprite/WifeRoom/2f_wiferoom_frame.png`</td>
<td>액자/프레임 오브젝트</td>
</tr>
<tr>
<td>`Assets/Sprite/WifeRoom/2f_wiferoom_window.png`</td>
<td>창문</td>
</tr>
</table>
</details>
### 8-4. 전체 경로 목록 (원본)
- 레포 파일: `disputatio/docs/sprite-png-inventory.txt` (253줄, 정렬됨)
