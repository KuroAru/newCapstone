public static class HintInformationPolicy
{
    public static string BuildPromptBlock(PlayerSkillProfile profile)
    {
        if (profile == null)
            return string.Empty;

        switch (profile.level)
        {
            case PlayerSkillLevel.Novice:
                return "\n\n[정보량 정책]\n- 플레이어가 막혀 있습니다. 다음 행동 대상을 더 직접적으로 제시하세요.\n- 단, 정답을 노출하지 말고 1~2단계의 추론 여지는 남기세요.";
            case PlayerSkillLevel.Intermediate:
                return "\n\n[정보량 정책]\n- 단서의 방향은 주되, 핵심 해답은 직접 말하지 마세요.\n- 플레이어가 스스로 연결할 수 있도록 중간 힌트 중심으로 답하세요.";
            default:
                return "\n\n[정보량 정책]\n- 정보 노출을 최소화하고 수수께끼성을 유지하세요.\n- 정답 유도보다 탐색 동기를 유지하는 말투를 우선하세요.";
        }
    }
}
