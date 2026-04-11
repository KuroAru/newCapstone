using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Fungus;

public class QuizInputHandler : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField inputField; // Unity Inspector에서 Input Field를 여기에 연결합니다.
    [SerializeField] private GameObject inputPanel; // Input Field를 감싸는 부모 패널 (Input Field 자체일 수도 있습니다)

    [Header("Fungus Integration")]
    [SerializeField] private Flowchart targetFlowchart; // 이 스크립트가 속한 Flowchart 또는 제어할 Flowchart
    [SerializeField] private string fungusVariableName = "PlayerAnswer"; // Fungus String 변수 이름
    [SerializeField] private TutorChatbot tutorChatbot; // TutorChatbot 스크립트 인스턴스

    [Header("Debug")]
    [Tooltip("TMP 필드 텍스트 변경·제출 시 콘솔 로그(끄면 릴리즈 부담 없음).")]
    [SerializeField] private bool debugLogInputField = true;

    [Header("Quiz 완료")]
    [Tooltip("전송 버튼이 별도 오브젝트일 때 비활성화합니다. 비우면 무시.")]
    [SerializeField] private GameObject sendButtonObject;

    private bool _quizInputLocked;

    /// <summary>튜터 미션 완료 후 호출 — 추가 제출·빈 패널 진행을 막습니다.</summary>
    public void SetQuizInputLocked(bool locked)
    {
        _quizInputLocked = locked;
        if (inputField != null)
            inputField.interactable = !locked;
        if (sendButtonObject != null)
            sendButtonObject.SetActive(!locked);
    }

    public bool IsQuizInputLocked => _quizInputLocked;

    /// <summary>튜터 입력 패널(Parret_Panel 등)이 이미 켜져 있는지 — 창 클릭 직후 AI만으로는 패널을 자동 오픈하지 않을 때 사용.</summary>
    public bool IsQuizInputPanelActive => inputPanel != null && inputPanel.activeInHierarchy;

    /// <summary>onSubmit이 같은 프레임에 두 번 호출될 때 중복 전송 방지.</summary>
    private int _lastHandledSubmitFrame = -1;

    /// <summary>제출 코루틴(IME 플러시) 중복 실행 방지.</summary>
    private bool _quizSubmitCoroutineRunning;

    void Awake()
    {
        if (inputField == null)
        {
            // 같은 오브젝트에 TMP_InputField가 있으면 자동으로 찾기 시도
            inputField = GetComponent<TMP_InputField>();
        }
        if (inputPanel == null)
        {
            inputPanel = inputField.gameObject; // InputField 자체를 패널로 사용
        }

        if (inputField != null)
        {
            // TMP onSubmit은 한글 IME가 조합 확정용 Enter를 쓸 때도 호출되는 경우가 있어 제거함.
            // 키보드 전송은 Update에서 Return + 조합 비어 있을 때만 처리(아래).
            inputField.interactable = true; // 상호작용 강제 활성화
            // 한글·IME: 포커스마다 전체 선택이면 다음 글자가 기존 텍스트를 통째로 지움(‘세 번째 단어’ 증상).
            inputField.onFocusSelectAll = false;
            // 포커스가 잠깐 빠질 때 텍스트가 비워지는 현상 완화
            inputField.resetOnDeActivation = false;
            // inputPanel.SetActive(false); // [수정] 시작 시 자동 비활성화 제거 (테스트 및 버그 방지)
            if (debugLogInputField)
                inputField.onValueChanged.AddListener(OnInputFieldValueChangedForDebug);
        }
        else
        {
            Debug.LogError("QuizInputHandler: InputField가 연결되지 않았습니다!");
        }

        AttachSayDialogSyncToPanel();
    }

    private static string EscapeForDebugLog(string s)
    {
        if (s == null)
            return "(null)";
        return s
            .Replace("\\", "\\\\")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\"", "\\\"");
    }

    private void OnInputFieldValueChangedForDebug(string newText)
    {
        string comp = Input.compositionString ?? string.Empty;
        int len = newText != null ? newText.Length : 0;
        Debug.Log(
            $"[QuizInput] TMP onValueChanged: len={len}, IME composition=\"{EscapeForDebugLog(comp)}\", text=\"{EscapeForDebugLog(newText)}\"");
    }

    private void Update()
    {
        if (_quizInputLocked)
            return;
        if (inputField == null || !inputField.isFocused || _quizSubmitCoroutineRunning)
            return;
        if (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter))
            return;
        // 조합 중이면 Enter는 글자 확정용일 수 있음 — 전송으로 취급하지 않음
        if (!string.IsNullOrEmpty(Input.compositionString))
            return;
        OnInputSubmit();
    }

    /// <summary>
    /// Fungus가 패널만 SetActive 할 때 SayDialog가 같이 켜지고 꺼지도록 <see cref="TutorPanelSayDialogSync"/>를 붙입니다.
    /// </summary>
    private void AttachSayDialogSyncToPanel()
    {
        if (inputPanel == null)
            return;

        var tutor = tutorChatbot != null ? tutorChatbot : FindObjectOfType<TutorChatbot>();
        if (tutor == null)
            return;

        SayDialog say = tutor.TutorSayDialogForPanelSync;
        if (say == null)
            return;

        var sync = inputPanel.GetComponent<TutorPanelSayDialogSync>();
        if (sync == null)
            sync = inputPanel.AddComponent<TutorPanelSayDialogSync>();
        sync.Initialize(say);
        say.gameObject.SetActive(inputPanel.activeSelf);
    }

    /// <summary>
    /// 오브젝트가 활성화될 때마다 자동으로 포커스를 줍니다.
    /// </summary>
    void OnEnable()
    {
        if (_quizInputLocked)
            return;
        if (inputField != null)
        {
            inputField.ActivateInputField();
            inputField.Select();
        }
    }

    private void OnDestroy()
    {
        if (inputField != null && debugLogInputField)
            inputField.onValueChanged.RemoveListener(OnInputFieldValueChangedForDebug);
    }

    /// <summary>UI 버튼 등에서 호출 — 항상 필드의 현재 텍스트를 사용합니다(IME/onSubmit 빈 문자열 대응).</summary>
    public void SubmitQuizAnswerFromUI()
    {
        if (_quizInputLocked)
            return;
        OnInputSubmit();
    }

    private void OnInputSubmit(string _ = null)
    {
        if (_quizInputLocked)
            return;
        if (Time.frameCount == _lastHandledSubmitFrame)
            return;
        if (_quizSubmitCoroutineRunning)
            return;
        StartCoroutine(CoSubmitQuizAnswerWithImeFlush());
    }

    /// <summary>
    /// TMP <see cref="TMP_InputField.text"/>에는 아직 반영되지 않고 <see cref="Input.compositionString"/>에만 남은
    /// 마지막 한글 음절(예: '마리' + 조합 중 '아')을 합칩니다. 버튼 클릭으로 포커스가 빠지기 전에 동기 읽기만 하면 잘립니다.
    /// </summary>
    private bool IsEventSystemSelectingOurInputField()
    {
        if (inputField == null || EventSystem.current == null)
            return false;
        GameObject sel = EventSystem.current.currentSelectedGameObject;
        if (sel == null)
            return false;
        Transform root = inputField.transform;
        return sel == root.gameObject || sel.transform.IsChildOf(root);
    }

    private static string ReadQuizInputIncludingImeComposition(TMP_InputField field)
    {
        if (field == null)
            return string.Empty;
        string t = field.text ?? string.Empty;
        string comp = Input.compositionString ?? string.Empty;
        if (string.IsNullOrEmpty(comp))
            return t;
        if (t.EndsWith(comp, StringComparison.Ordinal))
            return t;
        return t + comp;
    }

    private IEnumerator CoSubmitQuizAnswerWithImeFlush()
    {
        if (_quizSubmitCoroutineRunning)
            yield break;
        if (_quizInputLocked)
            yield break;
        _quizSubmitCoroutineRunning = true;
        try
        {
            // Enter 제출 시 이미 인풋이 선택돼 있으면 Select/Activate가 IME 조합을 끊어 글자 단위로 깨질 수 있음 — 버튼 제출일 때만 포커스 복구
            if (inputField != null && !IsEventSystemSelectingOurInputField())
            {
                inputField.Select();
                inputField.ActivateInputField();
            }

            yield return null;
            yield return new WaitForEndOfFrame();
            yield return null;
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.03f);

            if (inputField == null)
                yield break;

            string mergedBeforeTrim = ReadQuizInputIncludingImeComposition(inputField);
            string inputText = mergedBeforeTrim.Trim();
            if (debugLogInputField)
            {
                string rawField = inputField.text ?? string.Empty;
                string comp = Input.compositionString ?? string.Empty;
                Debug.Log(
                    $"[QuizInput] submit read: field.text=\"{EscapeForDebugLog(rawField)}\" (len={rawField.Length}), " +
                    $"IME=\"{EscapeForDebugLog(comp)}\", merged=\"{EscapeForDebugLog(mergedBeforeTrim)}\", trimmed=\"{EscapeForDebugLog(inputText)}\"");
            }

            if (string.IsNullOrWhiteSpace(inputText))
            {
                var tutor = tutorChatbot != null ? tutorChatbot : FindObjectOfType<TutorChatbot>();
                if (tutor != null && tutor.TryHandleEmptyPanelSubmit())
                {
                    if (debugLogInputField)
                        Debug.Log("[QuizInput] 빈 제출 → TutorChatbot.TryHandleEmptyPanelSubmit 처리됨(다음 문제 진행).");
                    _lastHandledSubmitFrame = Time.frameCount;
                    yield break;
                }

                Debug.Log("빈 답변은 전송되지 않습니다.");
                inputField.Select();
                inputField.ActivateInputField();
                yield break;
            }

            if (Time.frameCount == _lastHandledSubmitFrame)
                yield break;

            _lastHandledSubmitFrame = Time.frameCount;
            ProcessSubmit(inputText);
        }
        finally
        {
            _quizSubmitCoroutineRunning = false;
        }
    }

    private void ProcessSubmit(string inputText)
    {
        if (inputField == null || string.IsNullOrWhiteSpace(inputText))
            return;

        var tutor = tutorChatbot != null ? tutorChatbot : FindObjectOfType<TutorChatbot>();
        if (tutor != null && tutor.IsTutorQuizFinished)
        {
            if (debugLogInputField)
                Debug.Log("[QuizInput] ProcessSubmit 건너뜀(튜터 퀴즈 이미 완료).");
            return;
        }

        if (tutor != null && tutor.IsAiResponseInFlight)
        {
            if (debugLogInputField)
                Debug.Log($"[QuizInput] ProcessSubmit 건너뜀(AI 응답 진행 중): \"{EscapeForDebugLog(inputText)}\"");
            return;
        }

        if (debugLogInputField)
            Debug.Log($"[QuizInput] ProcessSubmit 전송: \"{EscapeForDebugLog(inputText)}\" (len={inputText.Length})");

        // Fungus String 변수에 플레이어의 답변 저장
        if (targetFlowchart != null)
        {
            targetFlowchart.SetStringVariable(fungusVariableName, inputText);
            Debug.Log($"[QuizInput] Fungus '{fungusVariableName}' = \"{EscapeForDebugLog(inputText)}\"");
        }
        else
        {
            Debug.LogError("QuizInputHandler: Flowchart가 연결되지 않았습니다.");
        }

        inputField.text = "";

        if (tutor != null)
            tutor.AddPlayerMessageAndGetResponse(inputText);
        else
            Debug.LogError("QuizInputHandler: TutorChatbot이 연결되지 않았습니다.");
    }

    /// <summary>
    /// Fungus에서 호출하여 Input Field를 활성화하는 함수
    /// </summary>
    public void ActivateQuizInputField()
    {
        if (inputPanel != null)
        {
            inputPanel.SetActive(true);
            if (inputField != null)
            {
                inputField.Select();
                inputField.ActivateInputField();
            }
            Debug.Log("Quiz Input Field 활성화.");
        }
        else
        {
            Debug.LogError("QuizInputHandler: Input Panel이 연결되지 않았습니다!");
        }
    }

    /// <summary>미션 완료 등 — <see cref="TutorPanelSayDialogSync"/>로 SayDialog와 함께 정리됩니다.</summary>
    public void DeactivateQuizInputPanel()
    {
        if (inputPanel != null)
            inputPanel.SetActive(false);
    }
}