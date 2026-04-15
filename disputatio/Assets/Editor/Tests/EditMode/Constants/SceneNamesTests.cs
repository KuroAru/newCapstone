using NUnit.Framework;

[TestFixture]
public class SceneNamesTests
{
    [Test]
    public void MainMenu_IsMainMenuScene()
    {
        Assert.AreEqual("MainMenuScene", SceneNames.MainMenu);
    }

    [Test]
    public void MainScene_IsMainScene()
    {
        Assert.AreEqual("MainScene", SceneNames.MainScene);
    }

    [Test]
    public void Kitchen_IsKitchen()
    {
        Assert.AreEqual("Kitchen", SceneNames.Kitchen);
    }

    [Test]
    public void MaidRoom_IsMaidRoom()
    {
        Assert.AreEqual("MaidRoom", SceneNames.MaidRoom);
    }

    [Test]
    public void StudyRoom_IsStudyRoom()
    {
        Assert.AreEqual("StudyRoom", SceneNames.StudyRoom);
    }

    [Test]
    public void TutorRoom_IsTutorRoom()
    {
        Assert.AreEqual("TutorRoom", SceneNames.TutorRoom);
    }

    [Test]
    public void ChildRoom_IsChildRoom()
    {
        Assert.AreEqual("ChildRoom", SceneNames.ChildRoom);
    }

    [Test]
    public void WifeRoom_IsWifeRoom()
    {
        Assert.AreEqual("WifeRoom", SceneNames.WifeRoom);
    }

    [Test]
    public void BedRoom_IsBedRoom()
    {
        Assert.AreEqual("BedRoom", SceneNames.BedRoom);
    }

    [Test]
    public void HallRight_IsHall_Right()
    {
        Assert.AreEqual("Hall_Right", SceneNames.HallRight);
    }
}
