using System;
using NUnit.Framework;

[TestFixture]
public class ChatHttpClientTests
{
    // ---------------------------------------------------------------
    //  TryNormalizePromptForChatApi
    // ---------------------------------------------------------------

    [Test]
    public void TryNormalizePrompt_NullInput_ReturnsFalse()
    {
        bool result = ChatHttpClient.TryNormalizePromptForChatApi(null, out string normalized);

        Assert.IsFalse(result);
        Assert.AreEqual("", normalized);
    }

    [Test]
    public void TryNormalizePrompt_EmptyString_ReturnsFalse()
    {
        bool result = ChatHttpClient.TryNormalizePromptForChatApi("", out string normalized);

        Assert.IsFalse(result);
        Assert.AreEqual("", normalized);
    }

    [Test]
    public void TryNormalizePrompt_WhitespaceOnly_ReturnsFalse()
    {
        bool result = ChatHttpClient.TryNormalizePromptForChatApi("   \t\n  ", out string normalized);

        Assert.IsFalse(result);
        Assert.AreEqual("", normalized);
    }

    [Test]
    public void TryNormalizePrompt_NormalMessage_ReturnsTrueAndTrimmed()
    {
        bool result = ChatHttpClient.TryNormalizePromptForChatApi("  hello world  ", out string normalized);

        Assert.IsTrue(result);
        Assert.AreEqual("hello world", normalized);
    }

    [Test]
    public void TryNormalizePrompt_ExactlyMaxLength_ReturnsUnchanged()
    {
        string input = new string('A', 2000);

        bool result = ChatHttpClient.TryNormalizePromptForChatApi(input, out string normalized);

        Assert.IsTrue(result);
        Assert.AreEqual(2000, normalized.Length);
        Assert.AreEqual(input, normalized);
    }

    [Test]
    public void TryNormalizePrompt_ExceedsMaxLength_TruncatesTo2000()
    {
        string input = new string('B', 3000);

        bool result = ChatHttpClient.TryNormalizePromptForChatApi(input, out string normalized);

        Assert.IsTrue(result);
        Assert.AreEqual(2000, normalized.Length);
        Assert.IsTrue(normalized.StartsWith("BBB"));
    }

    [Test]
    public void TryNormalizePrompt_OneChar_ReturnsTrue()
    {
        bool result = ChatHttpClient.TryNormalizePromptForChatApi("X", out string normalized);

        Assert.IsTrue(result);
        Assert.AreEqual("X", normalized);
    }

    // ---------------------------------------------------------------
    //  ResolveChatClientUserId
    // ---------------------------------------------------------------

    [Test]
    public void ResolveChatClientUserId_ReturnsNonNullNonEmpty()
    {
        string userId = ChatHttpClient.ResolveChatClientUserId();

        Assert.IsNotNull(userId);
        Assert.IsNotEmpty(userId);
    }

    [Test]
    public void ResolveChatClientUserId_InEditor_ReturnsEditorId()
    {
#if UNITY_EDITOR
        Assert.AreEqual("unity-editor", ChatHttpClient.ResolveChatClientUserId());
#else
        Assert.Pass("Skipped: not running in editor.");
#endif
    }

    // ---------------------------------------------------------------
    //  Constructor validation
    // ---------------------------------------------------------------

    [Test]
    public void Constructor_NullResolveServerUrl_ThrowsArgumentNullException()
    {
        var history = new ChatHistoryManager(appendCommonVoice: false);
        var host = new StubChatHttpCallbacks();

        Assert.Throws<ArgumentNullException>(() =>
            new ChatHttpClient(null, host, history));
    }

    [Test]
    public void Constructor_NullHost_ThrowsArgumentNullException()
    {
        var history = new ChatHistoryManager(appendCommonVoice: false);

        Assert.Throws<ArgumentNullException>(() =>
            new ChatHttpClient(() => "http://localhost", null, history));
    }

    [Test]
    public void Constructor_NullHistory_ThrowsArgumentNullException()
    {
        var host = new StubChatHttpCallbacks();

        Assert.Throws<ArgumentNullException>(() =>
            new ChatHttpClient(() => "http://localhost", host, null));
    }

    [Test]
    public void ResolvedServerUrl_DelegatesToFunc()
    {
        const string expectedUrl = "http://test-server:9999/chat";
        var history = new ChatHistoryManager(appendCommonVoice: false);
        var host = new StubChatHttpCallbacks();

        var client = new ChatHttpClient(() => expectedUrl, host, history);

        Assert.AreEqual(expectedUrl, client.ResolvedServerUrl);
    }

    // ---------------------------------------------------------------
    //  Minimal stub for IChatHttpCallbacks (constructor tests only)
    // ---------------------------------------------------------------

    private sealed class StubChatHttpCallbacks : IChatHttpCallbacks
    {
        public bool IsRequestInProgress { get; set; }
        public bool? UseToolsOverrideForNextRequest { get; set; }

        public string BuildAndComposeSystemPrompt(string userMessage) => "";
        public void AugmentChatPayload(LocalLlamaPayload payload, string userMessage) { }
        public void OnChatHttpWaitStarted() { }
        public void OnChatHttpWaitFinished() { }
        public void OnStreamTextDelta(string delta) { }
        public void SayLine(string message, Action onComplete) => onComplete?.Invoke();
        public UnityEngine.Coroutine StartHostCoroutine(System.Collections.IEnumerator routine) => null;
        public System.Collections.IEnumerator HandleChatbotResponse(
            string responseMessage,
            System.Collections.Generic.List<FunctionCallData> functionCalls)
        {
            yield break;
        }
    }
}
