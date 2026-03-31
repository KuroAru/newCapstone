"""
FastMCP entry — tools only; domain logic lives in automation.py.
"""

from __future__ import annotations

from typing import Any

from mcp.server.fastmcp import FastMCP

from hwp_mcp.automation import HwpAutomationError, fill_template, probe_hwp_com

mcp = FastMCP("hwp-automation")


@mcp.tool()
def hwp_fill_template(
    template_path: str,
    output_path: str,
    data: dict[str, Any],
) -> str:
    """
    Fill an HWP template's named fields (누름틀) and save a new .hwp file.

    - template_path: Absolute or relative path to the .hwp template
    - output_path: Target path for the new document (parent folders are created if needed)
    - data: Map of field name → text (values are converted to strings)
    """
    try:
        return fill_template(template_path, output_path, data)
    except HwpAutomationError as e:
        return f"Error: {e}"
    except Exception as e:
        return f"HWP error: {e}"


@mcp.tool()
def hwp_probe() -> str:
    """
    Check whether Hangul (HWP) COM automation is available on this machine.
    Run this before batch jobs if HWP was recently installed or updated.
    """
    try:
        return probe_hwp_com()
    except HwpAutomationError as e:
        return f"Error: {e}"
    except Exception as e:
        return f"HWP probe failed: {e}"


def main() -> None:
    mcp.run(transport="stdio")


if __name__ == "__main__":
    main()
