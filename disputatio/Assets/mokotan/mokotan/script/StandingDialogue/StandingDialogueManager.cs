// Prefab 구조:
// Canvas (StandingDialogueCanvas) — Screen Space Overlay, Sort Order 10
//   CanvasGroup
//   ├── LeftSlot
//   │     └── LeftCharImage (Image, Preserve Aspect: true)   ← LeftOverlay 삭제해도 됨
//   ├── RightSlot
//   │     └── RightCharImage (Image, Preserve Aspect: true)  ← RightOverlay 삭제해도 됨
//   └── DialogueBox (하단 중앙)
//         ├── NameText (TMP)
//         └── DialogueText (TMP)

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mokotan.StandingDialogue
{
    public enum Side { Left, Right }

    [AddComponentMenu("Mokotan/Standing Dialogue Manager")]
    public class StandingDialogueManager : MonoBehaviour
    {
        public const float FadeDurationSeconds = 0.3f;

        // 비활성 캐릭터에 적용할 밝기 (0=완전검정, 1=원본)
        private static readonly Color ActiveColor   = Color.white;
        private static readonly Color InactiveColor = new Color(0.35f, 0.35f, 0.35f, 1f);

        public static StandingDialogueManager ActiveStandingDialogue { get; set; }

        private static readonly List<StandingDialogueManager> ActiveStandingDialogues =
            new List<StandingDialogueManager>();

        public static StandingDialogueManager Instance
        {
            get
            {
                if (ActiveStandingDialogue != null) return ActiveStandingDialogue;
                return ActiveStandingDialogues.Count > 0 ? ActiveStandingDialogues[0] : null;
            }
        }

        [SerializeField] private Image leftCharImage;
        [SerializeField] private Image rightCharImage;
        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text dialogueTextField;
        [SerializeField] private CanvasGroup canvasGroup;

        private bool _waitingForInput;
        private Action _onInputComplete;
        private Coroutine _fadeAllRoutine;

        private void Awake()
        {
            if (!ActiveStandingDialogues.Contains(this))
                ActiveStandingDialogues.Add(this);
        }

        private void OnDestroy()
        {
            ActiveStandingDialogues.Remove(this);
            if (ActiveStandingDialogue == this) ActiveStandingDialogue = null;
        }

        public static StandingDialogueManager GetStandingDialogue()
        {
            if (ActiveStandingDialogue != null) return ActiveStandingDialogue;

            if (ActiveStandingDialogues.Count > 0)
            {
                ActiveStandingDialogue = ActiveStandingDialogues[0];
                return ActiveStandingDialogue;
            }

            GameObject prefab = Resources.Load<GameObject>("Prefabs/StandingDialogueCanvas");
            if (prefab == null) return null;

            GameObject go = UnityEngine.Object.Instantiate(prefab);
            go.name = "StandingDialogueCanvas";
            go.SetActive(false);
            StandingDialogueManager mgr = go.GetComponent<StandingDialogueManager>();
            if (mgr == null) { UnityEngine.Object.Destroy(go); return null; }

            ActiveStandingDialogue = mgr;
            return mgr;
        }

        private void Update()
        {
            if (!_waitingForInput) return;
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                CompleteWaitingInput();
        }

        // ── 공개 API ──────────────────────────────────────────────

        /// <summary>
        /// 스프라이트 설정 + 하이라이트 + 대사를 한 번에 처리합니다.
        /// speakerSprite / otherSprite가 null 이면 현재 슬롯 이미지를 유지합니다.
        /// </summary>
        public void TalkStanding(Side speakerSide,
            Sprite speakerSprite, Vector2 speakerOffset,
            Sprite otherSprite,  Vector2 otherOffset,
            string speakerName, string dialogueText, Action onComplete)
        {
            EnsureCanvasVisible();

            var speakerImg = GetCharImage(speakerSide);
            var otherImg   = GetCharImage(speakerSide == Side.Left ? Side.Right : Side.Left);

            if (speakerSprite != null && speakerImg != null)
            {
                speakerImg.sprite = speakerSprite;
                speakerImg.gameObject.SetActive(true);
            }
            if (speakerImg != null) ApplyOffset(speakerImg, speakerOffset);

            if (otherSprite != null && otherImg != null)
            {
                otherImg.sprite = otherSprite;
                otherImg.gameObject.SetActive(true);
            }
            if (otherImg != null) ApplyOffset(otherImg, otherOffset);

            Speak(speakerSide, speakerName, dialogueText, onComplete);
        }

        /// <summary>
        /// 하이라이트 + 대사 표시. 스프라이트는 이미 로드된 상태여야 합니다.
        /// </summary>
        public void Speak(Side side, string speakerName, string dialogueText, Action onComplete)
        {
            EnsureCanvasVisible();

            if (nameText != null)       nameText.text       = speakerName  ?? string.Empty;
            if (dialogueTextField != null) dialogueTextField.text = dialogueText ?? string.Empty;
            if (dialogueBox != null)    dialogueBox.SetActive(true);

            ApplySpeakerHighlight(side);

            StopWaitingForInput();
            _waitingForInput  = true;
            _onInputComplete  = onComplete;
        }

        /// <summary>
        /// 전체 연출을 페이드 아웃하고 정리합니다.
        /// </summary>
        public void HideAll()
        {
            StopWaitingForInput();
            if (_fadeAllRoutine != null) StopCoroutine(_fadeAllRoutine);
            _fadeAllRoutine = StartCoroutine(HideAllRoutine());
        }

        public void ShowCharacter(Side side, Sprite sprite)
        {
            var img = GetCharImage(side);
            if (img == null) return;
            EnsureCanvasVisible();
            img.sprite = sprite;
            img.color  = ActiveColor;
            img.gameObject.SetActive(true);
        }

        // ── 내부 ──────────────────────────────────────────────────

        private void ApplySpeakerHighlight(Side speakerSide)
        {
            var speakerImg = GetCharImage(speakerSide);
            var otherImg   = GetCharImage(speakerSide == Side.Left ? Side.Right : Side.Left);

            // 스프라이트가 있는 Image에만 색상을 적용 — 빈 슬롯은 건드리지 않음
            if (speakerImg != null && speakerImg.sprite != null)
                speakerImg.color = ActiveColor;

            if (otherImg != null && otherImg.sprite != null)
                otherImg.color = InactiveColor;
        }

        private IEnumerator HideAllRoutine()
        {
            if (canvasGroup != null)
            {
                float elapsed = 0f;
                float start   = canvasGroup.alpha;
                while (elapsed < FadeDurationSeconds)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(start, 0f, Mathf.Clamp01(elapsed / FadeDurationSeconds));
                    yield return null;
                }
                canvasGroup.alpha = 0f;
            }

            if (dialogueBox != null) dialogueBox.SetActive(false);

            ResetSlot(leftCharImage);
            ResetSlot(rightCharImage);
            _fadeAllRoutine = null;
        }

        private static void ResetSlot(Image img)
        {
            if (img == null) return;
            img.sprite = null;
            img.color  = ActiveColor;
            img.gameObject.SetActive(false);
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
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (canvasGroup != null && canvasGroup.alpha < 1f) canvasGroup.alpha = 1f;
        }

        private Image GetCharImage(Side side) =>
            side == Side.Left ? leftCharImage : rightCharImage;

        private static void ApplyOffset(Image img, Vector2 offset)
        {
            img.rectTransform.anchoredPosition = offset;
        }
    }
}
