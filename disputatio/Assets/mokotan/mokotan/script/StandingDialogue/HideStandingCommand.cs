using Mokotan.StandingDialogue;
using UnityEngine;

namespace Fungus
{
    [CommandInfo("StandingDialogue",
        "Hide Standing",
        "스탠딩 연출 전체를 종료합니다")]
    [AddComponentMenu("")]
    public class HideStandingCommand : Command
    {
        [Tooltip("이 커맨드에서 숨길 스탠딩 UI. 비우면 Set Standing Dialog 또는 씬의 첫 인스턴스·Resources를 사용합니다.")]
        [SerializeField] private StandingDialogueManager setStandingDialogue;

        public override void OnEnter()
        {
            if (setStandingDialogue != null)
            {
                StandingDialogueManager.ActiveStandingDialogue = setStandingDialogue;
            }

            StandingDialogueManager mgr = StandingDialogueManager.GetStandingDialogue();
            if (mgr != null)
            {
                mgr.HideAll();
            }

            Continue();
        }

        public override string GetSummary()
        {
            return "Hide standing UI";
        }

        public override Color GetButtonColor()
        {
            return new Color32(220, 200, 200, 255);
        }
    }
}
