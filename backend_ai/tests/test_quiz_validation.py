from __future__ import annotations

from pathlib import Path

from services.quiz_validation import validate_quiz_bank_csv


def test_validate_good_csv(tmp_path: Path) -> None:
    p = tmp_path / "good.csv"
    p.write_text(
        "question_id,question_ko,acceptable_answers,reference_snippet,difficulty,tags\n"
        "A1,질문?,정답|답,참고,1,t\n",
        encoding="utf-8",
    )
    assert validate_quiz_bank_csv(p) == []


def test_validate_duplicate_id(tmp_path: Path) -> None:
    p = tmp_path / "bad.csv"
    p.write_text(
        "question_id,question_ko,acceptable_answers,reference_snippet\n"
        "X,q,a,r\n"
        "X,q2,a2,r2\n",
        encoding="utf-8",
    )
    errs = validate_quiz_bank_csv(p)
    assert any("Duplicate" in e for e in errs)


def test_validate_missing_column(tmp_path: Path) -> None:
    p = tmp_path / "bad2.csv"
    p.write_text("question_id,question_ko\nA1,q\n", encoding="utf-8")
    errs = validate_quiz_bank_csv(p)
    assert any("Missing required column" in e for e in errs)
