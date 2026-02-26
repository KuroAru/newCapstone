using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AnimationPPUWindow : EditorWindow
{
    private float targetPPU = 1.0f; // 기본값 1

    // 유니티 상단 메뉴 설정
    [MenuItem("Tools/Custom PPU Modifier")]
    public static void ShowWindow()
    {
        // 윈도우 생성
        GetWindow<AnimationPPUWindow>("PPU 설정 창");
    }

    private void OnGUI()
    {
        GUILayout.Label("애니메이션 기반 PPU 일괄 변경", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 사용자가 숫자를 입력하는 칸
        targetPPU = EditorGUILayout.FloatField("변경할 PPU 수치", targetPPU);

        GUILayout.Space(10);

        if (GUILayout.Button("선택한 애니메이션의 이미지 변경하기"))
        {
            ModifyPPUFromAnim(targetPPU);
        }

        GUILayout.Space(5);
        EditorGUILayout.HelpBox("1. 프로젝트 창에서 .anim 파일을 선택하세요.\n2. 원하는 PPU를 입력하고 버튼을 누르세요.", MessageType.Info);
    }

    private void ModifyPPUFromAnim(float newPPU)
    {
        // 1. 선택한 애니메이션 클립 가져오기
        Object[] selectedObjects = Selection.GetFiltered<AnimationClip>(SelectionMode.Assets);

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("알림", "먼저 프로젝트 창에서 애니메이션 클립(.anim)을 선택해야 합니다!", "확인");
            return;
        }

        HashSet<string> texturePaths = new HashSet<string>();

        foreach (AnimationClip clip in selectedObjects)
        {
            EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);

            foreach (var binding in bindings)
            {
                ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                
                foreach (var frame in keyframes)
                {
                    if (frame.value is Sprite sprite)
                    {
                        string path = AssetDatabase.GetAssetPath(sprite.texture);
                        if (!string.IsNullOrEmpty(path)) texturePaths.Add(path);
                    }
                }
            }
        }

        int count = 0;
        foreach (string path in texturePaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                // 사용자가 입력한 값(newPPU)으로 설정
                importer.spritePixelsPerUnit = newPPU;
                importer.SaveAndReimport();
                count++;
                Debug.Log($"[PPU 수정 완료] {newPPU}로 변경: {path}");
            }
        }

        EditorUtility.DisplayDialog("완료", $"총 {count}개의 이미지 PPU를 {newPPU}로 변경했습니다.", "확인");
    }
}