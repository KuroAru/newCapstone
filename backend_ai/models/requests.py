from __future__ import annotations

from pydantic import BaseModel, Field


class ChatRequest(BaseModel):
    prompt: str = Field(..., min_length=1, max_length=2000)
    system: str = "당신은 저택의 도우미입니다."
    use_tools: bool = True
    rag_profile: str | None = None
    rag_query: str | None = Field(None, max_length=2000)
    current_question_id: str | None = Field(None, max_length=128)
    rag_top_k: int | None = Field(None, ge=1, le=20)


class TutorGradeRequest(BaseModel):
    question_id: str = Field(..., min_length=1, max_length=128)
    user_answer: str = Field(..., min_length=1, max_length=2000)
    correct_count_before: int = Field(default=0, ge=0, le=100)
    quiz_target: int = Field(default=5, ge=1, le=50)
