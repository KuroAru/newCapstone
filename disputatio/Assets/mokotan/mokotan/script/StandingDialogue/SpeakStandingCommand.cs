using Mokotan.StandingDialogue;
using UnityEngine;

namespace Fungus
{
    [CommandInfo("StandingDialogue",
        "Speak Standing",
        "해당 캐릭터가 말하고 클릭/스페이스바 대기")]
    [AddComponentMenu("")]
    public class SpeakStandingCommand : Command
    {
        [SerializeField] private Side side = Side.Left;
        [SerializeField] private string speakerName = "";
        [TextArea(3, 6)]
        [SerializeField] private string dialogueText = "";

        public override void OnEnter()
        {
            var mgr = StandingDialogueManager.Instance;
            if (mgr == null)
            {
                Continue();
                return;
            }

            mgr.Speak(side, speakerName, dialogueText, () => Continue());
        }

        public override string GetSummary()
        {
            var name = string.IsNullOrEmpty(speakerName) ? "..." : speakerName;
            return side + ": " + name;
        }

        public override Color GetButtonColor()
        {
            return new Color32(180, 230, 200, 255);
        }
    }
}
