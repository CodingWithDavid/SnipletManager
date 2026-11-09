#!/usr/bin/env python3
"""Generate a first-pass return contract consistency audit report."""

from __future__ import annotations

import argparse
import datetime as dt
import re
from dataclasses import dataclass
from pathlib import Path


EXCLUDED_DIRS = {
    ".git",
    ".agents",
    ".azuredevops",
    ".codex",
    ".copilot",
    ".github",
    ".squad",
    ".vs",
    "bin",
    "obj",
    "node_modules",
    "dist",
    "build",
    "coverage",
    "wwwroot",
}

SOURCE_SUFFIXES = {
    ".cs",
    ".razor",
    ".cshtml",
    ".vb",
    ".fs",
    ".ts",
    ".tsx",
    ".js",
    ".jsx",
}

PATTERNS = {
    "null_returns": re.compile(r"\breturn\s+null\s*;"),
    "default_returns": re.compile(r"\breturn\s+default(?:\s*\([^)]*\))?\s*;"),
    "empty_collection_returns": re.compile(
        r"\breturn\s+(?:\[\]|Array\.Empty<[^>]+>\(\)|Enumerable\.Empty<[^>]+>\(\)|new\s+List<[^>]+>\s*\(\))"
    ),
    "empty_object_returns": re.compile(r"\breturn\s+(?:new\s*\(\)|new\s+[A-Za-z_][A-Za-z0-9_<>.]*\s*\(\))\s*;"),
    "bool_returns": re.compile(r"\breturn\s+(?:true|false)\s*;"),
    "result_like_types": re.compile(
        r"\b(?:Result|Results|ServiceResult|OperationResult|ErrorOr|OneOf)<|"
        r"\b(?:IResult|ActionResult|ValidationResult)\b"
    ),
    "catch_blocks": re.compile(r"\bcatch\s*\([^)]*Exception[^)]*\)|\bcatch\s*\{"),
    "throw_statements": re.compile(r"\bthrow(?:\s+new|\s*;)"),
    "log_calls": re.compile(r"\b(?:Log|logger|_logger)\.(?:Error|Warning|Information|Critical|LogError|LogWarning|LogInformation|LogCritical)\b"),
}

METHOD_PATTERN = re.compile(
    r"\b(?:public|private|protected|internal)\s+"
    r"(?:static\s+)?(?:async\s+)?"
    r"(?P<return>[A-Za-z_][A-Za-z0-9_<>,.? \[\]]*)\s+"
    r"(?P<name>[A-Za-z_][A-Za-z0-9_]*)\s*\("
)


@dataclass(frozen=True)
class Hit:
    path: Path
    line_no: int
    line: str
    method: str | None


def is_source_file(path: Path) -> bool:
    if path.suffix not in SOURCE_SUFFIXES:
        return False
    if any(part.startswith(".") for part in path.parts):
        return False
    return not any(part in EXCLUDED_DIRS for part in path.parts)


def iter_source_files(repo: Path) -> list[Path]:
    return sorted(path for path in repo.rglob("*") if path.is_file() and is_source_file(path.relative_to(repo)))


def nearest_method(lines: list[str], index: int) -> str | None:
    start = max(0, index - 80)
    for i in range(index, start - 1, -1):
        match = METHOD_PATTERN.search(lines[i])
        if match:
            return f"{match.group('name')} : {match.group('return').strip()}"
    return None


def collect_hits(files: list[Path]) -> dict[str, list[Hit]]:
    hits: dict[str, list[Hit]] = {key: [] for key in PATTERNS}
    for path in files:
        try:
            text = path.read_text(encoding="utf-8")
        except UnicodeDecodeError:
            text = path.read_text(encoding="utf-8-sig", errors="replace")
        lines = text.splitlines()
        for index, line in enumerate(lines):
            for key, pattern in PATTERNS.items():
                if pattern.search(line):
                    hits[key].append(
                        Hit(
                            path=path,
                            line_no=index + 1,
                            line=line.strip(),
                            method=nearest_method(lines, index),
                        )
                    )
    return hits


def rel(path: Path, repo: Path) -> str:
    return str(path.relative_to(repo)).replace("\\", "/")


def format_examples(hits: list[Hit], repo: Path, limit: int = 12) -> list[str]:
    rows: list[str] = []
    for hit in hits[:limit]:
        method = f" ({hit.method})" if hit.method else ""
        rows.append(f"- `{rel(hit.path, repo)}:{hit.line_no}`{method}: `{hit.line}`")
    if len(hits) > limit:
        rows.append(f"- ... {len(hits) - limit} more")
    return rows


def categorize_risks(hits: dict[str, list[Hit]]) -> list[str]:
    risks: list[str] = []
    if hits["empty_collection_returns"] and hits["catch_blocks"]:
        risks.append(
            "Empty collections appear alongside broad exception handling; verify that empty means valid no-data rather than hidden operational failure."
        )
    if hits["empty_object_returns"]:
        risks.append(
            "Empty/default object returns can make missing data look like a valid domain object unless callers check sentinel fields."
        )
    if hits["bool_returns"]:
        risks.append(
            "`bool` returns are common; audit command methods for cases where `false` means more than one thing, such as validation failure, not found, and exception."
        )
    if hits["null_returns"]:
        risks.append(
            "`null` is used in multiple contexts; check whether nullable annotations and interfaces clearly document when it is expected."
        )
    if not hits["result_like_types"]:
        risks.append(
            "No strong Result-style pattern was detected by the script; error details may be carried by logs or exceptions rather than return values."
        )
    return risks


def write_report(repo: Path, out_dir: Path, files: list[Path], hits: dict[str, list[Hit]]) -> Path:
    out_dir.mkdir(parents=True, exist_ok=True)
    stamp = dt.datetime.now().strftime("%Y-%m-%d-%H%M")
    report = out_dir / f"return-contract-audit-{stamp}.md"

    total_hits = sum(len(v) for v in hits.values())
    lines: list[str] = [
        "# Return Contract Consistency Audit",
        "",
        f"Generated: {dt.datetime.now().isoformat(timespec='seconds')}",
        f"Repository: `{repo}`",
        f"Source files scanned: {len(files)}",
        f"Pattern hits: {total_hits}",
        "",
        "## Executive Summary",
        "",
        "This is a first-pass static audit of how methods represent missing data, validation failure, operational failure, and successful no-data results. Review the examples below and refine conclusions with targeted source inspection before making broad changes.",
        "",
        "## Pattern Counts",
        "",
        "| Pattern | Count |",
        "| --- | ---: |",
    ]

    for key, values in hits.items():
        lines.append(f"| {key.replace('_', ' ')} | {len(values)} |")

    lines.extend(["", "## Key Risks", ""])
    risks = categorize_risks(hits)
    lines.extend(f"- {risk}" for risk in risks)

    lines.extend(["", "## Evidence By Pattern", ""])
    for key, values in hits.items():
        lines.extend([f"### {key.replace('_', ' ').title()}", ""])
        if values:
            lines.extend(format_examples(values, repo))
        else:
            lines.append("- No matches found.")
        lines.append("")

    lines.extend(
        [
            "## Recommended Convention",
            "",
            "- Collections: return empty only for valid no-data; preserve operational failures by throwing or returning a typed failure.",
            "- Single-item lookups: prefer `T?` for not found over empty domain objects.",
            "- Commands: use `Result` / `Result<T>` when callers need a failure reason; reserve `bool` for pure predicates.",
            "- Infrastructure and repository failures: avoid converting exceptions into valid-looking empty values.",
            "",
            "## Manual Follow-Up Checklist",
            "",
            "- Inspect repository/data-access methods first.",
            "- Compare interface signatures with implementation fallbacks.",
            "- Identify methods where one return value has multiple meanings.",
            "- Choose one cleanup target with low blast radius and add tests around the chosen contract.",
            "",
        ]
    )

    report.write_text("\n".join(lines), encoding="utf-8")
    return report


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo", default=".", help="Repository root to scan.")
    parser.add_argument("--out", default="documents", help="Output directory for the Markdown report.")
    args = parser.parse_args()

    repo = Path(args.repo).resolve()
    out_dir = (repo / args.out).resolve() if not Path(args.out).is_absolute() else Path(args.out).resolve()
    files = iter_source_files(repo)
    hits = collect_hits(files)
    report = write_report(repo, out_dir, files, hits)
    print(report)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
