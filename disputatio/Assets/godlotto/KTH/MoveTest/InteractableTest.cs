using UnityEngine;

public class InteractableTest : MonoBehaviour
{
    // 나중에 다른 상호작용(아이템 줍기 등)을 만들 때 상속해서 쓰면 좋습니다.
    public virtual void InteractTest()
    {
        GameLog.Log(gameObject.name + "와 접촉하여 상호작용을 시작합니다!");
        // 여기에 원하는 로직(UI 띄우기, 점수 획득 등)을 작성하세요.
    }
}