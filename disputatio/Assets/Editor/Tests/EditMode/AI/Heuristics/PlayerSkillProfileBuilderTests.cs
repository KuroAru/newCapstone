using NUnit.Framework;

public class PlayerSkillProfileBuilderTests
{
    [Test]
    public void Build_WithHighProgressAndLowStuck_ReturnsExpert()
    {
        var input = new HeuristicSignalInput
        {
            progressScore = 0.95f,
            accuracyScore = 0.95f,
            unsolvedRevisitCount = 0,
            revisitIntervalSeconds = 120f,
            noProgressAfterRevisitCount = 0
        };

        PlayerSkillProfile profile = PlayerSkillProfileBuilder.Build(input);
        Assert.AreEqual(PlayerSkillLevel.Expert, profile.level);
    }

    [Test]
    public void Build_WithRepeatedUnsolvedRevisit_ReturnsNovice()
    {
        var input = new HeuristicSignalInput
        {
            progressScore = 0.1f,
            accuracyScore = 0.1f,
            unsolvedRevisitCount = 4,
            revisitIntervalSeconds = 5f,
            noProgressAfterRevisitCount = 4
        };

        PlayerSkillProfile profile = PlayerSkillProfileBuilder.Build(input);
        Assert.AreEqual(PlayerSkillLevel.Novice, profile.level);
        Assert.Greater(profile.stuckScore, 0.7f);
    }
}
