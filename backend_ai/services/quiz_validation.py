from __future__ import annotations

import csv
from collections import Counter
from pathlib import Path

REQUIRED_COLUMNS = ("question_id", "question_ko", "acceptable_answers", "reference_snippet")


def validate_quiz_bank_csv(path: Path) -> list[str]:
    errors: list[str] = []
    if not path.is_file():
        return [f"Missing file: {path}"]

    with path.open(encoding="utf-8-sig", newline="") as f:
        reader = csv.DictReader(f)
        if reader.fieldnames is None:
            return ["CSV has no header row"]
        for col in REQUIRED_COLUMNS:
            if col not in reader.fieldnames:
                errors.append(f"Missing required column: {col}")

        if errors:
            return errors

        rows = list(reader)
        if not rows:
            errors.append("CSV has no data rows")
            return errors

        ids = [r.get("question_id", "").strip() for r in rows]
        dupes = [k for k, v in Counter(ids).items() if v > 1 and k]
        if dupes:
            errors.append(f"Duplicate question_id values: {', '.join(dupes)}")

        for i, r in enumerate(rows, start=2):
            qid = (r.get("question_id") or "").strip()
            qko = (r.get("question_ko") or "").strip()
            acc = (r.get("acceptable_answers") or "").strip()
            if not qid:
                errors.append(f"Row {i}: empty question_id")
            if not qko:
                errors.append(f"Row {i}: empty question_ko")
            if not acc:
                errors.append(f"Row {i}: empty acceptable_answers")

    return errors
