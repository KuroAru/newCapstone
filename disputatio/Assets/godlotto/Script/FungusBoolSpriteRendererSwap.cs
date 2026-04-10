using Fungus;
using UnityEngine;

/// <summary>
/// Fungus bool(예: UsedStudyKey)이 true가 되면 <see cref="SpriteRenderer"/>의 스프라이트를 교체합니다.
/// 씬 재진입 시 이미 true인 경우와, 같은 씬에서 열쇠 사용 직후 모두 처리합니다.
/// </summary>
public class FungusBoolSpriteRendererSwap : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("비우면 FlowchartLocator(Variablemanager)를 사용합니다.")]
    [SerializeField] private Flowchart flowchartOverride;
    [SerializeField] private string fungusBooleanKey = "UsedStudyKey";
    [SerializeField] private Sprite spriteWhenTrue;
    [Tooltip("Variablemanager에 키가 없을 때 Fungus GlobalVariables도 조회합니다.")]
    [SerializeField] private bool checkGlobalVariables = true;

    private bool _unlockedApplied;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ApplyUnlockSpriteIfNeeded();
        enabled = !_unlockedApplied && spriteWhenTrue != null;
    }

    private void Update()
    {
        if (_unlockedApplied)
        {
            enabled = false;
            return;
        }

        ApplyUnlockSpriteIfNeeded();
        if (_unlockedApplied)
            enabled = false;
    }

    /// <summary>UnityEvent 등에서 즉시 반영할 때 호출합니다.</summary>
    public void RefreshFromFungus()
    {
        ApplyUnlockSpriteIfNeeded();
    }

    private void ApplyUnlockSpriteIfNeeded()
    {
        if (spriteWhenTrue == null || spriteRenderer == null)
            return;

        if (!IsBoolTrue())
            return;

        spriteRenderer.sprite = spriteWhenTrue;
        _unlockedApplied = true;
    }

    private bool IsBoolTrue()
    {
        Flowchart fc = FlowchartLocator.Resolve(flowchartOverride);
        if (fc != null && fc.GetBooleanVariable(fungusBooleanKey))
            return true;

        if (checkGlobalVariables && FlowchartLocator.GetFungusGlobalBoolean(fungusBooleanKey))
            return true;

        return false;
    }
}
