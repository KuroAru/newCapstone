# Post Processing Fungus Commands Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fungus 플로우차트에서 URP Volume 효과(Vignette, Bloom, ColorAdjustments)를 씬 참조 없이 제어하는 커스텀 커맨드 3개를 추가한다.

**Architecture:** 각 커맨드는 `Fungus.Command`를 상속하는 MonoBehaviour이며, `OnEnter()`에서 `Object.FindFirstObjectByType<Volume>()`으로 Volume을 자동 탐색한다. `duration > 0`이면 SmoothStep 코루틴으로 페이드하고, `waitUntilFinished` 여부에 따라 Continue() 타이밍을 결정한다.

**Tech Stack:** Unity URP (`UnityEngine.Rendering.Universal`), Fungus (`Fungus.Command`), C# Coroutines

---

## 파일 구조

| 동작 | 경로 |
|------|------|
| 생성 | `disputatio/Assets/godlotto/Script/FungusCommands/SetVignette.cs` |
| 생성 | `disputatio/Assets/godlotto/Script/FungusCommands/SetBloom.cs` |
| 생성 | `disputatio/Assets/godlotto/Script/FungusCommands/SetColorAdjustments.cs` |

참고 패턴: `disputatio/Assets/godlotto/Script/FungusCommands/AddItemToInventory.cs`

---

## Task 1: SetVignette 커맨드

**Files:**
- Create: `disputatio/Assets/godlotto/Script/FungusCommands/SetVignette.cs`

- [ ] **Step 1: SetVignette.cs 생성**

```csharp
using System.Collections;
using Fungus;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[CommandInfo("Post Processing",
             "Set Vignette",
             "URP Volume의 Vignette 효과를 설정합니다. Duration이 0이면 즉시 적용, 양수면 SmoothStep으로 부드럽게 변환합니다.")]
[AddComponentMenu("")]
public class SetVignette : Command
{
    [Tooltip("비네트 강도. 0이면 효과 없음, 1이면 화면 가장자리가 완전히 검게 됨. 눈 깜빡임, 의식 흐림 연출에 사용.")]
    [Range(0f, 1f)]
    [SerializeField] private float intensity = 0.5f;

    [Tooltip("비네트 경계 부드러움. 0에 가까울수록 경계가 선명하고, 1에 가까울수록 경계가 부드럽게 퍼짐.")]
    [Range(0f, 1f)]
    [SerializeField] private float smoothness = 0.5f;

    [Tooltip("체크 시 Color 필드를 적용. 미체크 시 색상은 현재 값을 유지한 채 Intensity/Smoothness만 변경.")]
    [SerializeField] private bool overrideColor = false;

    [Tooltip("비네트 색상. 기본값 검정(공포/긴장), 붉은색(공포/위협), 흰색(희망/빛) 등 연출에 따라 변경.")]
    [SerializeField] private Color color = Color.black;

    [Tooltip("변환에 걸리는 시간(초). 0이면 즉시 적용, 양수면 SmoothStep으로 부드럽게 변환.")]
    [SerializeField] private float duration = 0f;

    [Tooltip("체크 시 Duration이 끝날 때까지 플로우차트 진행을 멈춤. 미체크 시 변환 도중에도 다음 커맨드 실행.")]
    [SerializeField] private bool waitUntilFinished = false;

    private Coroutine fadeCoroutine;

    public override void OnEnter()
    {
        Volume volume = Object.FindFirstObjectByType<Volume>();
        if (volume == null || !volume.profile.TryGet(out Vignette vignette))
        {
            Debug.LogWarning("[SetVignette] 씬에서 Vignette 효과를 찾을 수 없습니다.");
            Continue();
            return;
        }

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (duration <= 0f)
        {
            Apply(vignette, intensity, smoothness);
            Continue();
        }
        else
        {
            fadeCoroutine = StartCoroutine(Fade(vignette, duration));
            if (!waitUntilFinished) Continue();
        }
    }

    private void Apply(Vignette vignette, float targetIntensity, float targetSmoothness)
    {
        vignette.intensity.overrideState = true;
        vignette.intensity.value = targetIntensity;
        vignette.smoothness.overrideState = true;
        vignette.smoothness.value = targetSmoothness;
        if (overrideColor)
        {
            vignette.color.overrideState = true;
            vignette.color.value = color;
        }
    }

    private IEnumerator Fade(Vignette vignette, float fadeDuration)
    {
        float startIntensity = vignette.intensity.value;
        float startSmoothness = vignette.smoothness.value;
        Color startColor = vignette.color.value;

        vignette.intensity.overrideState = true;
        vignette.smoothness.overrideState = true;
        if (overrideColor) vignette.color.overrideState = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, t / fadeDuration);
            vignette.intensity.value = Mathf.Lerp(startIntensity, intensity, s);
            vignette.smoothness.value = Mathf.Lerp(startSmoothness, smoothness, s);
            if (overrideColor) vignette.color.value = Color.Lerp(startColor, color, s);
            yield return null;
        }

        Apply(vignette, intensity, smoothness);
        fadeCoroutine = null;
        if (waitUntilFinished) Continue();
    }

    public override string GetSummary()
    {
        string durationStr = duration <= 0f ? "즉시" : $"{duration}s";
        return $"Intensity: {intensity}, Smoothness: {smoothness}, Duration: {durationStr}";
    }

    public override Color GetButtonColor()
    {
        return new Color32(106, 159, 181, 255);
    }
}
```

- [ ] **Step 2: Unity 에디터에서 컴파일 오류 없음 확인**

Unity Editor 콘솔(Window > General > Console)에 빨간 오류 없이 컴파일되면 통과.

- [ ] **Step 3: Fungus 커맨드 메뉴 노출 확인**

씬에 Flowchart 배치 → Block 선택 → `+` 버튼 클릭 → `Post Processing > Set Vignette` 항목이 보이면 통과.

- [ ] **Step 4: 커밋**

```bash
git add disputatio/Assets/godlotto/Script/FungusCommands/SetVignette.cs
git commit -m "feat: Set Vignette Fungus 커맨드 추가"
```

---

## Task 2: SetBloom 커맨드

**Files:**
- Create: `disputatio/Assets/godlotto/Script/FungusCommands/SetBloom.cs`

- [ ] **Step 1: SetBloom.cs 생성**

```csharp
using System.Collections;
using Fungus;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[CommandInfo("Post Processing",
             "Set Bloom",
             "URP Volume의 Bloom 효과 강도를 설정합니다. Duration이 0이면 즉시 적용, 양수면 SmoothStep으로 부드럽게 변환합니다.")]
[AddComponentMenu("")]
public class SetBloom : Command
{
    [Tooltip("블룸 강도. 0이면 효과 없음, 값이 클수록 밝은 영역의 빛 번짐이 강해짐. 몽환적·신성한 분위기 연출에 적합.")]
    [Min(0f)]
    [SerializeField] private float intensity = 1f;

    [Tooltip("변환에 걸리는 시간(초). 0이면 즉시 적용, 양수면 SmoothStep으로 부드럽게 변환.")]
    [SerializeField] private float duration = 0f;

    [Tooltip("체크 시 Duration이 끝날 때까지 플로우차트 진행을 멈춤. 미체크 시 변환 도중에도 다음 커맨드 실행.")]
    [SerializeField] private bool waitUntilFinished = false;

    private Coroutine fadeCoroutine;

    public override void OnEnter()
    {
        Volume volume = Object.FindFirstObjectByType<Volume>();
        if (volume == null || !volume.profile.TryGet(out Bloom bloom))
        {
            Debug.LogWarning("[SetBloom] 씬에서 Bloom 효과를 찾을 수 없습니다.");
            Continue();
            return;
        }

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (duration <= 0f)
        {
            Apply(bloom, intensity);
            Continue();
        }
        else
        {
            fadeCoroutine = StartCoroutine(Fade(bloom, duration));
            if (!waitUntilFinished) Continue();
        }
    }

    private void Apply(Bloom bloom, float targetIntensity)
    {
        bloom.intensity.overrideState = true;
        bloom.intensity.value = targetIntensity;
    }

    private IEnumerator Fade(Bloom bloom, float fadeDuration)
    {
        float startIntensity = bloom.intensity.value;
        bloom.intensity.overrideState = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, t / fadeDuration);
            bloom.intensity.value = Mathf.Lerp(startIntensity, intensity, s);
            yield return null;
        }

        Apply(bloom, intensity);
        fadeCoroutine = null;
        if (waitUntilFinished) Continue();
    }

    public override string GetSummary()
    {
        string durationStr = duration <= 0f ? "즉시" : $"{duration}s";
        return $"Intensity: {intensity}, Duration: {durationStr}";
    }

    public override Color GetButtonColor()
    {
        return new Color32(181, 166, 106, 255);
    }
}
```

- [ ] **Step 2: Unity 에디터에서 컴파일 오류 없음 확인**

Unity Editor 콘솔에 빨간 오류 없으면 통과.

- [ ] **Step 3: 커밋**

```bash
git add disputatio/Assets/godlotto/Script/FungusCommands/SetBloom.cs
git commit -m "feat: Set Bloom Fungus 커맨드 추가"
```

---

## Task 3: SetColorAdjustments 커맨드

**Files:**
- Create: `disputatio/Assets/godlotto/Script/FungusCommands/SetColorAdjustments.cs`

- [ ] **Step 1: SetColorAdjustments.cs 생성**

```csharp
using System.Collections;
using System.Collections.Generic;
using Fungus;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[CommandInfo("Post Processing",
             "Set Color Adjustments",
             "URP Volume의 Color Adjustments 효과(노출, 명암)를 설정합니다. Duration이 0이면 즉시 적용, 양수면 SmoothStep으로 부드럽게 변환합니다.")]
[AddComponentMenu("")]
public class SetColorAdjustments : Command
{
    [Tooltip("체크 시 Post Exposure 필드를 적용. 미체크 시 현재 노출값 유지.")]
    [SerializeField] private bool overridePostExposure = true;

    [Tooltip("노출(밝기) 보정값(EV). 양수면 화면이 밝아지고, 음수면 어두워짐. -3 이하는 거의 완전 암흑, 6 이상은 섬광 효과.")]
    [SerializeField] private float postExposure = 0f;

    [Tooltip("체크 시 Contrast 필드를 적용. 미체크 시 현재 명암값 유지.")]
    [SerializeField] private bool overrideContrast = false;

    [Tooltip("명암 대비. 양수면 밝고 어두운 부분의 차이가 강해지고, 음수면 화면이 회색빛으로 평탄해짐.")]
    [Range(-100f, 100f)]
    [SerializeField] private float contrast = 0f;

    [Tooltip("변환에 걸리는 시간(초). 0이면 즉시 적용, 양수면 SmoothStep으로 부드럽게 변환.")]
    [SerializeField] private float duration = 0f;

    [Tooltip("체크 시 Duration이 끝날 때까지 플로우차트 진행을 멈춤. 미체크 시 변환 도중에도 다음 커맨드 실행.")]
    [SerializeField] private bool waitUntilFinished = false;

    private Coroutine fadeCoroutine;

    public override void OnEnter()
    {
        Volume volume = Object.FindFirstObjectByType<Volume>();
        if (volume == null || !volume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            Debug.LogWarning("[SetColorAdjustments] 씬에서 Color Adjustments 효과를 찾을 수 없습니다.");
            Continue();
            return;
        }

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (duration <= 0f)
        {
            Apply(colorAdjustments, postExposure, contrast);
            Continue();
        }
        else
        {
            fadeCoroutine = StartCoroutine(Fade(colorAdjustments, duration));
            if (!waitUntilFinished) Continue();
        }
    }

    private void Apply(ColorAdjustments ca, float targetExposure, float targetContrast)
    {
        if (overridePostExposure)
        {
            ca.postExposure.overrideState = true;
            ca.postExposure.value = targetExposure;
        }
        if (overrideContrast)
        {
            ca.contrast.overrideState = true;
            ca.contrast.value = targetContrast;
        }
    }

    private IEnumerator Fade(ColorAdjustments ca, float fadeDuration)
    {
        float startExposure = ca.postExposure.value;
        float startContrast = ca.contrast.value;

        if (overridePostExposure) ca.postExposure.overrideState = true;
        if (overrideContrast) ca.contrast.overrideState = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, t / fadeDuration);
            if (overridePostExposure) ca.postExposure.value = Mathf.Lerp(startExposure, postExposure, s);
            if (overrideContrast) ca.contrast.value = Mathf.Lerp(startContrast, contrast, s);
            yield return null;
        }

        Apply(ca, postExposure, contrast);
        fadeCoroutine = null;
        if (waitUntilFinished) Continue();
    }

    public override string GetSummary()
    {
        string durationStr = duration <= 0f ? "즉시" : $"{duration}s";
        var parts = new List<string>();
        if (overridePostExposure) parts.Add($"PostExposure: {postExposure}");
        if (overrideContrast) parts.Add($"Contrast: {contrast}");
        parts.Add($"Duration: {durationStr}");
        return string.Join(", ", parts);
    }

    public override Color GetButtonColor()
    {
        return new Color32(154, 106, 181, 255);
    }
}
```

- [ ] **Step 2: Unity 에디터에서 컴파일 오류 없음 확인**

Unity Editor 콘솔에 빨간 오류 없으면 통과.

- [ ] **Step 3: 커밋**

```bash
git add disputatio/Assets/godlotto/Script/FungusCommands/SetColorAdjustments.cs
git commit -m "feat: Set Color Adjustments Fungus 커맨드 추가"
```

---

## Task 4: 플레이모드 통합 검증

씬에 Post Processing Volume + Flowchart가 있는 씬에서 실행.

- [ ] **Step 1: Set Vignette 즉시 적용 확인**

  Flowchart Block에 `Set Vignette` 추가, Intensity=0.8, Smoothness=0.5, Duration=0. 플레이 → 즉시 비네트 적용 확인.

- [ ] **Step 2: Set Vignette 페이드 + Wait Until Finished 확인**

  Duration=2, Wait Until Finished=true. 다음 커맨드로 `Debug Log "done"` 추가. 플레이 → 2초 후 콘솔에 "done" 출력 확인.

- [ ] **Step 3: Set Vignette 페이드 + 비차단 확인**

  Wait Until Finished=false. 플레이 → "done" 이 페이드 시작 직후 즉시 출력되는지 확인.

- [ ] **Step 4: Set Bloom 확인**

  Intensity=5, Duration=1.5. 플레이 → 빛 번짐이 서서히 강해지는지 확인.

- [ ] **Step 5: Set Color Adjustments 확인**

  Override Post Exposure=true, Post Exposure=-3, Duration=2. 플레이 → 화면이 서서히 어두워지는지 확인.

- [ ] **Step 6: Volume 없는 씬에서 경고 로그 확인**

  Volume이 없는 씬에서 커맨드 실행 → 콘솔에 `[SetVignette] 씬에서 Vignette 효과를 찾을 수 없습니다.` 경고 출력 후 플로우차트 계속 진행 확인.

- [ ] **Step 7: 최종 커밋**

```bash
git add -A
git commit -m "feat: Post Processing Fungus 커맨드 3종 완성 (Vignette, Bloom, ColorAdjustments)"
```
