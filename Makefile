SHELL := /usr/bin/bash
.SHELLFLAGS := -Eeuo pipefail -c
.DEFAULT_GOAL := help
.ONESHELL:

ROOT := $(abspath $(dir $(lastword $(MAKEFILE_LIST))))
SOLUTION := $(ROOT)/Calcufolio.slnx
PRESENTATION_PROJECT := $(ROOT)/src/Calcufolio.Presentation/Calcufolio.Presentation.csproj
COMMON_SCRIPT := $(ROOT)/scripts/lib/common.sh
HOOK_INSTALLER := $(ROOT)/scripts/hooks/install.sh
CHECKS_DIRECTORY := $(ROOT)/scripts/checks
PATCH_RUNNER := $(ROOT)/scripts/patch/apply-package.sh
PATCH_TOOL := $(ROOT)/scripts/patch/patch_tool.py

CONFIGURATION ?= Debug
BASE_REF ?= origin/develop
PATCH ?=
PATCH_DOWNLOADS_DIR ?= $(HOME)/Téléchargements
PATCH_DIR ?=
PATCH_OUTPUT ?=

.PHONY: \
	help doctor hooks-install hooks-check \
	clean restore build rebuild run test status \
	git-check branch-check staged syntax format format-check lint audit \
	signatures linear-history verify-fast verify \
	patch patch-validate patch-pack patch-self-test

help: ## Show the available commands
	@printf '\nAvailable commands:\n\n'
	@awk 'BEGIN { FS = ":.*## " } /^[a-zA-Z0-9_.-]+:.*## / { printf "  %-20s %s\n", $$1, $$2 }' $(MAKEFILE_LIST)
	@printf '\nOptional variables:\n\n'
	@printf '  CONFIGURATION=Debug|Release\n'
	@printf '  BASE_REF=origin/develop\n'
	@printf '  PATCH=<archive-name-without-.zip>\n'
	@printf '  PATCH_DOWNLOADS_DIR=$$HOME/Téléchargements\n'
	@printf '  PATCH_DIR=/path/to/unpacked/package\n'
	@printf '  PATCH_OUTPUT=/path/to/package.zip\n\n'

doctor: ## Audit the local development environment
	@source "$(COMMON_SCRIPT)"
	section "Development environment"
	require_command bash
	require_command git
	require_command gh
	require_command make
	require_command dotnet
	require_file "$(SOLUTION)"
	require_file "$(PRESENTATION_PROJECT)"
	info "Repository: $$(git rev-parse --show-toplevel)"
	info "Branch: $$(git branch --show-current)"
	info ".NET SDK: $$(dotnet --version)"
	info "Git version: $$(git --version)"
	info "GitHub CLI: $$(gh --version | sed -n '1p')"
	info "Commit signing: $$(git config --get commit.gpgsign || printf 'not configured')"
	info "Signing key: $$(git config --get user.signingkey || printf 'not configured')"
	info "Hooks path: $$(git config --get core.hooksPath || printf 'not configured')"
	success "Development environment audit completed."

hooks-install: ## Install the repository Git hooks for this worktree
	@"$(HOOK_INSTALLER)" install

hooks-check: ## Verify the repository Git hooks for this worktree
	@"$(HOOK_INSTALLER)" check

clean: ## Remove .NET build outputs
	@source "$(COMMON_SCRIPT)"
	section "Cleaning solution"
	dotnet clean "$(SOLUTION)" --configuration "$(CONFIGURATION)"
	success "Solution cleaned."

restore: ## Restore NuGet dependencies
	@source "$(COMMON_SCRIPT)"
	section "Restoring dependencies"
	dotnet restore "$(SOLUTION)"
	success "Dependencies restored."

build: restore ## Build the complete solution
	@source "$(COMMON_SCRIPT)"
	section "Building solution"
	dotnet build "$(SOLUTION)" \
		--configuration "$(CONFIGURATION)" \
		--no-restore
	success "Solution built."

rebuild: clean build ## Clean and rebuild the complete solution

run: build ## Build and run the Avalonia application
	@source "$(COMMON_SCRIPT)"
	section "Running application"
	dotnet run \
		--project "$(PRESENTATION_PROJECT)" \
		--configuration "$(CONFIGURATION)" \
		--no-build

test: build ## Build and run all unit tests
	@source "$(COMMON_SCRIPT)"
	section "Running tests"
	if grep -Eq \
		'"runner"[[:space:]]*:[[:space:]]*"Microsoft.Testing.Platform"' \
		"$(ROOT)/global.json"; then
		info "Test runner: Microsoft Testing Platform"
		dotnet test \
			--solution "$(SOLUTION)" \
			--configuration "$(CONFIGURATION)" \
			--no-build \
			--no-restore
	else
		info "Test runner: VSTest compatibility mode"
		dotnet test "$(SOLUTION)" \
			--configuration "$(CONFIGURATION)" \
			--no-build \
			--no-restore
	fi
	success "Tests completed."

status: ## Display the concise Git status
	@git status --short

git-check: ## Inspect staged changes and repository status
	@source "$(COMMON_SCRIPT)"
	section "Staged diff validation"
	GIT_PAGER=cat git diff --cached --check
	GIT_PAGER=cat git diff --cached --stat
	GIT_PAGER=cat git diff --cached
	git status --short
	success "Git inspection completed."

branch-check: ## Enforce work branch naming and protection rules
	@"$(CHECKS_DIRECTORY)/branch-policy.sh"

staged: ## Validate staged file safety
	@"$(CHECKS_DIRECTORY)/staged-files.sh"

syntax: ## Validate Bash, Python, JSON, XML, and Makefile syntax
	@"$(CHECKS_DIRECTORY)/syntax.sh"

format: restore ## Apply .NET formatting and analyzer fixes
	@source "$(COMMON_SCRIPT)"
	section "Applying .NET formatting"
	dotnet format "$(SOLUTION)" --no-restore
	success "Formatting completed."

format-check: restore ## Verify .NET formatting without modifying files
	@source "$(COMMON_SCRIPT)"
	section "Verifying .NET formatting"
	dotnet format "$(SOLUTION)" --verify-no-changes --no-restore
	success "Formatting verification completed."

lint: ## Run ShellCheck on repository shell files
	@"$(CHECKS_DIRECTORY)/lint.sh"

audit: ## Audit direct and transitive NuGet vulnerabilities
	@"$(CHECKS_DIRECTORY)/vulnerabilities.sh"

signatures: ## Verify signatures of branch commits
	@BASE_REF="$(BASE_REF)" "$(CHECKS_DIRECTORY)/signatures.sh"

linear-history: ## Reject merge commits introduced by the work branch
	@BASE_REF="$(BASE_REF)" "$(CHECKS_DIRECTORY)/linear-history.sh"

patch: ## Safely apply, validate, test, stage, and commit a patch package
	@runtime_runner="$(PATCH_RUNNER).runtime.$$$$.sh"
	trap 'rm -f -- "$$runtime_runner"' EXIT
	cp -- "$(PATCH_RUNNER)" "$$runtime_runner"
	chmod 700 "$$runtime_runner"
	PATCH="$(PATCH)" \
		PATCH_DOWNLOADS_DIR="$(PATCH_DOWNLOADS_DIR)" \
		"$$runtime_runner"

patch-validate: ## Validate an unpacked patch package directory
	@test -n "$(PATCH_DIR)" || { printf 'PATCH_DIR is required.\n' >&2; exit 2; }
	@python3 "$(PATCH_TOOL)" validate-dir --package-dir "$(PATCH_DIR)"

patch-pack: ## Generate checksums and create a patch ZIP archive
	@test -n "$(PATCH_DIR)" || { printf 'PATCH_DIR is required.\n' >&2; exit 2; }
	@if [[ -n "$(PATCH_OUTPUT)" ]]; then
		python3 "$(PATCH_TOOL)" pack \
			--package-dir "$(PATCH_DIR)" \
			--output "$(PATCH_OUTPUT)"
	else
		python3 "$(PATCH_TOOL)" pack \
			--package-dir "$(PATCH_DIR)"
	fi

patch-self-test: ## Run patch workflow unit tests
	@PYTHONDONTWRITEBYTECODE=1 python3 -m unittest discover \
		-s "$(ROOT)/scripts/patch/tests" \
		-p 'test_*.py' \
		-v

verify-fast: ## Run fast checks suitable before a commit
	@$(MAKE) --no-print-directory \
		branch-check staged syntax format-check

verify: ## Run the complete local quality gate
	@$(MAKE) --no-print-directory \
		branch-check clean restore build test \
		syntax lint format-check audit signatures linear-history
