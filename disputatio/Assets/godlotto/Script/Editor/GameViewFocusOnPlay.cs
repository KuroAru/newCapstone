#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 플레이 직후 Game 뷰가 포커스를 받지 않으면 마우스 입력이 Inspector/Scene 등으로 가서
/// "게임 안에서만 있으면 클릭이 안 되고, 다른 곳을 한 번 누른 뒤에는 된다" 같은 현상이 납니다.
/// Enter Play Mode 시 Game 뷰로 포커스를 옮깁니다.
/// </summary>
[InitializeOnLoad]
internal static class GameViewFocusOnPlay
{
    static GameViewFocusOnPlay()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode)
            return;

        EditorApplication.delayCall += TryFocusGameView;
    }

    static void TryFocusGameView()
    {
        if (!EditorApplication.isPlaying)
            return;

        try
        {
            var gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
            if (gameViewType == null)
                return;

            var gameView = EditorWindow.GetWindow(gameViewType);
            if (gameView != null)
                gameView.Focus();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GameViewFocusOnPlay] Game 뷰 포커스 실패: {e.Message}");
        }
    }
}
#endif
