using Mokotan.StandingDialogue;
using UnityEngine;

namespace Fungus
{
    [CommandInfo("StandingDialogue",
        "Show Standing",
        "캐릭터 스프라이트를 좌/우 슬롯에 표시합니다")]
    [AddComponentMenu("")]
    public class ShowStandingCommand : Command
    {
        [SerializeField] private Side side = Side.Left;
        [SerializeField] private Sprite characterSprite;

        public override void OnEnter()
        {
            var mgr = StandingDialogueManager.Instance;
            if (mgr != null)
            {
                mgr.ShowCharacter(side, characterSprite);
            }

            Continue();
        }

        public override string GetSummary()
        {
            var spriteName = characterSprite != null ? characterSprite.name : "(none)";
            return side + ": " + spriteName;
        }

        public override Color GetButtonColor()
        {
            return new Color32(200, 220, 255, 255);
        }
    }
}
