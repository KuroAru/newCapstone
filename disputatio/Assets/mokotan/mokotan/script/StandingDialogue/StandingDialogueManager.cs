// Prefab 구조:
// Canvas (StandingDialogueCanvas) — Sort Order: 50, Render Mode: Screen Space Overlay
//   CanvasGroup
//   ├── LeftSlot
//   │     ├── LeftCharImage (Image, Preserve Aspect: true)
//   │     └── LeftOverlay (Image, Color: 000000 alpha:0, raycastTarget: false)
//   ├── RightSlot
//   │     ├── RightCharImage (Image, Preserve Aspect: true)
//   │     └── RightOverlay (Image, Color: 000000 alpha:0, raycastTarget: false)
//   └── DialogueBox (anchored bottom, height 120px)
//         ├── NameTag (Image 배경 + TMP nameText)
//         └── DialogueText (TMP, font size 18)

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mokotan.StandingDialogue
{
    public enum Side
    {
        Left,
        Right
    }

    /// <summary>
    /// RPG 스타일 좌우 스탠딩 대사 연출. 씬당 하나만 배치하는 싱글톤(DontDestroyOnLoad 없음).
    /// </summary>
    public class StandingDialogueManager : MonoBehaviour
    {
        public const float FadeDurationSeconds = 0.3f;
        public const float InactiveOverlayAlpha = 0.6f;

        public static StandingDialogueManager Instance { get; private set; }

        [SerializeField] private Image leftCharImage;
        [SerializeField] private Image rightCharImage;
        [SerializeField] private Image leftOverlay;
        [SerializeField] private Image rightOverlay;
        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text dialogueTextField;
        [SerializeField] private CanvasGroup canvasGroup;

        private bool _waitingForInput;
        private Action _onInputComplete;

        private Coroutine _fadeCharRoutine;
        private Coroutine _fadeAllRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[StandingDialogueManager] Duplicate instance in scene; destroying this component.");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (!_waitingForInput)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                CompleteWaitingInput();
            }
        }

        /// <summary>
        /// 좌 또는 우 슬롯에 캐릭터 스프라이트를 페이드 인으로 표시합니다.
        /// </summary>
        public void ShowCharacter(Side side, Sprite sprite)
        {
            var img = GetCharImage(side);
            if (img == null)
            {
                return;
            }

            EnsureCanvasVisible();
            img.sprite = sprite;
            img.gameObject.SetActive(true);

            if (_fadeCharRoutine != null)
            {
                StopCoroutine(_fadeCharRoutine);
            }

            _fadeCharRoutine = StartCoroutine(FadeImageAlpha(img, 0f, 1f, FadeDurationSeconds));
        }

        /// <summary>
        /// 말하는 쪽은 밝게, 반대쪽은 검은 오버레이로 어둡게 하고 대사를 표시한 뒤 입력을 기다립니다.
        /// </summary>
        public void Speak(Side side, string speakerName, string dialogueText, Action onComplete)
        {
            if (nameText != null)
            {
                nameText.text = speakerName ?? string.Empty;
            }

            if (dialogueTextField != null)
            {
                dialogueTextField.text = dialogueText ?? string.Empty;
            }

            EnsureCanvasVisible();

            ApplySpeakerHighlight(side);

            if (dialogueBox != null)
            {
                dialogueBox.SetActive(true);
            }

            StopWaitingForInput();
            _waitingForInput = true;
            _onInputComplete = onComplete;
        }

        /// <summary>
        /// 슬롯·대사창을 페이드 아웃한 뒤 정리합니다. HideStanding 커맨드는 대기 없이 바로 이어질 수 있습니다.
        /// </summary>
        public void HideAll()
        {
            StopWaitingForInput();

            if (_fadeAllRoutine != null)
            {
                StopCoroutine(_fadeAllRoutine);
            }

            _fadeAllRoutine = StartCoroutine(HideAllRoutine());
        }

        private IEnumerator HideAllRoutine()
        {
            if (canvasGroup != null)
            {
                float elapsed = 0f;
                float startAlpha = canvasGroup.alpha;

                while (elapsed < FadeDurationSeconds)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / FadeDurationSeconds);
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                    yield return null;
                }

                canvasGroup.alpha = 0f;
            }

            if (dialogueBox != null)
            {
                dialogueBox.SetActive(false);
            }

            SetImageAlpha(leftCharImage, 0f);
            SetImageAlpha(rightCharImage, 0f);
            if (leftCharImage != null)
            {
                leftCharImage.sprite = null;
                leftCharImage.gameObject.SetActive(false);
            }

            if (rightCharImage != null)
            {
                rightCharImage.sprite = null;
                rightCharImage.gameObject.SetActive(false);
            }

            SetOverlayAlpha(leftOverlay, 0f);
            SetOverlayAlpha(rightOverlay, 0f);
            _fadeAllRoutine = null;
        }

        private void CompleteWaitingInput()
        {
            _waitingForInput = false;
            var cb = _onInputComplete;
            _onInputComplete = null;
            cb?.Invoke();
        }

        private void StopWaitingForInput()
        {
            _waitingForInput = false;
            _onInputComplete = null;
        }

        private void EnsureCanvasVisible()
        {
            if (canvasGroup != null && canvasGroup.alpha < 1f)
            {
                canvasGroup.alpha = 1f;
            }
        }

        private void ApplySpeakerHighlight(Side speakerSide)
        {
            if (speakerSide == Side.Left)
            {
                SetOverlayAlpha(leftOverlay, 0f);
                SetOverlayAlpha(rightOverlay, InactiveOverlayAlpha);
            }
            else
            {
                SetOverlayAlpha(rightOverlay, 0f);
                SetOverlayAlpha(leftOverlay, InactiveOverlayAlpha);
            }
        }

        private static void SetOverlayAlpha(Image img, float alpha)
        {
            if (img == null)
            {
                return;
            }

            var c = img.color;
            c.a = alpha;
            img.color = c;
        }

        private static void SetImageAlpha(Image img, float alpha)
        {
            if (img == null)
            {
                return;
            }

            var c = img.color;
            c.a = alpha;
            img.color = c;
        }

        private Image GetCharImage(Side side)
        {
            return side == Side.Left ? leftCharImage : rightCharImage;
        }

        private IEnumerator FadeImageAlpha(Image img, float from, float to, float duration)
        {
            if (img == null)
            {
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetImageAlpha(img, Mathf.Lerp(from, to, t));
                yield return null;
            }

            SetImageAlpha(img, to);
            _fadeCharRoutine = null;
        }
    }
}
