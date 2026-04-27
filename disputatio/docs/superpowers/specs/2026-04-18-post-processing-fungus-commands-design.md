# Post Processing Fungus Commands — Design Spec

**Date:** 2026-04-18  
**Status:** Approved

---

## 목표

Fungus 플로우차트에서 URP Post Processing Volume 효과(Vignette, Bloom, ColorAdjustments)를 씬 오브젝트 참조 없이 직접 제어할 수 있는 커스텀 커맨드 3개를 추가한다.

---

## 파일 위치

```
disputatio/Assets/godlotto/Script/FungusCommands/
├── SetVignette.cs
├── SetBloom.cs
└── SetColorAdjustments.cs
```

Fungus 커맨드 메뉴 카테고리: **Post Processing**

---

## Volume 탐색

`Object.FindFirstObjectByType<Volume>()`으로 씬에서 자동 탐색한다. `AddItemToInventory`와 동일하게 씬/프리팹 참조 없이 동작하여 씬 전환에 안전하다.

---

## 공통 동작 규칙

| duration 값 | 동작 |
|-------------|------|
| `0` | 즉시 적용 |
| `> 0` | SmoothStep 보간으로 부드럽게 변환 |

- **Wait Until Finished**: 체크 시 페이드가 끝날 때까지 플로우차트 일시정지. 미체크 시 즉시 다음 커맨드로 진행 (배경 페이드 가능).
- 페이드 중 같은 커맨드가 다시 실행되면 기존 코루틴을 중단하고 새 값으로 시작한다.
- 효과를 찾지 못하면 경고 로그를 남기고 `Continue()`로 진행한다.

---

## 커맨드 상세

### 1. Set Vignette

**Fungus 메뉴:** `Post Processing / Set Vignette`  
**버튼 색상:** `#6A9FB5` (차분한 파란 계열)

| 필드 | 타입 | 범위 | 툴팁 |
|------|------|------|------|
| Intensity | float | 0 ~ 1 | 비네트 강도. 0이면 효과 없음, 1이면 화면 가장자리가 완전히 검게 됨. 눈 깜빡임, 의식 흐림 연출에 사용. |
| Smoothness | float | 0 ~ 1 | 비네트 경계 부드러움. 0에 가까울수록 경계가 선명하고, 1에 가까울수록 경계가 부드럽게 퍼짐. |
| Color | Color | — | 비네트 색상. 기본값 검정(공포/긴장), 붉은색(공포/위협), 흰색(희망/빛) 등 연출에 따라 변경. |
| Override Color | bool | — | 체크 시 Color 필드를 적용. 미체크 시 색상은 현재 값을 유지한 채 Intensity/Smoothness만 변경. |
| Duration | float | 0 ~ | 변환에 걸리는 시간(초). 0이면 즉시 적용, 양수면 SmoothStep으로 부드럽게 변환. |
| Wait Until Finished | bool | — | 체크 시 Duration이 끝날 때까지 플로우차트 진행을 멈춤. 미체크 시 변환 도중에도 다음 커맨드 실행. |

**GetSummary 예시:** `Intensity: 0.6, Smoothness: 0.5, Duration: 1.5s`

---

### 2. Set Bloom

**Fungus 메뉴:** `Post Processing / Set Bloom`  
**버튼 색상:** `#B5A66A` (따뜻한 노란 계열)

| 필드 | 타입 | 범위 | 툴팁 |
|------|------|------|------|
| Intensity | float | 0 ~ | 블룸 강도. 0이면 효과 없음, 값이 클수록 밝은 영역의 빛 번짐이 강해짐. 몽환적·신성한 분위기 연출에 적합. |
| Duration | float | 0 ~ | 변환에 걸리는 시간(초). 0이면 즉시 적용, 양수면 SmoothStep으로 부드럽게 변환. |
| Wait Until Finished | bool | — | 체크 시 Duration이 끝날 때까지 플로우차트 진행을 멈춤. 미체크 시 변환 도중에도 다음 커맨드 실행. |

**GetSummary 예시:** `Intensity: 3.0, Duration: 2s`

---

### 3. Set Color Adjustments

**Fungus 메뉴:** `Post Processing / Set Color Adjustments`  
**버튼 색상:** `#9A6AB5` (보라 계열)

| 필드 | 타입 | 범위 | 툴팁 |
|------|------|------|------|
| Post Exposure | float | — | 노출(밝기) 보정값(EV). 양수면 화면이 밝아지고, 음수면 어두워짐. -3 이하는 거의 완전 암흑, 6 이상은 섬광 효과. |
| Override Post Exposure | bool | — | 체크 시 Post Exposure 필드를 적용. 미체크 시 현재 값 유지. |
| Contrast | float | -100 ~ 100 | 명암 대비. 양수면 밝고 어두운 부분의 차이가 강해지고, 음수면 화면이 회색빛으로 평탄해짐. |
| Override Contrast | bool | — | 체크 시 Contrast 필드를 적용. 미체크 시 현재 값 유지. |
| Duration | float | 0 ~ | 변환에 걸리는 시간(초). 0이면 즉시 적용, 양수면 SmoothStep으로 부드럽게 변환. |
| Wait Until Finished | bool | — | 체크 시 Duration이 끝날 때까지 플로우차트 진행을 멈춤. 미체크 시 변환 도중에도 다음 커맨드 실행. |

**GetSummary 예시:** `PostExposure: 1.5, Contrast: 20, Duration: 0s (instant)`

---

## 확장 계획

SC Post Effects (Blur, BlackBars, ColorSplit 등)를 추가할 때도 동일한 패턴(`FindFirstObjectByType`, Override 체크박스, Duration + WaitUntilFinished)을 따른다.
