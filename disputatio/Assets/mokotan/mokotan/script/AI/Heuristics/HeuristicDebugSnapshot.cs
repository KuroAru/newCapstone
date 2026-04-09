using System;

[Serializable]
public class HeuristicDebugSnapshot
{
    public string roomName;
    public float progressScore;
    public float accuracyScore;
    public float stuckScore;
    public float skillScore;
    public PlayerSkillLevel level;
    public int unsolvedRevisitCount;
    public float revisitIntervalSeconds;
    public int noProgressAfterRevisitCount;
    public string reason;
    public string generatedAtUtc;
}
