#!/usr/bin/env python3
"""Validate quiz_bank.csv (required columns, unique IDs, non-empty fields)."""

from __future__ import annotations

import sys
from pathlib import Path

_BACKEND_DIR = Path(__file__).resolve().parent.parent
if str(_BACKEND_DIR) not in sys.path:
    sys.path.insert(0, str(_BACKEND_DIR))

from services.quiz_validation import validate_quiz_bank_csv  # noqa: E402

_DEFAULT_CSV = _BACKEND_DIR / "data" / "tutor_quiz" / "quiz_bank.csv"


def main() -> int:
    path = Path(sys.argv[1]).resolve() if len(sys.argv) > 1 else _DEFAULT_CSV
    errs = validate_quiz_bank_csv(path)
    if errs:
        print("Validation failed:", file=sys.stderr)
        for e in errs:
            print(f"  - {e}", file=sys.stderr)
        return 1
    print(f"OK: {path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
