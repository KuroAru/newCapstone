using System.Collections.Generic;
using Fungus;
using UnityEngine;

public static class PuzzleSolvedStateProvider
{
    private static readonly Dictionary<string, string[]> SolvedVariableCandidatesByScene = new Dictionary<string, string[]>
    {
        { "MainBedroom", new[] { "SafeSolved" } },
        { "ChildRoom", new[] { "allSealsComplete" } },
        { "TutorRoom", new[] { "quiz_complete", "QuizComplete" } },
        { "WifeRoom", new[] { "CheckedMirror" } },
        { "Kitchen", new[] { "giveFood" } },
    };

    public static bool IsSolved(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return false;

        Flowchart fc = FlowchartLocator.Find();
        if (fc == null)
            return false;

        foreach (var pair in SolvedVariableCandidatesByScene)
        {
            if (!sceneName.Contains(pair.Key))
                continue;

            string[] candidates = pair.Value;
            for (int i = 0; i < candidates.Length; i++)
            {
                if (TryGetBoolean(fc, candidates[i], out bool value) && value)
                    return true;
            }
        }

        return false;
    }

    private static bool TryGetBoolean(Flowchart flowchart, string variableName, out bool value)
    {
        value = false;
        if (flowchart == null || string.IsNullOrEmpty(variableName))
            return false;

        try
        {
            value = flowchart.GetBooleanVariable(variableName);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
