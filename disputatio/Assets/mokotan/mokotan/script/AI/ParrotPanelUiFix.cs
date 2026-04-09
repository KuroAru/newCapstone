using UnityEngine;
using Fungus;
using UnityEngine.UI;

/// <summary>
/// Hall 등에서 Parret 패널이 켜진 뒤에도 Fungus SayDialog(Overlay)의 CanvasGroup이
/// blocksRaycasts=true로 남아 있으면 그 아래 Screen Space Camera UI(입력 필드)가 클릭을 받지 못합니다.
/// 패널이 열릴 때 한 번 레이캐스트 차단을 끄고, Parret 오브젝트를 임시로 숨깁니다.
/// </summary>
[DisallowMultipleComponent]
public class ParrotPanelUiFix : MonoBehaviour
{
    private const string ElectricOnVariableName = "ElectricOn";

    [SerializeField] private SayDialog targetSayDialog;
    [SerializeField] private CanvasGroup sayDialogCanvasGroup;
    [SerializeField] private Image panelBackgroundImage;
    [SerializeField] private Sprite electricOnBackgroundSprite;
    [SerializeField] private string electricOnVariableName = ElectricOnVariableName;

    [Header("Parret Visibility Settings")]
    [Tooltip("자동으로 찾을 Parret 오브젝트의 이름입니다.")]
    [SerializeField] private string parretObjectName = "Parret";
    
    [Tooltip("자동으로 찾은 Parret 오브젝트가 여기에 캐싱됩니다. 미리 인스펙터에서 할당해둘 수도 있습니다.")]
    [SerializeField] private GameObject targetParret;

    private Sprite defaultBackgroundSprite;

    private void Awake()
    {
        if (sayDialogCanvasGroup == null && targetSayDialog != null)
            sayDialogCanvasGroup = targetSayDialog.GetComponent<CanvasGroup>();
        
        if (sayDialogCanvasGroup == null)
        {
            var sd = FindFirstObjectByType<SayDialog>();
            if (sd != null)
                sayDialogCanvasGroup = sd.GetComponent<CanvasGroup>();
        }

        if (panelBackgroundImage == null)
            panelBackgroundImage = GetComponent<Image>();

        if (panelBackgroundImage != null)
            defaultBackgroundSprite = panelBackgroundImage.sprite;
    }

    private void OnEnable()
    {
        // 1. 기존 레이캐스트 차단 해제 로직
        if (sayDialogCanvasGroup != null)
        {
            sayDialogCanvasGroup.blocksRaycasts = false;
            sayDialogCanvasGroup.interactable = false;
        }

        // 2. Parret 오브젝트 자동 탐색
        if (targetParret == null)
        {
            targetParret = GameObject.Find(parretObjectName);
            
            // 만약 스크립트로 프리팹을 생성(Instantiate)해서 이름 뒤에 "(Clone)"이 붙는다면 
            // 아래 주석을 해제하여 사용할 수 있습니다.
            // if (targetParret == null)
            //     targetParret = GameObject.Find(parretObjectName + "(Clone)");
        }

        // 3. Parret 임시 비활성화
        if (targetParret != null)
        {
            targetParret.SetActive(false);
        }

        ApplyBackgroundSprite();
    }

    private void OnDisable()
    {
        // 패널이 닫힐 때 Parret 다시 활성화
        if (targetParret != null)
        {
            targetParret.SetActive(true);
        }
    }

    private void ApplyBackgroundSprite()
    {
        if (panelBackgroundImage == null)
            return;

        bool isElectricOn = IsElectricOn();
        panelBackgroundImage.sprite = ChoosePanelBackground(
            defaultBackgroundSprite,
            electricOnBackgroundSprite,
            isElectricOn);
    }

    private bool IsElectricOn()
    {
        Flowchart flowchart = FlowchartLocator.Find();
        if (flowchart == null)
            return false;

        string key = string.IsNullOrWhiteSpace(electricOnVariableName)
            ? ElectricOnVariableName
            : electricOnVariableName;
        return flowchart.GetBooleanVariable(key);
    }

    public static Sprite ChoosePanelBackground(Sprite defaultSprite, Sprite electricOnSprite, bool isElectricOn)
    {
        if (!isElectricOn || electricOnSprite == null)
            return defaultSprite;

        return electricOnSprite;
    }
}