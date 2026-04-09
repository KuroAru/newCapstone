using System;

[Serializable]
public struct HeuristicSignalInput
{
    public string roomName;
    public float progressScore;
    public float accuracyScore;
    public int unsolvedRevisitCount;
    public float revisitIntervalSeconds;
    public int noProgressAfterRevisitCount;
}
