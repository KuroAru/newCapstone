using Fungus;
using UnityEngine;

/// <summary>
/// Callbacks that <see cref="ChesterParrotFlow"/> needs from its MonoBehaviour host.
/// </summary>
internal interface IChesterParrotHost
{
    bool IsRequestInProgress { get; }
    void ActivateQuizInputField();
    bool TryStartQuizTurn(string message);
}

/// <summary>
/// Manages the Chester window → Parrot click flow.
/// Plain C# class — subscribes/unsubscribes to Fungus events on behalf of the host.
/// </summary>
internal sealed class ChesterParrotFlow
{
    public const string ChesterParrotObjectName = "Parret";

    private readonly Flowchart _flowchart;
    private readonly TutorQuizStateTracker _state;
    private readonly IChesterParrotHost _host;
    private readonly bool _debug;

    private Clickable2D _clickable;
    private bool _eventSubscribed;
    private bool _quizStarted;
    private bool _immediateQuestionTurnPending;
    private bool _prevWindowClicked;

    public ChesterParrotFlow(
        Flowchart flowchart,
        Clickable2D initialClickable,
        TutorQuizStateTracker stateTracker,
        IChesterParrotHost host,
        bool debugQuizProgress)
    {
        _flowchart = flowchart;
        _clickable = initialClickable;
        _state = stateTracker;
        _host = host;
        _debug = debugQuizProgress;
    }

    public bool QuizStarted
    {
        get => _quizStarted;
        set => _quizStarted = value;
    }

    public bool ImmediateQuestionTurnPending
    {
        get => _immediateQuestionTurnPending;
        set => _immediateQuestionTurnPending = value;
    }

    public void InitializePrevWindowClicked()
    {
        if (_flowchart != null)
            _prevWindowClicked = _flowchart.GetBooleanVariable(FungusVariableKeys.WindowClicked);
    }

    /// <summary>Call from the host's LateUpdate to keep WindowClicked tracking in sync.</summary>
    public void Tick()
    {
        if (_flowchart == null)
            return;

        bool wc = _flowchart.GetBooleanVariable(FungusVariableKeys.WindowClicked);
        if (!wc && _prevWindowClicked)
            _quizStarted = false;

        _prevWindowClicked = wc;
    }

    // ------------------------------------------------------------------
    //  Clickable2D resolution
    // ------------------------------------------------------------------

    public void TryResolveClickable()
    {
        if (_clickable != null
            && _clickable.gameObject != null
            && !_clickable.gameObject.scene.IsValid())
        {
            _clickable = null;
        }

        if (_clickable != null)
            return;

        foreach (var c in Object.FindObjectsByType<Clickable2D>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c == null || c.gameObject == null || !c.gameObject.scene.IsValid())
                continue;
            if (c.gameObject.name != ChesterParrotObjectName)
                continue;
            _clickable = c;
            if (_debug)
                GameLog.Log("[TutorQuiz] Chester Parret Clickable2D 자동 연결.");
            break;
        }
    }

    // ------------------------------------------------------------------
    //  Event subscription
    // ------------------------------------------------------------------

    public void Subscribe()
    {
        if (_clickable == null || FungusManager.Instance == null || _eventSubscribed)
            return;
        FungusManager.Instance.EventDispatcher
            .AddListener<ObjectClicked.ObjectClickedEvent>(OnObjectClicked);
        _eventSubscribed = true;
    }

    public void Unsubscribe()
    {
        if (!_eventSubscribed || FungusManager.Instance == null)
            return;
        FungusManager.Instance.EventDispatcher
            .RemoveListener<ObjectClicked.ObjectClickedEvent>(OnObjectClicked);
        _eventSubscribed = false;
    }

    // ------------------------------------------------------------------
    //  Event handler
    // ------------------------------------------------------------------

    private void OnObjectClicked(ObjectClicked.ObjectClickedEvent evt)
    {
        if (evt == null || !IsEventFromChesterParrot(evt.ClickableObject))
            return;
        if (_flowchart == null || !_flowchart.GetBooleanVariable(FungusVariableKeys.WindowClicked))
            return;
        if (_state.IsTutorQuizFinished || _quizStarted)
            return;
        if (_host.IsRequestInProgress)
            return;

        _host.ActivateQuizInputField();

        _quizStarted = true;
        _immediateQuestionTurnPending = true;
        if (!_host.TryStartQuizTurn(""))
        {
            _quizStarted = false;
            _immediateQuestionTurnPending = false;
        }
    }

    public bool IsEventFromChesterParrot(Clickable2D clicked)
    {
        if (clicked == null)
            return false;
        if (_clickable != null)
        {
            if (clicked == _clickable)
                return true;
            if (_clickable.gameObject != null
                && clicked.gameObject == _clickable.gameObject)
                return true;
        }

        return clicked.gameObject != null
               && clicked.gameObject.scene.IsValid()
               && clicked.gameObject.name == ChesterParrotObjectName;
    }
}
