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
        public override void OnEnter()
        {
            var mgr = StandingDialogueManager.Instance;
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
