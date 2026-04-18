using Mokotan.StandingDialogue;
using UnityEngine;

namespace Fungus
{
    /// <summary>
    /// Say의 Set Say Dialog와 같이, 이후 StandingDialogue 커맨드가 사용할 스탠딩 UI를 지정합니다.
    /// </summary>
    [CommandInfo("StandingDialogue",
        "Set Standing Dialog",
        "스탠딩 대사 UI를 지정합니다 (Say의 Set Say Dialog와 동일)")]
    [AddComponentMenu("")]
    public class SetStandingDialogCommand : Command
    {
        [Tooltip("이후 Show/Speak/Hide Standing 커맨드가 이 인스턴스를 사용합니다.")]
        [SerializeField] private StandingDialogueManager standingDialogue;

        public override void OnEnter()
        {
            if (standingDialogue != null)
            {
                StandingDialogueManager.ActiveStandingDialogue = standingDialogue;
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (standingDialogue == null)
            {
                return "Error: No Standing Dialogue assigned";
            }

            return standingDialogue.name;
        }

        public override Color GetButtonColor()
        {
            return new Color32(184, 210, 235, 255);
        }
    }
}
