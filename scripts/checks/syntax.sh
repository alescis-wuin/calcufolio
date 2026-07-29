#!/usr/bin/env bash
set -Eeuo pipefail

SCRIPT_DIRECTORY="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
readonly SCRIPT_DIRECTORY
REPOSITORY_ROOT="$(git -C "$SCRIPT_DIRECTORY" rev-parse --show-toplevel)"
readonly REPOSITORY_ROOT
readonly COMMON_SCRIPT="$REPOSITORY_ROOT/scripts/lib/common.sh"

# shellcheck disable=SC1090
source "$COMMON_SCRIPT"

section "Syntax validation"

mapfile -d '' shell_files < <(
    find "$REPOSITORY_ROOT/scripts" "$REPOSITORY_ROOT/.githooks" \
        -type f ! -name 'README.md' \
        \( -name '*.sh' -o -perm -u+x \) -print0
)

if ((${#shell_files[@]} > 0)); then
    bash -n "${shell_files[@]}"
    success "Bash syntax is valid for ${#shell_files[@]} file(s)."
else
    warning "No shell files were found."
fi

python - "$REPOSITORY_ROOT" <<'PY'
import json
import sys
from pathlib import Path
from xml.etree import ElementTree

root = Path(sys.argv[1])
excluded = {"bin", "obj", "artifacts", ".git"}

json_files = sorted(
    path for path in root.rglob("*.json")
    if not excluded.intersection(path.parts)
)

xml_suffixes = {
    ".axaml", ".csproj", ".manifest", ".props",
    ".slnx", ".targets", ".xml",
}

xml_files = sorted(
    path for path in root.rglob("*")
    if path.is_file()
    and path.suffix.lower() in xml_suffixes
    and not excluded.intersection(path.parts)
)

for path in json_files:
    with path.open(encoding="utf-8-sig") as stream:
        json.load(stream)

for path in xml_files:
    ElementTree.parse(path)

print(f"[OK] JSON syntax is valid for {len(json_files)} file(s).")
print(f"[OK] XML syntax is valid for {len(xml_files)} file(s).")
PY

make --no-print-directory --dry-run help >/dev/null
success "Makefile syntax is valid."
