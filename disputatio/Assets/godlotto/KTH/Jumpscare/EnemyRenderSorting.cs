using UnityEngine;

/// <summary>
/// 런타임에만 스폰되는 Enemy 프리팹 루트에 붙이면 Awake 시 정렬을 적용합니다.
/// (씬에 미리 배치된 Enemy는 EnemySortingUtility가 씬 로드 시 처리합니다.)
/// </summary>
[DisallowMultipleComponent]
public class EnemyRenderSorting : MonoBehaviour
{
    private void Awake()
    {
        EnemySortingUtility.ApplyToRoot(gameObject);
    }
}
