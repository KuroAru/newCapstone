from __future__ import annotations

import json
from pathlib import Path

import pytest

from superpowers_mcp.library import (
    MAX_MARKDOWN_BYTES,
    SuperpowersLibrary,
    SuperpowersLibraryError,
)


@pytest.fixture()
def fake_root(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> Path:
    root = tmp_path / "superpowers"
    (root / ".cursor-plugin").mkdir(parents=True)
    (root / ".cursor-plugin" / "plugin.json").write_text(
        json.dumps({"name": "superpowers", "version": "0.0.0"}), encoding="utf-8"
    )
    skill_dir = root / "skills" / "demo-skill"
    skill_dir.mkdir(parents=True)
    (skill_dir / "SKILL.md").write_text(
        "---\nname: demo-skill\ndescription: A demo\n---\n\nBody here.\n",
        encoding="utf-8",
    )
    (root / "commands").mkdir()
    (root / "commands" / "demo-cmd.md").write_text(
        "---\ndescription: Cmd desc\n---\n\ncmd body\n", encoding="utf-8"
    )
    (root / "agents").mkdir()
    (root / "agents" / "demo-agent.md").write_text(
        "---\ndescription: Agent desc\n---\n\nagent body\n", encoding="utf-8"
    )
    monkeypatch.setenv("SUPERPOWERS_ROOT", str(root))
    return root


def test_list_and_get_skill(fake_root: Path) -> None:
    lib = SuperpowersLibrary()
    skills = lib.list_skills()
    assert len(skills) == 1
    assert skills[0].id == "demo-skill"
    assert "demo" in skills[0].description.lower()
    text = lib.get_skill("demo-skill")
    assert "Body here." in text


def test_get_skill_invalid_id(fake_root: Path) -> None:
    lib = SuperpowersLibrary()
    with pytest.raises(SuperpowersLibraryError):
        lib.get_skill("../etc/passwd")


def test_commands_and_agents(fake_root: Path) -> None:
    lib = SuperpowersLibrary()
    cmds = lib.list_commands()
    assert [c.id for c in cmds] == ["demo-cmd"]
    assert "cmd body" in lib.get_command("demo-cmd")
    ag = lib.list_agents()
    assert [a.id for a in ag] == ["demo-agent"]
    assert "agent body" in lib.get_agent("demo-agent")


def test_unknown_skill(fake_root: Path) -> None:
    lib = SuperpowersLibrary()
    with pytest.raises(SuperpowersLibraryError):
        lib.get_skill("missing")


def test_missing_root(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    bad = tmp_path / "nope"
    bad.mkdir()
    monkeypatch.setenv("SUPERPOWERS_ROOT", str(bad))
    with pytest.raises(SuperpowersLibraryError):
        SuperpowersLibrary()


def test_file_too_large(fake_root: Path) -> None:
    lib = SuperpowersLibrary()
    skill_path = lib.root / "skills" / "demo-skill" / "SKILL.md"
    skill_path.write_bytes(b"x" * (MAX_MARKDOWN_BYTES + 1))
    with pytest.raises(SuperpowersLibraryError):
        lib.get_skill("demo-skill")
