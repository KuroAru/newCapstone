using NUnit.Framework;
using UnityEngine;

public class SceneRevisitTrackerTests
{
    [Test]
    public void FillRevisitSignals_WhenNoData_ReturnsInputUntouched()
    {
        var trackerObject = new GameObject("TrackerTest");
        var tracker = trackerObject.AddComponent<SceneRevisitTracker>();

        var input = new HeuristicSignalInput
        {
            unsolvedRevisitCount = 1,
            revisitIntervalSeconds = 10f,
            noProgressAfterRevisitCount = 1
        };

        HeuristicSignalInput output = tracker.FillRevisitSignals(input, "UnknownScene");
        Assert.AreEqual(1, output.unsolvedRevisitCount);
        Assert.AreEqual(10f, output.revisitIntervalSeconds);
        Assert.AreEqual(1, output.noProgressAfterRevisitCount);

        Object.DestroyImmediate(trackerObject);
    }
}
