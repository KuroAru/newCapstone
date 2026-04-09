using UnityEngine;

public class InGameDeveloperOverlay : MonoBehaviour
{
    [SerializeField] private bool visible = true;
    [SerializeField] private Rect windowRect = new Rect(24f, 24f, 480f, 320f);

    private HeuristicDebugSnapshot latestSnapshot;
    private Vector2 scrollPosition;

    private void OnEnable()
    {
        PromptInfoBudgetComposer.OnSnapshotUpdated += HandleSnapshotUpdated;
    }

    private void OnDisable()
    {
        PromptInfoBudgetComposer.OnSnapshotUpdated -= HandleSnapshotUpdated;
    }

    public void ToggleVisible()
    {
        visible = !visible;
    }

    public void SetVisible(bool isVisible)
    {
        visible = isVisible;
    }

    private void HandleSnapshotUpdated(HeuristicDebugSnapshot snapshot)
    {
        latestSnapshot = snapshot;
    }

    private void OnGUI()
    {
        if (!visible || !DeveloperModeController.IsDeveloperModeEnabled)
            return;

        windowRect = GUILayout.Window(GetInstanceID(), windowRect, DrawWindow, "Developer Mode Console");
    }

    private void DrawWindow(int id)
    {
        GUILayout.Label($"DevMode: {(DeveloperModeController.IsDeveloperModeEnabled ? "ON" : "OFF")}");
        GUILayout.Space(4f);

        if (latestSnapshot == null)
        {
            GUILayout.Label("Heuristic snapshot: 아직 없음");
        }
        else
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(180f));
            GUILayout.Label($"Room: {latestSnapshot.roomName}");
            GUILayout.Label($"Level: {latestSnapshot.level}");
            GUILayout.Label($"Skill: {latestSnapshot.skillScore:0.000}");
            GUILayout.Label($"Progress: {latestSnapshot.progressScore:0.000}");
            GUILayout.Label($"Accuracy: {latestSnapshot.accuracyScore:0.000}");
            GUILayout.Label($"Stuck: {latestSnapshot.stuckScore:0.000}");
            GUILayout.Label($"RevisitCount: {latestSnapshot.unsolvedRevisitCount}");
            GUILayout.Label($"RevisitIntervalSec: {latestSnapshot.revisitIntervalSeconds:0.0}");
            GUILayout.Label($"NoProgressAfterRevisit: {latestSnapshot.noProgressAfterRevisitCount}");
            GUILayout.Label($"Reason: {latestSnapshot.reason}");
            GUILayout.Label($"GeneratedAt: {latestSnapshot.generatedAtUtc}");
            GUILayout.EndScrollView();
        }

        GUILayout.Space(6f);
        GUILayout.Label("F5: Quick Restart   F6: Opening Skip");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Quick Restart"))
        {
            FindFirstObjectByType<DeveloperModeController>()?.RequestQuickRestart();
        }

        if (GUILayout.Button("Skip Opening"))
        {
            FindFirstObjectByType<DeveloperModeController>()?.RequestSkipOpening();
        }
        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }
}
