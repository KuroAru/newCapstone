from __future__ import annotations

import csv
import logging
from dataclasses import dataclass
from pathlib import Path

logger = logging.getLogger(__name__)


@dataclass(frozen=True)
class QuizRow:
    question_id: str
    question_ko: str
    acceptable_answers: tuple[str, ...]
    reference_snippet: str
    difficulty: str
    tags: str


def _split_acceptable(raw: str) -> tuple[str, ...]:
    parts = [p.strip() for p in raw.split("|") if p.strip()]
    return tuple(parts)


class QuizBank:
    def __init__(self, rows: dict[str, QuizRow]) -> None:
        self._rows = rows

    @classmethod
    def load(cls, path: Path) -> QuizBank:
        if not path.is_file():
            logger.warning("Quiz bank CSV not found: %s — using empty bank", path)
            return cls({})

        rows: dict[str, QuizRow] = {}
        with path.open(encoding="utf-8-sig", newline="") as f:
            reader = csv.DictReader(f)
            for r in reader:
                qid = (r.get("question_id") or "").strip()
                if not qid:
                    continue
                rows[qid] = QuizRow(
                    question_id=qid,
                    question_ko=(r.get("question_ko") or "").strip(),
                    acceptable_answers=_split_acceptable(r.get("acceptable_answers") or ""),
                    reference_snippet=(r.get("reference_snippet") or "").strip(),
                    difficulty=(r.get("difficulty") or "").strip(),
                    tags=(r.get("tags") or "").strip(),
                )
        logger.info("Loaded %d quiz bank rows from %s", len(rows), path)
        return cls(rows)

    def get(self, question_id: str) -> QuizRow | None:
        return self._rows.get(question_id.strip())

    def format_bank_context_block(self, row: QuizRow) -> str:
        """Injected into tutor system prompt — question text must match bank; do not reveal answers."""
        return (
            "[문제 은행 — 반드시 준수]\n"
            f"- question_id: {row.question_id}\n"
            "- **대사 순서(매 응답 공통)**: (1) 시스템의 [현재 진행 상황]과 **같은 숫자**로 누적 정답 안내 한 줄 "
            "(예: 「현재 N/5…」) 또는 직전 답에 대한 정오·격려 (2) 짧은 전환 한 마디 (3) **그 다음에만** 아래 질문 문장을 "
            "**글자·띄어쓰기까지 동일하게** 한 줄로 말합니다. (3)만 단독으로 내거나 질문만 던지면 **금지**입니다.\n"
            f"- 질문(은행 원문, 변경 금지): {row.question_ko}\n"
            f"- 참고(출처 요지, 플레이어에게 그대로 읽어 주지 말 것): {row.reference_snippet}\n"
            "- 정답 후보 목록은 비밀이며 대사에 절대 포함하지 마세요. 정오는 참고 자료·은행 의도에 맞게 판단하세요."
        )
