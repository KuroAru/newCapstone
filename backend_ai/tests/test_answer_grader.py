from __future__ import annotations

import pytest

from services.answer_grader import grade_user_answer


@pytest.mark.parametrize(
    ("user", "expected_ok"),
    [
        ("예수", True),
        ("예수님", True),
        ("  예수  ", True),
        ("모세", False),
        ("애굽", False),
    ],
)
def test_grade_aliases_and_normalize(user: str, expected_ok: bool) -> None:
    acceptable = ("예수", "예수님", "그리스도")
    ok = grade_user_answer(user, acceptable, fuzzy_ratio=0.82, fuzzy_max_len=24)
    assert ok is expected_ok


def test_grade_fuzzy_small_typo() -> None:
    acceptable = ("abcd",)
    ok = grade_user_answer("abdc", acceptable, fuzzy_ratio=0.75, fuzzy_max_len=24)
    assert ok is True


def test_grade_substring() -> None:
    acceptable = ("예수",)
    assert grade_user_answer("예수님", acceptable, fuzzy_ratio=0.82, fuzzy_max_len=24) is True
