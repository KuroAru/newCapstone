"""
CI Error Collector
------------------
GitHub Actions 내부에서 실행되어 Python/C# 검사 결과를
구조화된 JSON error report로 변환합니다.

Usage (CI에서 자동 호출):
    python scripts/collect-errors.py \
        --type python \
        --branch main \
        --commit abc123 \
        --syntax-log /tmp/python_syntax.log \
        --ruff-json /tmp/ruff_lint.json
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

OUTPUT_PATH = Path("/tmp/error-report.json")

SYNTAX_ERROR_PATTERN = re.compile(
    r"ERROR\|(?P<file>[^|]+)\|(?P<message>.+)"
)
PYTHON_COMPILE_PATTERN = re.compile(
    r"File \"(?P<file>[^\"]+)\", line (?P<line>\d+)"
)
CSHARP_BRACE_PATTERN = re.compile(
    r"Unmatched braces: (?P<open>\d+) opening vs (?P<close>\d+) closing"
)


def parse_syntax_log(log_path: str, lang: str) -> list[dict[str, Any]]:
    errors: list[dict[str, Any]] = []
    path = Path(log_path)
    if not path.exists():
        return errors

    for raw_line in path.read_text(encoding="utf-8", errors="replace").splitlines():
        match = SYNTAX_ERROR_PATTERN.match(raw_line)
        if not match:
            continue

        file_path = match.group("file").strip()
        message = match.group("message").strip()
        line_num = 0
        column = 0

        line_match = PYTHON_COMPILE_PATTERN.search(message)
        if line_match:
            line_num = int(line_match.group("line"))

        error_type = f"{lang}_syntax"
        if CSHARP_BRACE_PATTERN.search(message):
            error_type = "csharp_brace_mismatch"

        errors.append({
            "type": error_type,
            "file": file_path,
            "line": line_num,
            "column": column,
            "message": message,
            "severity": "error",
        })

    return errors


def parse_ruff_json(json_path: str) -> list[dict[str, Any]]:
    errors: list[dict[str, Any]] = []
    path = Path(json_path)
    if not path.exists():
        return errors

    try:
        raw = path.read_text(encoding="utf-8", errors="replace").strip()
        if not raw or raw == "[]":
            return errors
        data = json.loads(raw)
    except json.JSONDecodeError:
        return errors

    for item in data:
        severity = "warning"
        if item.get("code", "").startswith("E"):
            severity = "error"

        errors.append({
            "type": "python_lint",
            "file": item.get("filename", ""),
            "line": item.get("location", {}).get("row", 0),
            "column": item.get("location", {}).get("column", 0),
            "message": f"[{item.get('code', '?')}] {item.get('message', '')}",
            "severity": severity,
            "rule": item.get("code", ""),
            "fix_available": item.get("fix") is not None,
        })

    return errors


def build_report(
    *,
    branch: str,
    commit: str,
    errors: list[dict[str, Any]],
) -> dict[str, Any]:
    return {
        "schema_version": "1.0",
        "branch": branch,
        "commit": commit,
        "timestamp": datetime.now(timezone.utc).isoformat(),
        "total_errors": len([e for e in errors if e["severity"] == "error"]),
        "total_warnings": len([e for e in errors if e["severity"] == "warning"]),
        "errors": errors,
    }


def main() -> int:
    parser = argparse.ArgumentParser(description="Collect CI errors into JSON report")
    parser.add_argument("--type", required=True, choices=["python", "csharp"])
    parser.add_argument("--branch", required=True)
    parser.add_argument("--commit", required=True)
    parser.add_argument("--syntax-log", default="")
    parser.add_argument("--ruff-json", default="")
    args = parser.parse_args()

    all_errors: list[dict[str, Any]] = []

    if args.syntax_log:
        all_errors.extend(parse_syntax_log(args.syntax_log, args.type))

    if args.ruff_json:
        all_errors.extend(parse_ruff_json(args.ruff_json))

    report = build_report(
        branch=args.branch,
        commit=args.commit,
        errors=all_errors,
    )

    OUTPUT_PATH.write_text(
        json.dumps(report, indent=2, ensure_ascii=False),
        encoding="utf-8",
    )

    print(f"Error report written to {OUTPUT_PATH}")
    print(f"  Branch: {report['branch']}")
    print(f"  Errors: {report['total_errors']}, Warnings: {report['total_warnings']}")

    return 1 if report["total_errors"] > 0 else 0


if __name__ == "__main__":
    sys.exit(main())
