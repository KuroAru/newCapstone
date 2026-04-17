using Mokotan.StandingDialogue;
using TMPro;
using UnityEngine;

namespace Fungus
{
    [CommandInfo("StandingDialogue",
        "Talk Standing",
        "캐릭터 스프라이트 설정 + 하이라이트 + 대사 표시를 한 번에 처리합니다.")]
    [AddComponentMenu("")]
    public class TalkStandingCommand : Command
    {
        [Tooltip("이 커맨드에서 사용할 스탠딩 UI. 비우면 씬의 첫 인스턴스를 사용합니다.")]
        [SerializeField] private StandingDialogueManager setStandingDialogue;

        [Header("말하는 캐릭터")]
        [Tooltip("Left = 왼쪽, Right = 오른쪽")]
        [SerializeField] private Side speakerSide = Side.Left;

        [Tooltip("화자 이름 (대사창 이름 칸에 표시됩니다)")]
        [SerializeField] private string speakerName = "";

        [Tooltip("대사 내용")]
        [TextArea(3, 6)]
        [SerializeField] private string dialogueText = "";

        [Tooltip("화자 스탠딩 이미지. 비우면 현재 슬롯 이미지를 유지합니다.")]
        [SerializeField] private Sprite speakerSprite;

        [Tooltip("화자 이미지 위치 오프셋 (X=좌우, Y=상하).")]
        [SerializeField] private Vector2 speakerOffset = Vector2.zero;

        [Header("상대 캐릭터 (선택)")]
        [Tooltip("반대편 캐릭터 스탠딩 이미지. 비우면 현재 슬롯 이미지를 유지합니다.")]
        [SerializeField] private Sprite otherSprite;

        [Tooltip("상대 이미지 위치 오프셋 (X=좌우, Y=상하).")]
        [SerializeField] private Vector2 otherOffset = Vector2.zero;

        [Header("타이포그래피")]
        [Tooltip("대사 폰트. 비우면 프리팹 기본 폰트(JalnanGothic SDF) 유지.")]
        [SerializeField] private TMP_FontAsset font;

        [Tooltip("대사 글자 크기. 0이면 프리팹 기본값 유지.")]
        [SerializeField] private float fontSize = 0f;

        [Tooltip("초당 출력 글자 수 (타이핑 효과). 0이면 즉시 전체 출력.")]
        [SerializeField] private float charsPerSecond = 0f;

        public override void OnEnter()
        {
            if (setStandingDialogue != null)
                StandingDialogueManager.ActiveStandingDialogue = setStandingDialogue;

            StandingDialogueManager mgr = StandingDialogueManager.GetStandingDialogue();
            if (mgr == null) { Continue(); return; }

            var typography = new TypographySettings
            {
                Font           = font,
                FontSize       = fontSize,
                CharsPerSecond = charsPerSecond,
            };

            mgr.TalkStanding(speakerSide,
                speakerSprite, speakerOffset,
                otherSprite,   otherOffset,
                speakerName,   dialogueText,
                typography,
                () => Continue());
        }

        public override string GetSummary()
        {
            if (string.IsNullOrEmpty(speakerName) && string.IsNullOrEmpty(dialogueText))
                return "(설정 없음)";
            var name    = string.IsNullOrEmpty(speakerName) ? "?" : speakerName;
            var preview = dialogueText?.Length > 20 ? dialogueText.Substring(0, 20) + "..." : dialogueText;
            var speed   = charsPerSecond > 0f ? $" | {charsPerSecond}자/초" : "";
            return $"[{speakerSide}] {name}: \"{preview}\"{speed}";
        }

        public override Color GetButtonColor() => new Color32(255, 220, 150, 255);
    }
}
