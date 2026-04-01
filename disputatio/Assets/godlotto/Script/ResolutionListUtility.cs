using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 설정 화면에서 사용하는 해상도 목록: 동일 (가로×세로)당 최고 주사율 1개만 남기고,
/// 프로젝트에서 허용한 일반 해상도만 필터링합니다.
/// </summary>
public static class ResolutionListUtility
{
    private static readonly Vector2Int[] PreferredSizes =
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1366, 768),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
        new Vector2Int(3840, 2160),
    };

    public static List<Resolution> BuildPreferredResolutionList()
    {
        Resolution[] all = Screen.resolutions;
        IEnumerable<Resolution> unique = all
            .GroupBy(r => new { r.width, r.height })
            .Select(g => g.OrderByDescending(r => r.refreshRateRatio.value).First());

        return unique
            .Where(r => PreferredSizes.Any(p => p.x == r.width && p.y == r.height))
            .ToList();
    }

    public static List<string> BuildLabels(IReadOnlyList<Resolution> resolutions)
    {
        var list = new List<string>(resolutions.Count);
        for (int i = 0; i < resolutions.Count; i++)
        {
            Resolution r = resolutions[i];
            list.Add($"{r.width} x {r.height}");
        }

        return list;
    }
}
