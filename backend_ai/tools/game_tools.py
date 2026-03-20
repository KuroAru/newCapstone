from __future__ import annotations

GIVE_HINT_TOOL: dict = {
    "type": "function",
    "function": {
        "name": "give_hint",
        "description": (
            "플레이어에게 퍼즐 힌트를 줄 때 호출합니다. "
            "Unity가 target_object를 시각적으로 강조하거나 카메라를 이동시킵니다."
        ),
        "parameters": {
            "type": "object",
            "properties": {
                "hint_level": {
                    "type": "string",
                    "enum": ["subtle", "moderate", "direct"],
                    "description": "힌트의 노골적인 정도. subtle=은유적, moderate=중간, direct=직접적",
                },
                "target_object": {
                    "type": "string",
                    "description": (
                        "힌트가 가리키는 게임 오브젝트 이름. "
                        "예: bed, drawer, safe, mirror, bookshelf, horse, "
                        "window_maria, poster_lazarus, painting_martha, bottle, "
                        "rotation_pad, utility_door, ingredients, bible_book, "
                        "curry_pot, diary, dressing_room, roman_numeral, stairs, map"
                    ),
                },
                "hint_category": {
                    "type": "string",
                    "enum": ["location", "puzzle", "item"],
                    "description": "힌트 유형. location=위치 안내, puzzle=퍼즐 풀이, item=아이템 관련",
                },
            },
            "required": ["hint_level", "target_object", "hint_category"],
        },
    },
}

UPDATE_QUIZ_TOOL: dict = {
    "type": "function",
    "function": {
        "name": "update_quiz",
        "description": (
            "TutorRoom 성경 퀴즈에서 플레이어의 답변을 평가한 뒤 반드시 호출합니다. "
            "매 응답마다 반드시 이 함수를 호출하여 정답 여부와 퀴즈 완료 여부를 알려야 합니다."
        ),
        "parameters": {
            "type": "object",
            "properties": {
                "is_correct": {
                    "type": "boolean",
                    "description": "플레이어의 방금 답변이 정답이면 true",
                },
                "quiz_complete": {
                    "type": "boolean",
                    "description": "이 답변으로 총 5회 정답 달성 시 true",
                },
            },
            "required": ["is_correct", "quiz_complete"],
        },
    },
}

EMOTE_TOOL: dict = {
    "type": "function",
    "function": {
        "name": "emote",
        "description": (
            "체셔 앵무새의 감정 표현을 Unity에 전달합니다. "
            "대답의 톤에 맞는 감정을 선택하세요."
        ),
        "parameters": {
            "type": "object",
            "properties": {
                "emotion": {
                    "type": "string",
                    "enum": ["mock", "laugh", "angry", "surprised", "helpful", "sad"],
                    "description": "체셔의 감정 상태",
                },
            },
            "required": ["emotion"],
        },
    },
}

GAME_TOOLS: list[dict] = [GIVE_HINT_TOOL, UPDATE_QUIZ_TOOL, EMOTE_TOOL]
