"""
FastMCP entry — exposes Superpowers skills/commands/agents as read-only tools.
"""

from __future__ import annotations

from mcp.server.fastmcp import FastMCP

from superpowers_mcp.library import SuperpowersLibrary, SuperpowersLibraryError

mcp = FastMCP("superpowers")

_lib: SuperpowersLibrary | None = None
_lib_error: str | None = None


def _get_lib() -> SuperpowersLibrary:
    global _lib, _lib_error
    if _lib_error is not None:
        raise SuperpowersLibraryError(_lib_error)
    if _lib is None:
        try:
            _lib = SuperpowersLibrary()
        except SuperpowersLibraryError as e:
            _lib_error = str(e)
            raise SuperpowersLibraryError(_lib_error) from e
    return _lib


def _err(e: Exception) -> str:
    return f"superpowers_mcp error: {e}"


@mcp.tool()
def superpowers_list_skills() -> str:
    """List Superpowers skills (id + description from SKILL.md frontmatter)."""
    try:
        lib = _get_lib()
        lines = [
            f"- **{s.id}**: {s.description}" for s in lib.list_skills()
        ]
        if not lines:
            return f"No skills found under {lib.root / 'skills'}."
        return "\n".join(lines)
    except SuperpowersLibraryError as e:
        return _err(e)


@mcp.tool()
def superpowers_get_skill(skill_id: str) -> str:
    """Load full SKILL.md for a skill (e.g. test-driven-development, brainstorming)."""
    try:
        return _get_lib().get_skill(skill_id.strip())
    except SuperpowersLibraryError as e:
        return _err(e)


@mcp.tool()
def superpowers_list_commands() -> str:
    """List Superpowers slash-style commands (.md under commands/)."""
    try:
        lib = _get_lib()
        lines = [f"- **{c.id}**: {c.description}" for c in lib.list_commands()]
        if not lines:
            return f"No commands found under {lib.root / 'commands'}."
        return "\n".join(lines)
    except SuperpowersLibraryError as e:
        return _err(e)


@mcp.tool()
def superpowers_get_command(command_id: str) -> str:
    """Load a command markdown file by id (stem, e.g. brainstorm)."""
    try:
        return _get_lib().get_command(command_id.strip())
    except SuperpowersLibraryError as e:
        return _err(e)


@mcp.tool()
def superpowers_list_agents() -> str:
    """List Superpowers agent definitions (.md under agents/)."""
    try:
        lib = _get_lib()
        lines = [f"- **{a.id}**: {a.description}" for a in lib.list_agents()]
        if not lines:
            return f"No agents found under {lib.root / 'agents'}."
        return "\n".join(lines)
    except SuperpowersLibraryError as e:
        return _err(e)


@mcp.tool()
def superpowers_get_agent(agent_id: str) -> str:
    """Load an agent markdown file by id (e.g. code-reviewer)."""
    try:
        return _get_lib().get_agent(agent_id.strip())
    except SuperpowersLibraryError as e:
        return _err(e)


@mcp.tool()
def superpowers_get_manifest() -> str:
    """Return .cursor-plugin/plugin.json from the Superpowers clone (version, paths)."""
    try:
        return _get_lib().get_plugin_manifest()
    except SuperpowersLibraryError as e:
        return _err(e)


def main() -> None:
    mcp.run(transport="stdio")


if __name__ == "__main__":
    main()
