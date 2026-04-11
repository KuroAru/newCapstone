from __future__ import annotations

import re
from difflib import SequenceMatcher


def normalize_answer(text: str) -> str:
    s = (text or "").strip().lower()
    s = re.sub(r"\s+", " ", s)
    return s


def _digits_only(text: str) -> str:
    return re.sub(r"\D", "", text)


def _fuzzy_ratio(a: str, b: str) -> float:
    if not a or not b:
        return 0.0
    return SequenceMatcher(None, a, b).ratio()


def _contains_loose(user: str, candidate: str) -> bool:
    """Short substring match (e.g. 예수 in 예수님)."""
    if len(user) < 2 or len(candidate) < 2:
        return False
    return user in candidate or candidate in user


def grade_user_answer(
    user_raw: str,
    acceptable: tuple[str, ...],
    *,
    fuzzy_ratio: float,
    fuzzy_max_len: int,
) -> bool:
    """
    Return True if the user answer should be scored correct (aliases, normalize, fuzzy, substring).
    Used to override LLM update_quiz when confident.
    """
    user = normalize_answer(user_raw)
    if not user:
        return False

    user_digits = _digits_only(user)
    for cand_raw in acceptable:
        cand = normalize_answer(cand_raw)
        if not cand:
            continue
        if user == cand:
            return True
        if user_digits and user_digits == _digits_only(cand):
            return True
        if _contains_loose(user, cand):
            return True
        max_len = max(len(user), len(cand))
        if max_len <= fuzzy_max_len and _fuzzy_ratio(user, cand) >= fuzzy_ratio:
            return True

    return False
