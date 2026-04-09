using UnityEngine;

public static class PlayerSkillProfileBuilder
{
    private const float ProgressWeight = 0.45f;
    private const float AccuracyWeight = 0.35f;
    private const float StuckWeight = 0.40f;

    private const float NoviceThreshold = 0.30f;
    private const float IntermediateThreshold = 0.70f;

    private const float RevisitWeight = 0.60f;
    private const float IntervalWeight = 0.25f;
    private const float NoProgressWeight = 0.15f;

    private const float ShortIntervalSeconds = 45f;

    public static PlayerSkillProfile Build(HeuristicSignalInput input)
    {
        float progress = Mathf.Clamp01(input.progressScore);
        float accuracy = Mathf.Clamp01(input.accuracyScore);
        float stuck = BuildStuckScore(input);
        float skill = Mathf.Clamp01((ProgressWeight * progress) + (AccuracyWeight * accuracy) - (StuckWeight * stuck));

        PlayerSkillLevel level;
        if (skill < NoviceThreshold)
            level = PlayerSkillLevel.Novice;
        else if (skill < IntermediateThreshold)
            level = PlayerSkillLevel.Intermediate;
        else
            level = PlayerSkillLevel.Expert;

        return new PlayerSkillProfile
        {
            progressScore = progress,
            accuracyScore = accuracy,
            stuckScore = stuck,
            skillScore = skill,
            level = level,
            reason = $"progress={progress:0.00}, accuracy={accuracy:0.00}, stuck={stuck:0.00}"
        };
    }

    public static float BuildStuckScore(HeuristicSignalInput input)
    {
        float unsolvedRevisitRate = Mathf.Clamp01(input.unsolvedRevisitCount / 3f);
        float shortIntervalRate = input.revisitIntervalSeconds <= 0f
            ? 0f
            : Mathf.Clamp01((ShortIntervalSeconds - input.revisitIntervalSeconds) / ShortIntervalSeconds);
        float noProgressRate = Mathf.Clamp01(input.noProgressAfterRevisitCount / 3f);

        float score = (RevisitWeight * unsolvedRevisitRate)
                    + (IntervalWeight * shortIntervalRate)
                    + (NoProgressWeight * noProgressRate);
        return Mathf.Clamp01(score);
    }
}
