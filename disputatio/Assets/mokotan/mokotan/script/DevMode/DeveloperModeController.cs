using UnityEngine;
using Fungus;

public class DeveloperModeController : MonoBehaviour
{
    private const string DevModeVariableKey = "DevModeEnabled";

    [Header("Toggle Keys")]
    [SerializeField] private KeyCode toggleDevModeKey = KeyCode.F2;
    [SerializeField] private KeyCode toggleOverlayKey = KeyCode.F3;
    [SerializeField] private KeyCode quickRestartKey = KeyCode.F5;
    [SerializeField] private KeyCode skipOpeningKey = KeyCode.F6;

    [Header("Services")]
    [SerializeField] private QuickRestartService quickRestartService;
    [SerializeField] private OpeningSkipService openingSkipService;
    [SerializeField] private InGameDeveloperOverlay developerOverlay;

    private static DeveloperModeController instance;

    public static bool IsDeveloperModeEnabled { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        IsDeveloperModeEnabled = Debug.isDebugBuild && ReadDevModeFromVariableManager();
    }

    private void OnEnable()
    {
        JumpscareManager.OnPlayerDied += HandlePlayerDied;
        SpecialJumpscareManager.OnPlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        JumpscareManager.OnPlayerDied -= HandlePlayerDied;
        SpecialJumpscareManager.OnPlayerDied -= HandlePlayerDied;
    }

    private void Start()
    {
        if (developerOverlay == null)
            developerOverlay = FindFirstObjectByType<InGameDeveloperOverlay>(FindObjectsInactive.Include);
        if (developerOverlay == null)
        {
            var overlayObject = new GameObject("InGameDeveloperOverlay");
            developerOverlay = overlayObject.AddComponent<InGameDeveloperOverlay>();
            DontDestroyOnLoad(overlayObject);
        }

        if (quickRestartService == null)
            quickRestartService = GetComponent<QuickRestartService>() ?? gameObject.AddComponent<QuickRestartService>();
        if (openingSkipService == null)
            openingSkipService = GetComponent<OpeningSkipService>() ?? gameObject.AddComponent<OpeningSkipService>();
        if (developerOverlay != null)
            developerOverlay.SetVisible(IsDeveloperModeEnabled);
    }

    private void Update()
    {
        if (!Debug.isDebugBuild)
            return;

        if (Input.GetKeyDown(toggleDevModeKey))
        {
            SetDeveloperModeEnabled(!IsDeveloperModeEnabled);
        }

        if (!IsDeveloperModeEnabled)
            return;

        if (Input.GetKeyDown(toggleOverlayKey) && developerOverlay != null)
            developerOverlay.ToggleVisible();
        if (Input.GetKeyDown(quickRestartKey))
            quickRestartService.TriggerRestart();
        if (Input.GetKeyDown(skipOpeningKey))
            openingSkipService.SkipOpening();
    }

    private void HandlePlayerDied()
    {
        if (!IsDeveloperModeEnabled)
            return;

        quickRestartService.TriggerRestart();
    }

    public void RequestSkipOpening()
    {
        if (IsDeveloperModeEnabled)
            openingSkipService.SkipOpening();
    }

    public void RequestQuickRestart()
    {
        if (IsDeveloperModeEnabled)
            quickRestartService.TriggerRestart();
    }

    private void SetDeveloperModeEnabled(bool enabled)
    {
        IsDeveloperModeEnabled = enabled;
        WriteDevModeToVariableManager(enabled);

        if (developerOverlay != null)
            developerOverlay.SetVisible(enabled);
    }

    private static bool ReadDevModeFromVariableManager()
    {
        Flowchart flowchart = FlowchartLocator.Find();
        if (flowchart == null)
            return false;

        EnsureDevModeVariableExists(flowchart);
        return flowchart.GetBooleanVariable(DevModeVariableKey);
    }

    private static void WriteDevModeToVariableManager(bool enabled)
    {
        Flowchart flowchart = FlowchartLocator.Find();
        if (flowchart == null)
            return;

        EnsureDevModeVariableExists(flowchart);
        flowchart.SetBooleanVariable(DevModeVariableKey, enabled);
    }

    private static void EnsureDevModeVariableExists(Flowchart flowchart)
    {
        if (flowchart == null || flowchart.HasVariable(DevModeVariableKey))
            return;

        var variable = flowchart.gameObject.AddComponent<BooleanVariable>();
        variable.Key = DevModeVariableKey;
        variable.Scope = VariableScope.Public;
        variable.Value = false;
        flowchart.Variables.Add(variable);
        Debug.Log($"[DeveloperModeController] Variablemanager에 bool 변수 '{DevModeVariableKey}'를 추가했습니다.");
    }
}
