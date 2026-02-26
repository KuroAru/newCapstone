using UnityEditor;
using UnityEngine;

public class PPUModifier : Editor
{
    // 유니티 상단 메뉴에 'Tools > Set PPU to 1' 메뉴를 추가합니다.
    [MenuItem("Tools/Set PPU to 1")]
    public static void SetPPUToOne()
    {
        // 프로젝트 창에서 선택한 모든 오브젝트를 가져옵니다.
        Object[] textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);

        if (textures.Length == 0)
        {
            Debug.LogWarning("PPU를 변경할 이미지를 먼저 선택해주세요!");
            return;
        }

        foreach (Texture2D texture in textures)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            // 이미지의 임포트 설정(Importer)을 가져옵니다.
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null)
            {
                // PPU를 1로 설정합니다.
                importer.spritePixelsPerUnit = 1;
                // 변경 사항을 저장하고 다시 불러옵니다.
                importer.SaveAndReimport();
                Debug.Log($"{texture.name}의 PPU가 1로 변경되었습니다.");
            }
        }
        
        Debug.Log($"총 {textures.Length}개의 이미지 수정 완료!");
    }
}