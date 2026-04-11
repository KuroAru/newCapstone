from __future__ import annotations

from pathlib import Path

import pytest

from config import Settings
from models.requests import TutorGradeRequest
from services.quiz_bank import QuizBank
from services.tutor_grade import grade_tutor_answer


@pytest.fixture
def bank(tmp_path: Path) -> QuizBank:
    p = tmp_path / "b.csv"
    p.write_text(
        "question_id,question_ko,acceptable_answers,reference_snippet,difficulty,tags\n"
        "Q002,거인?,골리앗|골리엇,다윗과 골리앗,,",
        encoding="utf-8",
    )
    return QuizBank.load(p)


def test_grade_correct_goliath(bank: QuizBank) -> None:
    r = grade_tutor_answer(
        TutorGradeRequest(question_id="Q002", user_answer="골리앗", correct_count_before=1),
        bank,
        Settings(),
    )
    assert r.is_correct is True
    assert r.unknown_question is False
    assert "다윗" in r.reference_snippet
    assert r.quiz_complete_after is False


def test_grade_wrong(bank: QuizBank) -> None:
    r = grade_tutor_answer(
        TutorGradeRequest(question_id="Q002", user_answer="삼손", correct_count_before=1),
        bank,
        Settings(),
    )
    assert r.is_correct is False
    assert r.quiz_complete_after is False


def test_quiz_complete_after_fifth(bank: QuizBank) -> None:
    r = grade_tutor_answer(
        TutorGradeRequest(question_id="Q002", user_answer="골리앗", correct_count_before=4, quiz_target=5),
        bank,
        Settings(),
    )
    assert r.is_correct is True
    assert r.quiz_complete_after is True


def test_unknown_question(bank: QuizBank) -> None:
    r = grade_tutor_answer(
        TutorGradeRequest(question_id="Q999", user_answer="x", correct_count_before=0),
        bank,
        Settings(),
    )
    assert r.unknown_question is True
    assert r.is_correct is False
