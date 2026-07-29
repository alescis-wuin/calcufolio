#!/usr/bin/env bash
set -Eeuo pipefail

SCRIPT_DIRECTORY="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
readonly SCRIPT_DIRECTORY
REPOSITORY_ROOT="$(
    cd -- "$SCRIPT_DIRECTORY/../.." &&
        pwd
)"
readonly REPOSITORY_ROOT
readonly COMMON_SCRIPT="$REPOSITORY_ROOT/scripts/lib/common.sh"
readonly SOLUTION="$REPOSITORY_ROOT/Calcufolio.slnx"

# shellcheck disable=SC1090
source "$COMMON_SCRIPT"

section "Dependency vulnerability audit"

dotnet restore "$SOLUTION" \
    -p:NuGetAudit=true \
    -p:NuGetAuditMode=all

mapfile -t project_files < <(
    dotnet sln "$SOLUTION" list | awk '/\.csproj$/ { print }'
)

((${#project_files[@]} > 0)) ||
    die "No projects were found in the solution."

for project_file in "${project_files[@]}"; do
    info "Auditing project: $project_file"

    dotnet package list \
        --project "$REPOSITORY_ROOT/$project_file" \
        --include-transitive \
        --vulnerable \
        --no-restore
done

success "Dependency vulnerability audit completed."
