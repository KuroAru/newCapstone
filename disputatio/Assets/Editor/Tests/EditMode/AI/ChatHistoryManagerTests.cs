using NUnit.Framework;

[TestFixture]
public class ChatHistoryManagerTests
{
    // ---------------------------------------------------------------
    //  Initialize
    // ---------------------------------------------------------------

    [Test]
    public void Initialize_ClearsHistoryAndAddsSystemMessage()
    {
        var mgr = new ChatHistoryManager(appendCommonVoice: false);
        mgr.Initialize();

        Assert.AreEqual(1, mgr.History.Count);
        Assert.AreEqual("system", mgr.History[0].role);
        Assert.IsNotNull(mgr.History[0].content);
        Assert.IsNotEmpty(mgr.History[0].content);
    }

    [Test]
    public void Initialize_CalledTwice_ResetsToSingleMessage()
    {
        var mgr = new ChatHistoryManager(appendCommonVoice: false);
        mgr.Initialize();
        mgr.AddMessage("user", "hello");
        mgr.AddMessage("assistant", "hi");
        Assert.AreEqual(3, mgr.History.Count);

        mgr.Initialize();

        Assert.AreEqual(1, mgr.History.Count);
        Assert.AreEqual("system", mgr.History[0].role);
    }

    // ---------------------------------------------------------------
    //  AddMessage
    // ---------------------------------------------------------------

    [Test]
    public void AddMessage_AppendsToHistory()
    {
        var mgr = new ChatHistoryManager(appendCommonVoice: false);
        mgr.Initialize();

        mgr.AddMessage("user", "test input");

        Assert.AreEqual(2, mgr.History.Count);
        Assert.AreEqual("user", mgr.History[1].role);
        Assert.AreEqual("test input", mgr.History[1].content);
    }

    [Test]
    public void AddMessage_MultipleTimes_PreservesOrder()
    {
        var mgr = new ChatHistoryManager(appendCommonVoice: false);
        mgr.Initialize();

        mgr.AddMessage("user", "first");
        mgr.AddMessage("assistant", "second");
        mgr.AddMessage("user", "third");

        Assert.AreEqual(4, mgr.History.Count);
        Assert.AreEqual("first", mgr.History[1].content);
        Assert.AreEqual("second", mgr.History[2].content);
        Assert.AreEqual("third", mgr.History[3].content);
    }

    // ---------------------------------------------------------------
    //  ComposeSystemPromptWithCommonRules
    // ---------------------------------------------------------------

    [Test]
    public void ComposeSystemPrompt_AppendVoiceFalse_ReturnsRoomPromptUnchanged()
    {
        var mgr = new ChatHistoryManager(appendCommonVoice: false);
        const string roomPrompt = "You are in the kitchen.";

        string result = mgr.ComposeSystemPromptWithCommonRules(roomPrompt);

        Assert.AreEqual(roomPrompt, result);
    }

    [Test]
    public void ComposeSystemPrompt_NullRoomPrompt_ReturnsNull()
    {
        var mgr = new ChatHistoryManager(appendCommonVoice: true);

        string result = mgr.ComposeSystemPromptWithCommonRules(null);

        Assert.IsNull(result);
    }

    [Test]
    public void ComposeSystemPrompt_EmptyRoomPrompt_ReturnsEmpty()
    {
        var mgr = new ChatHistoryManager(appendCommonVoice: true);

        string result = mgr.ComposeSystemPromptWithCommonRules("");

        Assert.AreEqual("", result);
    }

    // ---------------------------------------------------------------
    //  History property
    // ---------------------------------------------------------------

    [Test]
    public void History_BeforeInitialize_IsEmpty()
    {
        var mgr = new ChatHistoryManager(appendCommonVoice: false);

        Assert.IsNotNull(mgr.History);
        Assert.AreEqual(0, mgr.History.Count);
    }
}
