#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
internal static class DraggableSnap2DPlayModeGate
{
    static DraggableSnap2DPlayModeGate()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            DraggableSnap2D.EditorResetPlayModePrefGate();
    }
}
#endif
