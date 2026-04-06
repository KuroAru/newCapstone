using System.Collections.Generic;
using UnityEngine;
using Fungus;
using UnityEngine.UI;

/// <summary>
/// 🎨 저장 슬롯 이미지 변경 스크립트.
/// 이제 "SceneName" 문자열 변수를 기준으로 씬 썸네일 이미지를 변경.
/// (예: Opening_Office, Hall_Left 등)
/// <para>저장 슬롯 PNG 미리보기는 정적 스택 최상단 스프라이트(또는 SceneName 동일 매핑)를 우선 사용합니다.</para>
/// </summary>
public class ChangeSP : MonoBehaviour
{
    /// <summary>세이브 썸네일에 쓸 현재 슬롯용 스프라이트(스택). <see cref="OnChangeButtonImage"/>에서 갱신됩니다.</summary>
    static readonly Stack<Sprite> SaveThumbnailStack = new Stack<Sprite>();

    /// <summary>스택 최상단을 교체합니다(깊이는 1로 유지해 최신 SceneName 썸네일만 보관).</summary>
    public static void SetSaveThumbnailStackTop(Sprite sprite)
    {
        SaveThumbnailStack.Clear();
        if (sprite != null)
            SaveThumbnailStack.Push(sprite);
    }

    public static bool TryPeekSaveThumbnailSprite(out Sprite sprite)
    {
        if (SaveThumbnailStack.Count == 0)
        {
            sprite = null;
            return false;
        }
        sprite = SaveThumbnailStack.Peek();
        return sprite != null;
    }

    public static ChangeSP FindCatalog()
    {
        return Object.FindFirstObjectByType<ChangeSP>(FindObjectsInactive.Include);
    }

    [Header("Fungus 연동")]
    public Flowchart flowchart;

    [Header("UI 요소")]
    public Button slot;

    [Header("씬별 슬롯 이미지 리스트 (씬 순서에 맞게 지정)")]
    public Sprite[] sprite;

    [Header("기본 썸네일 (일치하지 않을 때 표시)")]
    public Sprite defaultSprite;

    /// <summary>
    /// 저장 슬롯 이미지 변경 (저장 시 호출)
    /// </summary>
    public void OnChangeButtonImage()
    {
        if (flowchart == null || slot == null || sprite == null || sprite.Length == 0)
        {
            Debug.LogWarning("ChangeSP: 필수 요소가 연결되지 않았습니다.");
            return;
        }

        // 🔹 SavePointKey 대신 SceneName 사용
        string sceneName = flowchart.GetStringVariable("SceneName");
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("ChangeSP: Flowchart의 SceneName 변수가 비어 있습니다.");
            if (defaultSprite != null)
            {
                slot.image.sprite = defaultSprite;
                SetSaveThumbnailStackTop(defaultSprite);
            }
            return;
        }

        Debug.Log($"🖼️ ChangeSP: 현재 씬 이름 = {sceneName}");

        // 🔹 씬 이름에 따라 슬롯 이미지 변경
        Sprite targetSprite = GetSpriteForScene(sceneName);

        if (targetSprite != null)
        {
            slot.image.sprite = targetSprite;
            SetSaveThumbnailStackTop(targetSprite);
        }
        else
        {
            Debug.LogWarning($"ChangeSP: {sceneName} 에 해당하는 스프라이트가 없습니다.");
            if (defaultSprite != null)
            {
                slot.image.sprite = defaultSprite;
                SetSaveThumbnailStackTop(defaultSprite);
            }
        }
    }

    /// <summary>저장 미리보기·인코더에서 사용: 씬 이름에 대응하는 슬롯 썸네일 스프라이트.</summary>
    public Sprite GetThumbnailSpriteForSceneName(string sceneName)
    {
        return GetSpriteForScene(sceneName);
    }

    /// <summary>
    /// 🔸 씬 이름(SceneName)에 따라 Sprite 반환
    /// </summary>
    private Sprite GetSpriteForScene(string name)
    {
        switch (name)
        {
            case "Opening_Office": return sprite.Length > 0 ? sprite[0] : null;
            case "Opening_Mention": return sprite.Length > 1 ? sprite[1] : null;
            case "Opening_Mention _open": return sprite.Length > 2 ? sprite[2] : null;
            case "Hall_playerble": return sprite.Length > 3 ? sprite[3] : null;
            case "Hall_Left": return sprite.Length > 4 ? sprite[4] : null;
            case "Hall_Left2": return sprite.Length > 5 ? sprite[5] : null;
            case "Kitchen": return sprite.Length > 6 ? sprite[6] : null;
            case "UtilityRoom": return sprite.Length > 7 ? sprite[7] : null;
            case "Hallway_Left": return sprite.Length > 8 ? sprite[8] : null;
            case "Hallway_Left2": return sprite.Length > 9 ? sprite[9] : null;
            case "Hall_Right": return sprite.Length > 10 ? sprite[10] : null;
            case "Hall_Right2": return sprite.Length > 11 ? sprite[11] : null;
            case "Hall_RightCross": return sprite.Length > 12 ? sprite[12] : null;
            case "MaidEntrance": return sprite.Length > 13 ? sprite[13] : null;
            case "MaidRoom": return sprite.Length > 14 ? sprite[14] : null;
            case "StudyEntrance": return sprite.Length > 15 ? sprite[15] : null;
            case "StudyRoom": return sprite.Length > 16 ? sprite[16] : null;
            case "BookCase1": return sprite.Length > 17 ? sprite[17] : null;
            case "BookCase2": return sprite.Length > 18 ? sprite[18] : null;
            case "BookCase2Back": return sprite.Length > 19 ? sprite[19] : null;
            case "BookCase3": return sprite.Length > 20 ? sprite[20] : null;
            case "BookCase4": return sprite.Length > 21 ? sprite[21] : null;
            case "PrisonEntrance": return sprite.Length > 22 ? sprite[22] : null;
            case "Prison": return sprite.Length > 23 ? sprite[23] : null;
            case "Hallway_Right": return sprite.Length > 24 ? sprite[24] : null;
            case "Hallway_Right2": return sprite.Length > 25 ? sprite[25] : null;
            case "2floorMainHall": return sprite.Length > 26 ? sprite[26] : null;
            case "2floorLeft": return sprite.Length > 27 ? sprite[27] : null;
            case "2floorLeftCross": return sprite.Length > 28 ? sprite[28] : null;
            case "TutorEntrance": return sprite.Length > 29 ? sprite[29] : null;
            case "TutorRoom": return sprite.Length > 30 ? sprite[30] : null;
            case "ChildEntrance": return sprite.Length > 31 ? sprite[31] : null;
            case "ChildRoom": return sprite.Length > 32 ? sprite[32] : null;
            case "2floorHallway_Left": return sprite.Length > 33 ? sprite[33] : null;
            case "2floorRight": return sprite.Length > 34 ? sprite[34] : null;
            case "2floorRightCross": return sprite.Length > 35 ? sprite[35] : null;
            case "BedEntrance": return sprite.Length > 36 ? sprite[36] : null;
            case "BedRoom": return sprite.Length > 37 ? sprite[37] : null;
            case "WifeEntrance": return sprite.Length > 38 ? sprite[38] : null;
            case "WifeRoom": return sprite.Length > 39 ? sprite[39] : null;
            case "DressingRoom": return sprite.Length > 40 ? sprite[40] : null;
            case "2floorHallway_Right": return sprite.Length > 41 ? sprite[41] : null;
        }
        return null;
    }
}
