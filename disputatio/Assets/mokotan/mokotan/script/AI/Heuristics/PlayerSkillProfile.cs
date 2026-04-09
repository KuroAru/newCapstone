using System;

[Serializable]
public class PlayerSkillProfile
{
    public float progressScore;
    public float accuracyScore;
    public float stuckScore;
    public float skillScore;
    public PlayerSkillLevel level;
    public string reason;
}
