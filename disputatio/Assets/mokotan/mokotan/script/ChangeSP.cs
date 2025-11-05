using UnityEngine;
using Fungus;
using UnityEngine.UI;

/// <summary>
/// 저장 슬롯 이미지 변경 스크립트.
/// Fungus Flowchart의 "CurrentScene" 문자열 변수에 따라 슬롯 이미지(sprite)를 교체.
/// </summary>
public class ChangeSP : MonoBehaviour
{
    [Header("Fungus 연동")]
    public Flowchart flowchart;

    [Header("UI 요소")]
    public Button slot;

    [Header("씬별 슬롯 이미지 리스트 (씬 순서에 맞게 지정)")]
    public Sprite[] sprite;

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

        string sceneName = flowchart.GetStringVariable("CurrentScene");
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("ChangeSP: Flowchart의 CurrentScene 변수가 비어 있습니다.");
            return;
        }

        Debug.Log($"🖼️ ChangeSP: 현재 씬 이름 = {sceneName}");

        // 씬 이름에 따라 슬롯 이미지 변경
        switch (sceneName)
        {
            case "Opening_Office":
                slot.image.sprite = sprite[0];
                break;

            case "Opening_Mention":
                slot.image.sprite = sprite[1];
                break;

            case "Opening_Mention _open":
                slot.image.sprite = sprite[2];
                break;

            case "Hall_playerble":
                slot.image.sprite = sprite[3];
                break;

            case "Hall_Left":
                slot.image.sprite = sprite[4];
                break;

            case "Hall_Left2":
                slot.image.sprite = sprite[5];
                break;

            case "Kitchen":
                slot.image.sprite = sprite[6];
                break;

            case "UtilityRoom":
                slot.image.sprite = sprite[7];
                break;
            
            case "Hallway_Left":
                slot.image.sprite = sprite[8];
                break;

            case "Hallway_Left2":
                slot.image.sprite = sprite[9];
                break;

            case "Hall_Right":
                slot.image.sprite = sprite[10];
                break;

            case "Hall_Right2":
                slot.image.sprite = sprite[11];
                break;

            case "Hall_RightCross":
                slot.image.sprite = sprite[12];
                break;

            case "MaidEntrance":
                slot.image.sprite = sprite[13];
                break;

            case "MaidRoom":
                slot.image.sprite = sprite[14];
                break;

            case "StudyEntrance":
                slot.image.sprite = sprite[15];
                break;

            case "StudyRoom":
                slot.image.sprite = sprite[16];
                break;

            case "BookCase1":
                slot.image.sprite = sprite[17];
                break;

            case "BookCase2":
                slot.image.sprite = sprite[18];
                break;

            case "BookCase2Back":
                slot.image.sprite = sprite[19];
                break;

            case "BookCase3":
                slot.image.sprite = sprite[20];
                break;

            case "BookCase4":
                slot.image.sprite = sprite[21];
                break;

            case "PrisonEntrance":
                slot.image.sprite = sprite[22];
                break;

            case "Prison":
                slot.image.sprite = sprite[23];
                break;

            case "Hallway_Right":
                slot.image.sprite = sprite[24];
                break;

            case "Hallway_Right2":
                slot.image.sprite = sprite[25];
                break;

            case "2floorMainHall":
                slot.image.sprite = sprite[26];
                break;

            case "2floorLeft":
                slot.image.sprite = sprite[27];
                break;

            case "2floorLeftCross":
                slot.image.sprite = sprite[28];
                break;

            case "TutorEntrance":
                slot.image.sprite = sprite[29];
                break;

            case "TutorRoom":
                slot.image.sprite = sprite[30];
                break;

            case "ChildEntrance":
                slot.image.sprite = sprite[31];
                break;

            case "ChildRoom":
                slot.image.sprite = sprite[32];
                break;

            case "2floorHallway_Left":
                slot.image.sprite = sprite[33];
                break;

            case "2floorRight":
                slot.image.sprite = sprite[34];
                break;

            case "2floorRightCross":
                slot.image.sprite = sprite[35];
                break;

            case "BedEntrance":
                slot.image.sprite = sprite[36];
                break;

            case "BedRoom":
                slot.image.sprite = sprite[37];
                break;

            case "WifeEntrance":
                slot.image.sprite = sprite[38];
                break;

            case "WifeRoom":
                slot.image.sprite = sprite[39];
                break;

            case "DressingRoom":
                slot.image.sprite = sprite[40];
                break;

            case "2floorHallway_Right":
                slot.image.sprite = sprite[41];
                break;

            default:
                Debug.LogWarning($"ChangeSP: {sceneName} 에 해당하는 스프라이트가 없습니다.");
                break;
        }
    }
}
