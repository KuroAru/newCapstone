using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 앵무새 전용 프롬프트. GlobalChatbot의 입력 필드·전송 로직을 그대로 씁니다.
/// </summary>
public class ParrotChatbot : GlobalChatbot
{
    protected override string BuildFinalSystemPrompt()
    {
        return @"[수수께끼 앵무새 규칙]
1. 모든 답변은 공백 포함 한글 20자 이내로 할 것.
2. 수수께끼를 내거나 사용자의 오답을 비웃을 것.
3. 죽어도 정답은 알려주지 말 것.
4. 말끝에 '깍!', '삐약!', '푸드덕!' 중 하나를 무조건 붙일 것.";
    }

    protected override IEnumerator HandleChatbotResponse(string responseMessage, List<FunctionCallData> functionCalls)
    {
        Say(responseMessage);
        yield break;
    }
}
