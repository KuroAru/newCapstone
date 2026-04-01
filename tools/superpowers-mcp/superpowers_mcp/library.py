from __future__ import annotations

import re
from dataclasses import dataclass
from pathlib import Path

from superpowers_mcp.paths import resolve_superpowers_root, validate_root

# Prevent reading arbitrarily large markdown files through MCP.
MAX_MARKDOWN_BYTES = 512 * 1024

SLUG_RE = re.compile(r"^[a-z0-9][a-z0-9-]{0,127}$")


@dataclass(frozen=True)
class ItemMeta:
    id: str
    description: str
    path: Path


class SuperpowersLibraryError(Exception):
    pass


def _read_text_bounded(path: Path) -> str:
    data = path.read_bytes()
    if len(data) > MAX_MARKDOWN_BYTES:
        raise SuperpowersLibraryError(
            f"File too large ({len(data)} bytes, max {MAX_MARKDOWN_BYTES}): {path}"
        )
    return data.decode("utf-8", errors="replace")


def _parse_simple_frontmatter(raw: str) -> tuple[dict[str, str], str]:
    """
    Parse leading YAML-like frontmatter delimited by --- lines.
    Only supports single-line key: value pairs (sufficient for Superpowers SKILL.md).
    """
    if not raw.startswith("---"):
        return {}, raw
    end = raw.find("\n---", 3)
    if end == -1:
        return {}, raw
    block = raw[3:end].strip("\n")
    body = raw[end + 4 :].lstrip("\n")
    meta: dict[str, str] = {}
    for line in block.splitlines():
        line = line.strip()
        if not line or line.startswith("#"):
            continue
        if ":" not in line:
            continue
        key, _, rest = line.partition(":")
        key = key.strip()
        val = rest.strip().strip('"').strip("'")
        meta[key] = val
    return meta, body


class SuperpowersLibrary:
    def __init__(self, root: Path | None = None) -> None:
        self._root = resolve_superpowers_root(
            str(root) if root is not None else None
        )
        err = validate_root(self._root)
        if err:
            raise SuperpowersLibraryError(err)

    @property
    def root(self) -> Path:
        return self._root

    def _assert_slug(self, name: str, label: str) -> None:
        if not SLUG_RE.match(name):
            raise SuperpowersLibraryError(
                f"Invalid {label} id (use lowercase letters, digits, hyphen): {name!r}"
            )

    def list_skills(self) -> list[ItemMeta]:
        skills_dir = self._root / "skills"
        if not skills_dir.is_dir():
            return []
        out: list[ItemMeta] = []
        for child in sorted(skills_dir.iterdir(), key=lambda p: p.name):
            if not child.is_dir():
                continue
            skill_file = child / "SKILL.md"
            if not skill_file.is_file():
                continue
            sid = child.name
            if not SLUG_RE.match(sid):
                continue
            try:
                raw = _read_text_bounded(skill_file)
            except SuperpowersLibraryError:
                continue
            meta, _ = _parse_simple_frontmatter(raw)
            desc = meta.get("description", "").strip() or "(no description in frontmatter)"
            out.append(ItemMeta(id=sid, description=desc, path=skill_file))
        return out

    def get_skill(self, skill_id: str) -> str:
        self._assert_slug(skill_id, "skill")
        path = self._root / "skills" / skill_id / "SKILL.md"
        if not path.is_file():
            raise SuperpowersLibraryError(f"Unknown skill: {skill_id}")
        return _read_text_bounded(path)

    def _list_markdown_dir(self, sub: str) -> list[ItemMeta]:
        base = self._root / sub
        if not base.is_dir():
            return []
        out: list[ItemMeta] = []
        for path in sorted(base.glob("*.md"), key=lambda p: p.name):
            stem = path.stem
            if not SLUG_RE.match(stem):
                continue
            try:
                raw = _read_text_bounded(path)
            except SuperpowersLibraryError:
                continue
            meta, _ = _parse_simple_frontmatter(raw)
            desc = meta.get("description", "").strip() or "(no description in frontmatter)"
            out.append(ItemMeta(id=stem, description=desc, path=path))
        return out

    def list_commands(self) -> list[ItemMeta]:
        return self._list_markdown_dir("commands")

    def get_command(self, command_id: str) -> str:
        self._assert_slug(command_id, "command")
        path = self._root / "commands" / f"{command_id}.md"
        if not path.is_file():
            raise SuperpowersLibraryError(f"Unknown command: {command_id}")
        return _read_text_bounded(path)

    def list_agents(self) -> list[ItemMeta]:
        return self._list_markdown_dir("agents")

    def get_agent(self, agent_id: str) -> str:
        self._assert_slug(agent_id, "agent")
        path = self._root / "agents" / f"{agent_id}.md"
        if not path.is_file():
            raise SuperpowersLibraryError(f"Unknown agent: {agent_id}")
        return _read_text_bounded(path)

    def get_plugin_manifest(self) -> str:
        path = self._root / ".cursor-plugin" / "plugin.json"
        if not path.is_file():
            raise SuperpowersLibraryError("plugin.json not found under .cursor-plugin/")
        return _read_text_bounded(path)
