from __future__ import annotations

import os
from pathlib import Path

ENV_ROOT = "SUPERPOWERS_ROOT"
DEFAULT_RELATIVE_TO_HOME = Path(".cursor") / "plugins" / "local" / "superpowers"
MANIFEST_RELATIVE = Path(".cursor-plugin") / "plugin.json"


def resolve_superpowers_root(explicit: str | None = None) -> Path:
    """
    Root of the obra/superpowers clone.

    Order: explicit argument, env SUPERPOWERS_ROOT, then ~/.cursor/plugins/local/superpowers.
    """
    if explicit:
        return Path(explicit).expanduser().resolve()
    env = os.environ.get(ENV_ROOT, "").strip()
    if env:
        return Path(env).expanduser().resolve()
    return (Path.home() / DEFAULT_RELATIVE_TO_HOME).resolve()


def validate_root(root: Path) -> str | None:
    """Return error message if root does not look like superpowers, else None."""
    if not root.is_dir():
        return f"Superpowers root is not a directory: {root}"
    manifest = root / MANIFEST_RELATIVE
    skills = root / "skills"
    if not manifest.is_file() and not skills.is_dir():
        return (
            f"Missing expected Superpowers layout (no {MANIFEST_RELATIVE} and no skills/): {root}"
        )
    return None
