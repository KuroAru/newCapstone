using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Fungus;
using System;
using System.Text;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;

[Serializable]
internal class TutorGradeResponseDto
{
    [JsonProperty("is_correct")]
    public bool is_correct;

    [JsonProperty("question_id")]
    public string question_id;

    [JsonProperty("reference_snippet")]
    public string reference_snippet;

    [JsonProperty("quiz_complete_after")]
    public bool quiz_complete_after;

    [JsonProperty("unknown_question")]
    public bool unknown_question;
}

public class TutorChatbot : BaseChatbot
{
    private const string VarWindowClicked = "WindowClicked";
    private const string VarIsClicked = "isClicked";

    private const string VarCorrectAnswerCount = "CorrectAnswerCount";
    /// <summary>일부 씬에서 오타로만 정의된 경우(Fungus Integer).</summary>
    private const string VarCorrectAnswerCcTypo = "CorrectAnswerCc";

    /// <summary>미션 완료 판정·UI 잠금의 단일 기준(Fungus 정답 누적).</summary>
    public const int TutorQuizTargetCorrectCount = 5;

    [Header("TutorBot Settings")]
    [SerializeField] public Flowchart flowchart;
    [Tooltip("비워 두면 씬에서 QuizInputHandler를 찾습니다. 전송 Button이 TutorChatbot.OnSendButtonClick에만 연결돼 있을 때 실제 입력은 여기 필드를 읽습니다.")]
    [SerializeField] private QuizInputHandler quizInputHandler;
    [Tooltip("한 줄에 하나씩 question_id (quiz_bank.csv와 동일). CorrectAnswerCount번째 줄이 현재 출제/채점 ID.")]
    [SerializeField] private TextAsset tutorQuestionOrderAsset;

    [Header("Tutor 채점 API")]
    [Tooltip("비우면 BaseChatbot의 chat URL에서 /chat → /tutor/grade 로 바꿉니다. EC2 등에 아직 /tutor/grade가 없으면 404 — 서버 배포 후 사용하거나, 여기에 전체 URL을 직접 넣으세요.")]
    [SerializeField] private string tutorGradeUrlOverride = "";

    [Header("UX — 느린 응답·패널 버튼")]
    [SerializeField] private float thinkingHoldDelaySeconds = 3f;
    [SerializeField] [TextArea(1, 3)] private string thinkingHoldSayMessage = "음… 지금 생각하는 중이야. 조금만 기다려 줘.";

    [Header("Events")]
    public UnityEvent OnAIReponseComplete;
    public UnityEvent OnQuizCompletedEvent;

    [Header("Debug")]
    [Tooltip("CorrectAnswerCount·채점 경로 등 퀴즈 진행 로그.")]
    [SerializeField] private bool debugQuizProgress = true;

    [Header("Chester 창 → 앵무 플로우")]
    [Tooltip("앵무 오브젝트의 Clickable2D. 비워도 씬에서 이름이 \"Parret\"인 Clickable2D를 찾습니다. WindowClicked인 동안 빈 /chat은 앵무 클릭(또는 이미 연 패널·책상 경로) 전까지 막습니다.")]
    [SerializeField] private Clickable2D chesterParrotClickable;

    private Coroutine _thinkingHoldCoroutine;
    /// <summary><see cref="BaseChatbot.OnChatHttpWaitStarted"/>~<see cref="BaseChatbot.OnChatHttpWaitFinished"/> 구간만 true — Say 대기와 구분.</summary>
    private bool _tutorWaitingOnHttp;

    private bool _awaitingQuestionAdvance;
    private bool _lastGradedWasCorrect;
    private bool _lastEmbeddedQuizComplete;
    private int _skipOrderOffset;

    /// <summary>세션당 <see cref="OnQuizCompletedEvent"/> 한 번만 호출.</summary>
    private bool _quizCompletionEventFired;

    /// <summary>직전 AI 턴 이후 플레이어 자유 답변을 HTTP <c>/tutor/grade</c>로 채점할지.</summary>
    private bool _expectingQuizAnswer;

    /// <summary>앵무 클릭으로 첫 퀴즈 턴을 시작했는지(WindowClicked 모드).</summary>
    private bool _chesterParrotQuizStarted;

    /// <summary>다음 <see cref="CoTutorPlayerTurn"/> 한 번만 앵무 즉시 출제 프롬프트·생각 홀드 생략.</summary>
    private bool _chesterParrotImmediateQuestionTurnPending;

    private bool _chesterParrotEventSubscribed;
    private bool _prevFlowchartWindowClicked;

    private const string ChesterParrotObjectName = "Parret";

    private const string UserPromptAfterCorrectAnswer =
        "[시스템: 방금 플레이어 답은 서버에서 정답으로 확정되었다. 아주 짧게 격려한 뒤, " +
        "문제 은행에 적힌 **다음** 질문 문장을 글자 그대로 한 줄로만 말해. 진행 숫자·n/5·몇 문제는 말하지 마. 새 JSON이나 툴은 쓰지 마.]";

    private const string UserPromptMissionComplete =
        "[시스템: 플레이어가 오늘 퀴즈 미션을 모두 완료했다. 짧게 칭찬하고 마무리 인사만 해. 새 문제는 내지 마.]";

    /// <summary>Fungus가 창(체셔) 클릭 시 빈 문자열로 <see cref="AddPlayerMessageAndGetResponse"/>를 호출할 때 실제 /chat 프롬프트로 씁니다.</summary>
    private const string UserPromptChesterWindowOpen =
        "[시스템: 플레이어가 체셔(창)와 대화를 막 시작했다. 아주 짧게 인사만 한 뒤, " +
        "문제 은행에 적힌 **지금** 퀴즈 질문 문장을 글자 그대로 한 줄로만 말해. 진행 숫자·n/5·몇 문제는 말하지 마. 새 JSON이나 툴은 쓰지 마.]";

    /// <summary>앵무 클릭 직후 첫 /chat — 인사·도입 없이 질문 한 줄만.</summary>
    private const string UserPromptChesterParrotAskQuestionNow =
        "[시스템: 플레이어가 앵무를 방금 눌러 퀴즈를 시작했다. 인사·도입·잡담·추가 문장 금지. " +
        "문제 은행의 **지금** 질문 문장만 글자 그대로 한 줄로 말해. 진행 숫자·n/5·따옴표·머리말 금지. 새 JSON이나 툴은 쓰지 마.]";

    /// <summary>백엔드 <c>TutorGradeRequest.user_answer</c> 상한과 맞춤.</summary>
    private const int MaxTutorGradeAnswerChars = 4000;

    /// <summary>패널과 SayDialog 동기화(<see cref="TutorPanelSayDialogSync"/>)용.</summary>
    public SayDialog TutorSayDialogForPanelSync => chatSayDialog;

    /// <summary>서버 응답 처리 및 Say가 끝날 때까지 true — 이 동안 추가 제출은 막습니다.</summary>
    public bool IsAiResponseInFlight => isRequestInProgress;

    /// <summary>Fungus <see cref="VarCorrectAnswerCount"/>가 목표에 도달했거나 완료 이벤트가 이미 발생했습니다.</summary>
    public bool IsTutorQuizFinished =>
        _quizCompletionEventFired || ReadCorrectAnswerCount() >= TutorQuizTargetCorrectCount;

    /// <summary>Fungus Writer 기본 writingSpeed(~60)는 글자 단위로 밀어 넣어 한글도 한 글자씩 들어가는 것처럼 보입니다.</summary>
    private const int TutorWriterCharsPerSecond = 4800;

    /// <summary>Enter 제출은 <see cref="QuizInputHandler"/>만 처리 — BaseChatbot과 이중 등록 방지.</summary>
    protected override bool RegisterInputFieldSubmitListener => false;

    /// <summary>퀴즈 중 대사창이 꺼지면 입력·진행이 끊기므로 Say 완료 후에도 SayDialog를 켜 둡니다.</summary>
    protected override bool DeactivateSayDialogWhenLineCompletes => false;

    /// <summary>인풋과 TutorChatbot이 같은 오브젝트를 쓰면 Say 때 SetActive(false)가 IME·여러 단어 입력을 망가뜨립니다.</summary>
    protected override bool HideInputFieldDuringSay => false;

    protected override void Start()
    {
        base.Start();
        if (quizInputHandler == null)
            quizInputHandler = FindObjectOfType<QuizInputHandler>();
        ConfigureTutorSayDialogInput();
        if (ReadCorrectAnswerCount() >= TutorQuizTargetCorrectCount)
            LockQuizInputAfterSessionComplete();
        else
            _expectingQuizAnswer = false;

        TryResolveChesterParrotClickable();
        SubscribeChesterParrotClick();
        if (flowchart != null)
            _prevFlowchartWindowClicked = flowchart.GetBooleanVariable("WindowClicked");
        StartCoroutine(CoDeferredChesterParrotSubscribe());
    }

    private IEnumerator CoDeferredChesterParrotSubscribe()
    {
        yield return null;
        TryResolveChesterParrotClickable();
        SubscribeChesterParrotClick();
    }

    private void OnEnable()
    {
        TryResolveChesterParrotClickable();
        SubscribeChesterParrotClick();
    }

    private void OnDisable()
    {
        UnsubscribeChesterParrotClick();
    }

    private void LateUpdate()
    {
        if (flowchart == null)
            return;

        bool wc = flowchart.GetBooleanVariable("WindowClicked");
        if (!wc && _prevFlowchartWindowClicked)
            _chesterParrotQuizStarted = false;

        _prevFlowchartWindowClicked = wc;
    }

    private void TryResolveChesterParrotClickable()
    {
        if (chesterParrotClickable != null
            && chesterParrotClickable.gameObject != null
            && !chesterParrotClickable.gameObject.scene.IsValid())
        {
            chesterParrotClickable = null;
        }

        if (chesterParrotClickable != null)
            return;

        foreach (var c in FindObjectsByType<Clickable2D>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c == null || c.gameObject == null || !c.gameObject.scene.IsValid())
                continue;
            if (c.gameObject.name != ChesterParrotObjectName)
                continue;
            chesterParrotClickable = c;
            if (debugQuizProgress)
                Debug.Log("[TutorQuiz] Chester Parret Clickable2D 자동 연결.");
            break;
        }
    }

    private bool IsEventFromChesterParrot(Clickable2D clicked)
    {
        if (clicked == null)
            return false;
        if (chesterParrotClickable != null)
        {
            if (clicked == chesterParrotClickable)
                return true;
            if (chesterParrotClickable.gameObject != null
                && clicked.gameObject == chesterParrotClickable.gameObject)
                return true;
        }

        return clicked.gameObject != null
            && clicked.gameObject.scene.IsValid()
            && clicked.gameObject.name == ChesterParrotObjectName;
    }

    private void SubscribeChesterParrotClick()
    {
        if (chesterParrotClickable == null || FungusManager.Instance == null || _chesterParrotEventSubscribed)
            return;
        FungusManager.Instance.EventDispatcher.AddListener<ObjectClicked.ObjectClickedEvent>(OnChesterParrotObjectClicked);
        _chesterParrotEventSubscribed = true;
    }

    private void UnsubscribeChesterParrotClick()
    {
        if (!_chesterParrotEventSubscribed || FungusManager.Instance == null)
            return;
        FungusManager.Instance.EventDispatcher.RemoveListener<ObjectClicked.ObjectClickedEvent>(OnChesterParrotObjectClicked);
        _chesterParrotEventSubscribed = false;
    }

    private void OnChesterParrotObjectClicked(ObjectClicked.ObjectClickedEvent evt)
    {
        if (evt == null || !IsEventFromChesterParrot(evt.ClickableObject))
            return;
        if (flowchart == null || !flowchart.GetBooleanVariable("WindowClicked"))
            return;
        if (IsTutorQuizFinished || _chesterParrotQuizStarted)
            return;
        if (isRequestInProgress)
            return;

        if (quizInputHandler != null)
            quizInputHandler.ActivateQuizInputField();

        _chesterParrotQuizStarted = true;
        _chesterParrotImmediateQuestionTurnPending = true;
        if (!TryAddPlayerMessageAndGetResponse(""))
        {
            _chesterParrotQuizStarted = false;
            _chesterParrotImmediateQuestionTurnPending = false;
        }
    }

    /// <summary>
    /// Fungus 기본 SayDialog는 ClickAnywhere + Space로 대사 진행 — 한글 띄어쓰기·IME와 충돌. 패널 클릭으로만 진행.
    /// </summary>
    private void ConfigureTutorSayDialogInput()
    {
        if (chatSayDialog == null)
            return;
        var di = chatSayDialog.GetComponentInChildren<DialogInput>(true);
        if (di != null && di.clickMode == ClickMode.ClickAnywhere)
            di.clickMode = ClickMode.ClickOnDialog;
    }

    private static bool PointerHitContainsTMPInputField()
    {
        var es = EventSystem.current;
        if (es == null)
            return false;
        var ped = new PointerEventData(es) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        es.RaycastAll(ped, results);
        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].gameObject.GetComponentInParent<TMP_InputField>() != null)
                return true;
        }
        return false;
    }

    protected override bool AllowLegacyInputToAdvanceSayDialog(bool mouseButtonDown, bool advanceKeyDown)
    {
        var es = EventSystem.current;
        if (es == null)
            return true;

        GameObject sel = es.currentSelectedGameObject;
        if (sel != null && sel.GetComponentInParent<TMP_InputField>() != null)
            return false;

        if (mouseButtonDown && PointerHitContainsTMPInputField())
            return false;

        return true;
    }

    protected override void Say(string message, Action onComplete = null)
    {
        if (!string.IsNullOrEmpty(message))
            message = "{s=" + TutorWriterCharsPerSecond + "}" + message + "{/s}";
        base.Say(message, onComplete);
    }

    /// <summary>Button이 BaseChatbot 쪽만 바인딩된 경우에도 Quiz 입력 필드로 전송되게 합니다.</summary>
    public override void OnSendButtonClick()
    {
        if (IsTutorQuizFinished)
            return;

        if (quizInputHandler != null)
        {
            quizInputHandler.SubmitQuizAnswerFromUI();
            return;
        }

        base.OnSendButtonClick();
    }

    public void AddPlayerMessageAndGetResponse(string playerMessage)
    {
        TryAddPlayerMessageAndGetResponse(playerMessage);
    }

    private bool TryAddPlayerMessageAndGetResponse(string playerMessage)
    {
        if (IsTutorQuizFinished)
            return false;

        if (isRequestInProgress)
        {
            Debug.LogWarning("이미 AI 응답 요청이 진행 중입니다. 새로운 요청을 무시합니다.");
            return false;
        }

        StartCoroutine(CoTutorPlayerTurn(playerMessage));
        Debug.Log($"플레이어 답변 전송 및 GPT 응답 요청: {playerMessage}");
        return true;
    }

    /// <summary>GameTimer 등 — 퀴즈와 무관한 짧은 안내용.</summary>
    public void TriggerAIResponseByFlag()
    {
        if (isRequestInProgress)
            return;
        if (IsTutorQuizFinished)
            return;
        StartCoroutine(CoTutorPlayerTurn(
            "[타이머] 남은 플레이 시간이 얼마 없다는 안내만 짧게 해 줘. 새 퀴즈 문제는 내지 마."));
    }

    /// <summary>답 입력 없이 패널 버튼만 눌렀을 때 — 정답 직후 또는 건너뛰기 직후에만 다음 AI 턴으로 넘깁니다.</summary>
    public bool TryHandleEmptyPanelSubmit()
    {
        if (IsTutorQuizFinished)
            return false;

        if (debugQuizProgress)
            Debug.Log(
                $"[TutorQuiz] TryHandleEmptyPanelSubmit: awaiting={_awaitingQuestionAdvance}, " +
                $"isRequestInProgress={isRequestInProgress}, lastQuizComplete={_lastEmbeddedQuizComplete}, " +
                $"CorrectAnswerCount={ReadCorrectAnswerCount()}");

        if (!_awaitingQuestionAdvance || isRequestInProgress)
            return false;
        if (_lastEmbeddedQuizComplete)
            return false;

        _awaitingQuestionAdvance = false;

        if (!_lastGradedWasCorrect)
            _skipOrderOffset++;

        string msg = _lastGradedWasCorrect
            ? "[플레이어가 다음 문제로 넘어갔습니다. 짧게 반응한 뒤 다음 퀴즈 질문 한 가지만 출제해 줘.]"
            : "[플레이어가 이 문제를 건너뜁니다. 짧게 반응한 뒤 다음 퀴즈 질문 한 가지만 출제해 줘.]";

        if (debugQuizProgress)
            Debug.Log(
                $"[TutorQuiz] 빈 패널 제출 수락 → 다음 AI 턴 (직전 채점 정답={_lastGradedWasCorrect}), " +
                $"_skipOrderOffset={_skipOrderOffset}, CorrectAnswerCount={ReadCorrectAnswerCount()}");

        StartCoroutine(CoTutorPlayerTurn(msg));
        return true;
    }

    private IEnumerator CoTutorPlayerTurn(string playerMessage)
    {
        if (ShouldRunDeterministicGrading(playerMessage))
        {
            yield return StartCoroutine(CoGradeThenReact(playerMessage));
            yield break;
        }

        string llmUserTurn = playerMessage;
        // Chester: WindowClicked인 동안 빈 메시지는 앵무로 시작했거나(또는 책상 등으로 패널이 이미 켜진 뒤)만 허용.
        if (string.IsNullOrWhiteSpace(llmUserTurn)
            && flowchart != null
            && flowchart.GetBooleanVariable("WindowClicked")
            && !IsTutorQuizFinished
            && !_chesterParrotQuizStarted
            && !(quizInputHandler != null && quizInputHandler.IsQuizInputPanelActive))
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(llmUserTurn)
            && flowchart != null
            && flowchart.GetBooleanVariable("WindowClicked")
            && !IsTutorQuizFinished)
        {
            llmUserTurn = _chesterParrotImmediateQuestionTurnPending
                ? UserPromptChesterParrotAskQuestionNow
                : UserPromptChesterWindowOpen;
        }

        bool immediateParrotQuestion = _chesterParrotImmediateQuestionTurnPending;
        if (immediateParrotQuestion)
            _chesterParrotImmediateQuestionTurnPending = false;

        if (immediateParrotQuestion)
            _thinkingHoldCoroutine = null;
        else
            _thinkingHoldCoroutine = StartCoroutine(CoThinkingHoldIfSlow());
        yield return StartCoroutine(GetGPTResponse(llmUserTurn));
        if (_thinkingHoldCoroutine != null)
        {
            StopCoroutine(_thinkingHoldCoroutine);
            _thinkingHoldCoroutine = null;
        }
    }

    private bool ShouldRunDeterministicGrading(string playerMessage)
    {
        if (string.IsNullOrWhiteSpace(playerMessage))
            return false;
        if (playerMessage.TrimStart().StartsWith("[", StringComparison.Ordinal))
            return false;
        if (flowchart == null || !flowchart.GetBooleanVariable("WindowClicked"))
            return false;
        if (IsTutorQuizFinished)
            return false;
        return _expectingQuizAnswer;
    }

    /// <summary>
    /// <c>…/chat</c> → <c>…/tutor/grade</c>. 리버스 프록시·경로가 다르면 <see cref="tutorGradeUrlOverride"/> 사용.
    /// </summary>
    private string ResolveTutorGradeUrl()
    {
        if (!string.IsNullOrWhiteSpace(tutorGradeUrlOverride))
            return tutorGradeUrlOverride.Trim();

        string chatUrl = localServerUrl;
        if (string.IsNullOrWhiteSpace(chatUrl))
            return "";

        string u = chatUrl.Trim().TrimEnd('/');
        const string gradeSuffix = "/tutor/grade";
        // 인스펙터에 실수로 …/tutor/grade 를 넣은 경우 — 여기서 또 붙이면 …/tutor/grade/tutor/grade 가 됨.
        if (u.EndsWith(gradeSuffix, StringComparison.OrdinalIgnoreCase))
            return u;

        const string chatSuffix = "/chat";
        if (u.EndsWith(chatSuffix, StringComparison.OrdinalIgnoreCase))
            return u.Substring(0, u.Length - chatSuffix.Length) + gradeSuffix;

        if (u.Contains("/chat", StringComparison.OrdinalIgnoreCase))
            return u.Replace("/chat", gradeSuffix, StringComparison.OrdinalIgnoreCase);

        return u + gradeSuffix;
    }

    private IEnumerator CoGradeThenReact(string playerAnswer)
    {
        _expectingQuizAnswer = false;

        string qid = ResolveCurrentQuestionIdFromOrderAsset();
        if (string.IsNullOrWhiteSpace(qid))
        {
            Debug.LogError("[TutorQuiz] 채점할 question_id가 없습니다. TutorQuestionOrder·CorrectAnswerCount를 확인하세요.");
            _expectingQuizAnswer = true;
            yield break;
        }

        string url = ResolveTutorGradeUrl();
        if (string.IsNullOrWhiteSpace(url))
        {
            Debug.LogError("[TutorQuiz] 채점 URL이 비었습니다. BaseChatbot localServerUrl 또는 TutorGradeUrlOverride를 설정하세요.");
            _expectingQuizAnswer = true;
            yield break;
        }

        string answerForGrade = playerAnswer ?? "";
        if (answerForGrade.Length > MaxTutorGradeAnswerChars)
            answerForGrade = answerForGrade.Substring(0, MaxTutorGradeAnswerChars);
        int ccBefore = Mathf.Clamp(ReadCorrectAnswerCount(), 0, 10_000);

        var payload = new Dictionary<string, object>
        {
            ["question_id"] = qid,
            ["user_answer"] = answerForGrade,
            ["correct_count_before"] = ccBefore,
            ["quiz_target"] = TutorQuizTargetCorrectCount,
        };
        string jsonBody = JsonConvert.SerializeObject(payload);

        TutorGradeResponseDto grade = null;

        using (var req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.certificateHandler = new BypassCertificate();
            req.timeout = 30;

            OnChatHttpWaitStarted();
            try
            {
                yield return req.SendWebRequest();
            }
            finally
            {
                OnChatHttpWaitFinished();
            }

            if (req.result != UnityWebRequest.Result.Success)
            {
                string body = req.downloadHandler != null ? req.downloadHandler.text : "";
                if (body != null && body.Length > 200)
                    body = body.Substring(0, 200) + "…";
                Debug.LogError(
                    "[TutorQuiz] /tutor/grade 실패: " + req.error +
                    " | HTTP " + req.responseCode +
                    " | URL: " + url +
                    " | 서버에 POST /tutor/grade 라우트가 배포됐는지 확인하세요. (로그에 404면 구버전 백엔드일 수 있음)" +
                    (string.IsNullOrEmpty(body) ? "" : " | body: " + body));
                _expectingQuizAnswer = true;
                yield break;
            }

            try
            {
                grade = JsonConvert.DeserializeObject<TutorGradeResponseDto>(req.downloadHandler.text);
            }
            catch (Exception e)
            {
                Debug.LogError("[TutorQuiz] 채점 응답 파싱 실패: " + e.Message);
                _expectingQuizAnswer = true;
                yield break;
            }
        }

        if (grade == null)
        {
            _expectingQuizAnswer = true;
            yield break;
        }

        if (debugQuizProgress)
            Debug.Log(
                $"[TutorQuiz] /tutor/grade: qid={grade.question_id}, ok={grade.is_correct}, unknown={grade.unknown_question}");

        if (!grade.is_correct || grade.unknown_question)
        {
            string hint = string.IsNullOrWhiteSpace(grade.reference_snippet)
                ? "아직 정답이 아니야. 다시 생각해 봐!"
                : $"아직 정답이 아니야. 힌트: {grade.reference_snippet}";
            bool hintDone = false;
            Say(hint, () => hintDone = true);
            yield return new WaitUntil(() => hintDone);
            _lastGradedWasCorrect = false;
            _lastEmbeddedQuizComplete = false;
            _awaitingQuestionAdvance = true;
            _expectingQuizAnswer = true;
            yield break;
        }

        ApplyQuizResult(true, false);
        _lastGradedWasCorrect = true;

        useToolsOverrideForNextRequest = false;
        _thinkingHoldCoroutine = StartCoroutine(CoThinkingHoldIfSlow());

        if (IsTutorQuizFinished)
        {
            yield return StartCoroutine(GetGPTResponse(UserPromptMissionComplete));
            HideTutorQuizUiAfterSessionComplete();
        }
        else
            yield return StartCoroutine(GetGPTResponse(UserPromptAfterCorrectAnswer));

        if (_thinkingHoldCoroutine != null)
        {
            StopCoroutine(_thinkingHoldCoroutine);
            _thinkingHoldCoroutine = null;
        }
    }

    private IEnumerator CoThinkingHoldIfSlow()
    {
        float delay = Mathf.Max(thinkingHoldDelaySeconds, 0.5f);
        yield return new WaitForSecondsRealtime(delay);
        // isRequestInProgress는 HTTP 이후에도 Say가 끝날 때까지 true → 서버 대기 전용 플래그만 사용
        if (!_tutorWaitingOnHttp)
            yield break;
        if (chatSayDialog != null)
            chatSayDialog.Stop();
        Say(thinkingHoldSayMessage, null);
    }

    private static bool ShouldOfferPanelAdvanceAfterTurn(IReadOnlyList<OpenAIMessage> history, bool hasEmbeddedQuizJson, bool embeddedQuizComplete)
    {
        if (!hasEmbeddedQuizJson || embeddedQuizComplete)
            return false;
        if (history == null || history.Count < 2)
            return false;
        OpenAIMessage prev = history[history.Count - 2];
        if (prev.role != "user")
            return false;
        string c = prev.content ?? string.Empty;
        if (c.StartsWith("[", StringComparison.Ordinal))
            return false;
        return true;
    }

    /// <summary>정답 수 Integer 변수 키. <see cref="VarCorrectAnswerCount"/> 우선, 없으면 <see cref="VarCorrectAnswerCcTypo"/>.</summary>
    private string ResolveCorrectAnswerCountKey()
    {
        if (flowchart == null)
            return VarCorrectAnswerCount;
        if (flowchart.GetVariable(VarCorrectAnswerCount) is IntegerVariable)
            return VarCorrectAnswerCount;
        if (flowchart.GetVariable(VarCorrectAnswerCcTypo) is IntegerVariable)
            return VarCorrectAnswerCcTypo;
        return VarCorrectAnswerCount;
    }

    private int ReadCorrectAnswerCount()
    {
        return flowchart != null ? flowchart.GetIntegerVariable(ResolveCorrectAnswerCountKey()) : 0;
    }

    private void WriteCorrectAnswerCount(int value)
    {
        if (flowchart == null)
            return;
        string key = ResolveCorrectAnswerCountKey();
        int prev = flowchart.GetIntegerVariable(key);
        flowchart.SetIntegerVariable(key, value);
        if (debugQuizProgress)
            Debug.Log($"[TutorQuiz] CorrectAnswerCount 쓰기: key={key}, {prev} → {value}");
    }

    protected override string BuildFinalSystemPrompt()
    {
        string finalSystemPrompt = chatHistory[0].content;

        if (flowchart != null)
        {
            bool windowClicked = flowchart.GetBooleanVariable("WindowClicked");
            if (windowClicked)
            {
                TextAsset tutorRoomPromptAsset = Resources.Load<TextAsset>("TutorRoomPrompt");
                if (tutorRoomPromptAsset != null)
                {
                    finalSystemPrompt += "\n\n" + tutorRoomPromptAsset.text;
                }
                else
                {
                    Debug.LogError("TutorRoomPrompt.txt 파일을 찾을 수 없습니다!");
                    finalSystemPrompt += "\n\n[중요 지시]... (TutorRoomPrompt 내용)...";
                }
            }
        }
        return finalSystemPrompt;
    }

    protected override HeuristicSignalInput BuildHeuristicSignalInput(string userMessage)
    {
        var signal = base.BuildHeuristicSignalInput(userMessage);
        signal.roomName = nameof(TutorChatbot);

        if (flowchart != null)
        {
            int currentCorrectCount = Mathf.Clamp(ReadCorrectAnswerCount(), 0, TutorQuizTargetCorrectCount);
            float denom = Mathf.Max(1, TutorQuizTargetCorrectCount);
            signal.progressScore = currentCorrectCount / denom;
            signal.accuracyScore = currentCorrectCount / denom;
        }

        return signal;
    }

    protected override void OnChatHttpWaitStarted()
    {
        _tutorWaitingOnHttp = true;
    }

    protected override void OnChatHttpWaitFinished()
    {
        _tutorWaitingOnHttp = false;
    }

    protected override void AugmentChatPayload(LocalLlamaPayload payload, string userMessage)
    {
        if (flowchart == null || !flowchart.GetBooleanVariable("WindowClicked"))
            return;

        payload.rag_profile = "tutor";
        string qid = ResolveCurrentQuestionIdFromOrderAsset();
        if (!string.IsNullOrWhiteSpace(qid))
            payload.current_question_id = qid.Trim();
    }

    /// <summary>출제 순서 파일에서 CorrectAnswerCount 인덱스의 question_id를 가져옵니다.</summary>
    private string ResolveCurrentQuestionIdFromOrderAsset()
    {
        if (flowchart == null)
            return null;

        TextAsset asset = tutorQuestionOrderAsset != null
            ? tutorQuestionOrderAsset
            : Resources.Load<TextAsset>("TutorQuestionOrder");
        if (asset == null)
            return null;

        string[] rawLines = asset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
        var ids = new List<string>();
        foreach (string line in rawLines)
        {
            string t = line.Trim();
            if (t.Length == 0 || t.StartsWith("#", StringComparison.Ordinal))
                continue;
            ids.Add(t);
        }

        if (ids.Count == 0)
            return null;

        int idx = Mathf.Clamp(ReadCorrectAnswerCount() + _skipOrderOffset, 0, ids.Count - 1);
        return ids[idx];
    }

    protected override IEnumerator HandleChatbotResponse(string responseMessage, List<FunctionCallData> functionCalls)
    {
        if (chatSayDialog != null)
            chatSayDialog.Stop();

        // TutorRoomPrompt는 응답 끝에 JSON을 붙이도록 되어 있으나, 서버가 tool call을 안 줄 수 있음 → 본문에서 파싱
        string displayMessage = PrepareTutorDisplayAndQuizMetadata(
            responseMessage,
            out bool hasEmbeddedQuizJson,
            out bool embeddedIsCorrect,
            out bool embeddedQuizComplete);

        if (string.IsNullOrWhiteSpace(displayMessage) && hasEmbeddedQuizJson)
            displayMessage = " ";

        bool isComplete = false;
        Say(displayMessage, () => isComplete = true);
        yield return new WaitUntil(() => isComplete);

        ProcessTutorNonQuizToolsOnly(functionCalls);
        if (debugQuizProgress)
        {
            Debug.Log(
                $"[TutorQuiz] LLM 턴(채점은 /tutor/grade 전용): hasEmbeddedJson={hasEmbeddedQuizJson}, " +
                $"embeddedIsCorrect={embeddedIsCorrect}, CorrectAnswerCount={ReadCorrectAnswerCount()}");
        }

        if (flowchart != null && flowchart.GetBooleanVariable("WindowClicked"))
        {
            _expectingQuizAnswer = !IsTutorQuizFinished;
            if (hasEmbeddedQuizJson)
                _awaitingQuestionAdvance = ShouldOfferPanelAdvanceAfterTurn(
                    chatHistory, hasEmbeddedQuizJson, embeddedQuizComplete);
            else
                _awaitingQuestionAdvance = false;
            _lastEmbeddedQuizComplete = IsTutorQuizFinished;
        }

        // WindowClicked 튜터: 창 클릭만으로 Parret_Panel을 켜지 않음. 책상 등에서 패널이 이미 열린 뒤에만 포커스.
        if (_expectingQuizAnswer
            && !IsTutorQuizFinished
            && quizInputHandler != null
            && flowchart != null
            && flowchart.GetBooleanVariable("WindowClicked")
            && quizInputHandler.IsQuizInputPanelActive)
        {
            quizInputHandler.ActivateQuizInputField();
        }

        OnAIReponseComplete?.Invoke();
    }

    /// <summary>
    /// TutorRoomPrompt가 요구하는 응답 끝의 is_correct / quiz_complete JSON을 제거해 대사만 남기고, 값은 out으로 반환합니다.
    /// </summary>
    private static string PrepareTutorDisplayAndQuizMetadata(
        string raw,
        out bool hasMetadata,
        out bool isCorrect,
        out bool quizComplete)
    {
        hasMetadata = false;
        isCorrect = false;
        quizComplete = false;
        if (string.IsNullOrEmpty(raw))
            return raw;

        int marker = raw.LastIndexOf("\"is_correct\"", StringComparison.OrdinalIgnoreCase);
        if (marker < 0)
            return raw;

        int braceStart = raw.LastIndexOf('{', marker);
        if (braceStart < 0)
            return raw;

        if (!TryExtractBalancedJsonObject(raw, braceStart, out string jsonCandidate))
            return raw;

        jsonCandidate = jsonCandidate.Trim();
        try
        {
            JObject obj = JObject.Parse(jsonCandidate);
            JToken ic = obj["is_correct"];
            JToken qc = obj["quiz_complete"];
            if (ic == null || qc == null)
                return raw;

            hasMetadata = true;
            isCorrect = ic.Type == JTokenType.Boolean ? ic.Value<bool>() : bool.Parse(ic.ToString());
            quizComplete = qc.Type == JTokenType.Boolean ? qc.Value<bool>() : bool.Parse(qc.ToString());
            return raw.Substring(0, braceStart).TrimEnd();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[TutorChatbot] 튜터 응답 끝 JSON 파싱 실패: {e.Message}");
            return raw;
        }
    }

    /// <summary>
    /// 응답 끝에 <c>{"is_correct":...}</c> 뒤로 의성어·잔여 문자가 붙는 경우가 있어,
    /// 시작 <c>{</c>부터 짝이 맞는 <c>}</c>까지만 잘라 JSON으로 파싱합니다.
    /// </summary>
    private static bool TryExtractBalancedJsonObject(string raw, int openBrace, out string json)
    {
        json = null;
        if (openBrace < 0 || openBrace >= raw.Length || raw[openBrace] != '{')
            return false;

        int depth = 0;
        bool inString = false;
        bool escape = false;

        for (int i = openBrace; i < raw.Length; i++)
        {
            char c = raw[i];
            if (escape)
            {
                escape = false;
                continue;
            }

            if (inString)
            {
                if (c == '\\')
                    escape = true;
                else if (c == '"')
                    inString = false;
                continue;
            }

            if (c == '"')
            {
                inString = true;
                continue;
            }

            if (c == '{')
                depth++;
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                {
                    json = raw.Substring(openBrace, i - openBrace + 1);
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>정답 처리는 <see cref="CoGradeThenReact"/>만 담당. LLM의 update_quiz는 무시.</summary>
    private void ProcessTutorNonQuizToolsOnly(List<FunctionCallData> functionCalls)
    {
        if (functionCalls == null)
            return;

        foreach (var fc in functionCalls)
        {
            switch (fc.name)
            {
                case "update_quiz":
                    if (debugQuizProgress)
                        Debug.Log("[TutorQuiz] update_quiz 툴 호출은 무시합니다(채점은 /tutor/grade).");
                    break;
                case "emote":
                    HandleEmote(fc.arguments);
                    break;
                default:
                    Debug.Log($"Unhandled function call: {fc.name}");
                    break;
            }
        }
    }

    private void ApplyQuizResult(bool isCorrect, bool quizComplete)
    {
        int before = ReadCorrectAnswerCount();
        if (debugQuizProgress)
            Debug.Log(
                $"[TutorQuiz] ApplyQuizResult: isCorrect={isCorrect}, quizComplete={quizComplete}, " +
                $"CorrectAnswerCount(before)={before}, _skipOrderOffset(before)={_skipOrderOffset}");

        if (isCorrect)
        {
            _skipOrderOffset = 0;
            IncrementCorrectAnswerCount();
        }

        if (debugQuizProgress && isCorrect)
            Debug.Log($"[TutorQuiz] ApplyQuizResult: 정답 처리 후 CorrectAnswerCount={ReadCorrectAnswerCount()}, _skipOrderOffset={_skipOrderOffset}");

        TryFinalizeQuizSessionIfNeeded(quizComplete);
    }

    /// <summary>
    /// 게임 로직상 미션 완료는 Fungus 정답 누적(<see cref="TutorQuizTargetCorrectCount"/>)만 신뢰합니다.
    /// 모델의 <c>quiz_complete</c>는 UI·내러티브 힌트용이며, 조기 true여도 이벤트는 카운트 미달 시 발생하지 않습니다.
    /// </summary>
    private void TryFinalizeQuizSessionIfNeeded(bool modelSaidQuizComplete)
    {
        int n = ReadCorrectAnswerCount();
        if (modelSaidQuizComplete && n < TutorQuizTargetCorrectCount && debugQuizProgress)
        {
            Debug.LogWarning(
                $"[TutorQuiz] 모델이 quiz_complete=true였으나 CorrectAnswerCount={n} — " +
                $"OnQuizCompletedEvent는 정답 {TutorQuizTargetCorrectCount}회 도달 시에만 발생합니다.");
        }

        if (_quizCompletionEventFired || n < TutorQuizTargetCorrectCount)
            return;

        _quizCompletionEventFired = true;
        ResetTutorClickStateFlagsOnQuizComplete();
        OnQuizCompletedEvent?.Invoke();
        LockQuizInputAfterSessionComplete();
    }

    private void ResetTutorClickStateFlagsOnQuizComplete()
    {
        if (flowchart == null)
            return;

        if (flowchart.GetVariable(VarWindowClicked) is BooleanVariable)
            flowchart.SetBooleanVariable(VarWindowClicked, false);

        if (flowchart.GetVariable(VarIsClicked) is BooleanVariable)
            flowchart.SetBooleanVariable(VarIsClicked, false);
    }

    private void LockQuizInputAfterSessionComplete()
    {
        if (quizInputHandler == null)
            quizInputHandler = FindObjectOfType<QuizInputHandler>();
        if (quizInputHandler != null)
            quizInputHandler.SetQuizInputLocked(true);
    }

    /// <summary>5문제 완료 후 Parret 패널과 Fungus SayDialog를 끕니다.</summary>
    private void HideTutorQuizUiAfterSessionComplete()
    {
        if (quizInputHandler == null)
            quizInputHandler = FindObjectOfType<QuizInputHandler>();
        if (quizInputHandler != null)
            quizInputHandler.DeactivateQuizInputPanel();

        if (chatSayDialog == null)
            return;
        chatSayDialog.Stop();
        chatSayDialog.gameObject.SetActive(false);
    }

    private void HandleEmote(Dictionary<string, object> args)
    {
        if (args == null || !args.ContainsKey("emotion")) return;
        string emotion = args["emotion"].ToString();
        Debug.Log($"Chester emote: {emotion}");
        // TODO: wire to animation controller when Chester animations are ready
    }

    public void IncrementCorrectAnswerCount()
    {
        if (flowchart != null)
        {
            string key = ResolveCorrectAnswerCountKey();
            int currentCount = flowchart.GetIntegerVariable(key);
            if (currentCount >= TutorQuizTargetCorrectCount)
            {
                if (debugQuizProgress)
                    Debug.Log(
                        $"[TutorQuiz] IncrementCorrectAnswerCount: key={key}, 이미 {currentCount} (상한 {TutorQuizTargetCorrectCount}) — 증가 안 함");
                return;
            }

            WriteCorrectAnswerCount(currentCount + 1);
        }
    }
}
