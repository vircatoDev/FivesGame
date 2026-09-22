#!/usr/bin/env python3
"""Reject blank lines in text prefabs/scenes: UnityYAML does not support them.

This is a format guard, not a substitute for importing assets in Unity.
Usage: python3 tools/check_unity_text.py [Assets directory]
"""
import sys
from pathlib import Path


def check(assets):
    files = sorted([*assets.rglob("*.prefab"), *assets.rglob("*.unity")])
    failures = []
    for path in files:
        text = path.read_text(encoding="utf-8-sig")
        if not text.startswith("%YAML"):
            failures.append(f"{path}: expected a text-serialized Unity asset")
            continue
        for number, line in enumerate(text.splitlines(), 1):
            if not line.strip():
                failures.append(f"{path}:{number}: blank line is unsupported by UnityYAML")
    if not files:
        failures.append(f"{assets}: no prefab or scene files found")
    for failure in failures:
        print(failure, file=sys.stderr)
    print(f"Unity text guard: {len(files)} files, {len(failures)} errors")
    return 1 if failures else 0


if __name__ == "__main__":
    assets = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(__file__).resolve().parents[1] / "Assets"
    sys.exit(check(assets))
