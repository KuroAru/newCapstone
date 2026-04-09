using NUnit.Framework;

public class HintInformationPolicyTests
{
    [Test]
    public void BuildPromptBlock_Novice_ContainsDirectGuidanceText()
    {
        var profile = new PlayerSkillProfile { level = PlayerSkillLevel.Novice };
        string block = HintInformationPolicy.BuildPromptBlock(profile);
        StringAssert.Contains("직접적으로", block);
    }

    [Test]
    public void BuildPromptBlock_Expert_ContainsMinimalExposureText()
    {
        var profile = new PlayerSkillProfile { level = PlayerSkillLevel.Expert };
        string block = HintInformationPolicy.BuildPromptBlock(profile);
        StringAssert.Contains("정보 노출을 최소화", block);
    }
}
