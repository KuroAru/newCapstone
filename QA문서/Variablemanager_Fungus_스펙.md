# Variablemanager · Fungus 글로벌 변수 스펙

레포에서 자동 추출·교차 검증한 기준 문서입니다. Unity 에디터에서 변수를 추가·이름 변경할 때 이 표와 `FlowchartLocator` / `GameObject.Find("Variablemanager")`를 쓰는 스크립트를 함께 갱신하세요.

## 1. 글로벌 허브

| 항목 | 내용 |
|------|------|
| GameObject 이름 | `Variablemanager` |
| 컴포넌트 | `Flowchart` (Fungus) |
| 싱글톤 유지 | `VariablemanagerSingleton` → `DontDestroyOnLoad`로 최초 1개만 유지 |
| 코드 탐색 | `FlowchartLocator.Find()` → `disputatio/Assets/godlotto/Script/FlowchartLocator.cs` |
| 시리얼라이즈 소스(참고) | `disputatio/Assets/Scenes/Mokotan/First Floor/Hall_playerble.unity` 내 Variablemanager |

## 2. Variablemanager에 정의된 변수 (Hall_playerble 기준)

Fungus **Boolean** (`scope: 2` = Global) 및 **String** / **Integer**입니다. 아래 초기값은 해당 씬 YAML 스냅샷이며, 플레이·세이브 후 달라질 수 있습니다.

### 2-1. Boolean — 열쇠·진행

| 변수명 | 초기값(스냅샷) | 용도(요약) |
|--------|----------------|------------|
| `ElectricOn` | 0 | 전기/공용 진행 (`WhenClikcedButton` ↔ 지도 잠금 UI) |
| `HaveStudyKey` | 1 | 서재 열쇠 보유 |
| `UsedStudyKey` | 1 | 서재 열쇠 사용됨 |
| `HaveMaidKey` | 1 | 가정부 방 열쇠 보유 |
| `UsedMaidKey` | 1 | 사용됨 |
| `HaveTutorKey` | 1 | 가정교사 방 |
| `UsedTutorKey` | 1 | 사용됨 |
| `HaveChildKey` | 1 | 아이 방 |
| `UsedChildKey` | 1 | 사용됨 |
| `HaveWifeKey` | 1 | 아내 방 |
| `UsedWifeKey` | 0 | 사용됨 |
| `HaveBedKey` | 1 | 안방 |
| `UsedBedKey` | 1 | 사용됨 |
| `HaveBasementKey` | 1 | 지하 |
| `UsedBasementKey` | 1 | 사용됨 |
| `HavePrisonKey` | 1 | 감옥 |
| `UsedPrisonKey` | 1 | 사용됨 |
| `PrisonOpen` | 0 | 감옥 개방 상태 |
| `GetBottle` | 0 | 레거시 습득 플래그 → `ItemAcquisitionTracker` 마이그레이션 대상(id 1) |
| `isCalled` | 0 | 지도/메뉴 등 UI 호출 중 (`WhenClikcedButton`, `IntegratedSettingUI`, `InGameSettingsPanel`, `ClickedBubble`, `ControllExit`) |
| `pressTab` | 0 | 인벤토리 탭 (`InventoryManager`) |

### 2-2. String

| 변수명 | 초기값 | 용도 |
|--------|--------|------|
| `PrevScene` | 빈 문자열 | 직전 씬 (`SceneTracker`, `BackNavigator` + `globalFlowchartName: Variablemanager` 프리팹) |

### 2-3. Integer

| 변수명 | 초기값 | 용도 |
|--------|--------|------|
| `AcquiredItemsMask` | 0 | 아이템 습득 비트마스크 (id 1~30), `ItemAcquisitionTracker.FungusVariableKey` |

## 3. 다른 Flowchart에만 있는 전역 성격 변수 (주의)

`GameObject.Find("Variablemanager")`로는 안 잡히지만, 코드·다른 씬에서 `FindObjectOfType<Flowchart>` 등으로 쓰일 수 있습니다.

| 변수명 | 타입 | 발견 위치(예) | 코드 참조 |
|--------|------|----------------|-----------|
| `currentSlot` | int | `Opening_Office.unity` — Variablemanager가 **아닌** 다른 GameObject의 Flowchart | `SaveSlotManager` |
| `SceneName` | string | Opening_Office 등 | `SceneNameSetter`, `ChangeSP` |
| `SavePointKey` | string | Opening_Office 등 | `SceneNameSetter` |
| `MapClicked` | bool | `Hall_playerble.unity` — **별도 Flowchart**(지도 관련 오브젝트) | `HideIfFungusBoolTrue` 기본값, 씬 YAML |

**리스크:** `HideIfFungusBoolTrue`는 `FlowchartLocator`로 **Variablemanager만** 조회합니다. `MapClicked`가 Variablemanager에 없고 다른 Flowchart에만 있으면, Fungus 글로벌 스코프 동작에 따라 true/false가 기대와 다를 수 있습니다. QA 시 지도 픽업 후 오브젝트 숨김이 깨지면 이 불일치를 우선 의심하세요.

## 4. 지도 UI (`WhenClikcedButton`) ↔ bool 매핑

| 지도에서 쓰는 bool (열림) | 잠금 표시에 쓰는 키 |
|---------------------------|---------------------|
| `kitchen` / `unLocked` | `ElectricOn` |
| `lib` / `locked` | `UsedStudyKey` |
| `made` / `lock2` | `UsedMaidKey` |
| `bed` / `Lock_2_4` | `UsedBedKey` |
| `wife` / `Lock_2_3` | `UsedWifeKey` |
| `Tutor` / `Lock_2_2` | `UsedTutorKey` |
| `Child` / `Lock_2_1` | `UsedChildKey` |

## 5. 방(씬) 로컬 Flowchart — 챗봇·기믹이 읽는 변수

전역이 아니라 **해당 씬 Flowchart**에 두는 이름입니다. 이름 변경 시 인스펙터 연결을 같이 수정해야 합니다.

| 스크립트 | 변수명 | 타입 |
|----------|--------|------|
| `WifeRoomChatbot` | `CheckedMirror` | bool |
| `KitchenChatbot` | `giveFood` | bool |
| `MainBedroomChatbot` | `DiaryRead`, `SafeSolved` | bool |
| `SonRoomChatbot` | `HasBible` | bool |
| `SonRoomChatbot` | `HorsesPlacedCount` | int |
| `TutorChatbot` | `WindowClicked` | bool |
| `TutorChatbot` | `CorrectAnswerCount` | int |
| `QuizInputHandler` | `PlayerAnswer`(기본) | string |
| `CombinationLock`(등) | `solved` | bool |
| `UISafeLockController` | 인스펙터 지정 | bool |
| `SealManager` | 인스펙터 지정 | bool |

`ItemPickup` / `DraggableSnap2D` 등은 인스펙터의 `fungusVariableName`으로 bool을 씁니다 (씬마다 다름).

## 6. `ItemAcquisitionTracker`와 레거시 bool

| 레거시 bool | 비트마스크 id |
|-------------|----------------|
| `GetBottle` | 1 |
| `HasBible` | 19 |

`MigrateLegacyBools` 호출 시 Variablemanager에서 위 bool을 읽어 `AcquiredItemsMask`에 반영합니다.

## 7. 스펙 갱신 방법 (개발자용)

Variablemanager 변수 목록을 YAML에서 다시 뽑을 때(동일 GameObject의 `key:` 필드):

```bash
# 예: Hall_playerble의 Variablemanager GameObject fileID는 1594444090
# (Unity 버전/씬 저장에 따라 fileID는 바뀔 수 있으니, m_Name: Variablemanager 블록으로 확인)
```

에디터에서 Flowchart 변수를 추가했다면 **반드시** `GetBooleanVariable` / `SetBooleanVariable` / `GetStringVariable` 등으로 해당 키를 참조하는 C#을 `rg`로 검색해 목록을 맞춥니다.

---

*생성: 레포 분석 기준. 씬 fileID·초기값은 Unity 저장 시 변할 수 있음.*
