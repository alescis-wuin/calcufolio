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
DOTNET := $(ROOT)/scripts/toolchain/dotnet.sh
TOOLCHAIN_BOOTSTRAP := $(ROOT)/scripts/toolchain/bootstrap-dotnet.sh
TOOLCHAIN_RESOLVER := $(ROOT)/scripts/toolchain/resolve-dotnet.sh
TOOLCHAIN_VERIFY := $(ROOT)/scripts/toolchain/verify-dotnet.sh
TOOLCHAIN_TESTS := $(ROOT)/scripts/toolchain/tests/run.sh
TOOLCHAIN_INTEGRATION_TESTS := $(ROOT)/scripts/toolchain/tests/integration.sh

CONFIGURATION ?= Debug
BASE_REF ?= origin/develop
PATCH ?=
PATCH_DOWNLOADS_DIR ?= $(HOME)/Téléchargements
PATCH_DIR ?=
PATCH_OUTPUT ?=
TOOLCHAIN_ARCHIVE ?=
TOOLCHAIN_SHA256 ?=
TOOLCHAIN_OFFLINE_ONLY ?= 0
TOOLCHAIN_FORCE ?= 0

.PHONY: \
	help doctor toolchain-bootstrap toolchain-check toolchain-info \
	toolchain-clean toolchain-self-test hooks-install hooks-check \
	clean restore build rebuild run test status \
	git-check branch-check worktree-clean staged syntax format format-check lint audit \
	signatures linear-history verify-fast verify verify-push \
	patch patch-validate patch-pack patch-self-test worktree-clean-self-test

help: ## Show the available commands
	@printf '\nAvailable commands:\n\n'
	@awk 'BEGIN { FS = ":.*## " } /^[a-zA-Z0-9_.-]+:.*## / { printf "  %-20s %s\n", $$1, $$2 }' $(MAKEFILE_LIST)
	@printf '\nOptional variables:\n\n'
	@printf '  CONFIGURATION=Debug|Release\n'
	@printf '  BASE_REF=origin/develop\n'
	@printf '  PATCH=<archive-name-without-.zip>\n'
	@printf '  PATCH_DOWNLOADS_DIR=$$HOME/Téléchargements\n'
	@printf '  PATCH_DIR=/path/to/unpacked/package\n'
	@printf '  PATCH_OUTPUT=/path/to/package.zip\n'
	@printf '  TOOLCHAIN_ARCHIVE=/path/to/dotnet-sdk.tar.gz\n'
	@printf '  TOOLCHAIN_SHA256=<64-character-sha256>\n'
	@printf '  TOOLCHAIN_OFFLINE_ONLY=0|1\n'
	@printf '  TOOLCHAIN_FORCE=0|1\n\n'

doctor: ## Audit the local development environment
	@source "$(COMMON_SCRIPT)"
	section "Development environment"
	require_command bash
	require_command git
	require_command gh
	require_command make
	require_file "$(DOTNET)"
	require_file "$(TOOLCHAIN_VERIFY)"
	"$(TOOLCHAIN_VERIFY)"
	require_file "$(SOLUTION)"
	require_file "$(PRESENTATION_PROJECT)"
	info "Repository: $$(git rev-parse --show-toplevel)"
	info "Branch: $$(git branch --show-current)"
	info ".NET SDK: $$("$(DOTNET)" --version)"
	info ".NET source: $$("$(TOOLCHAIN_RESOLVER)" --source)"
	info "Git version: $$(git --version)"
	info "GitHub CLI: $$(gh --version | sed -n '1p')"
	info "Commit signing: $$(git config --get commit.gpgsign || printf 'not configured')"
	info "Signing key: $$(git config --get user.signingkey || printf 'not configured')"
	info "Hooks path: $$(git config --get core.hooksPath || printf 'not configured')"
	success "Development environment audit completed."

toolchain-bootstrap: ## Install the pinned SDK into .dotnet when required
	@arguments=()
	if [[ -n "$(TOOLCHAIN_ARCHIVE)" ]]; then
		arguments+=(--archive "$(TOOLCHAIN_ARCHIVE)")
	fi
	if [[ -n "$(TOOLCHAIN_SHA256)" ]]; then
		arguments+=(--sha256 "$(TOOLCHAIN_SHA256)")
	fi
	if [[ "$(TOOLCHAIN_OFFLINE_ONLY)" == '1' ]]; then
		arguments+=(--offline-only)
	fi
	if [[ "$(TOOLCHAIN_FORCE)" == '1' ]]; then
		arguments+=(--force)
	fi
	"$(TOOLCHAIN_BOOTSTRAP)" "$${arguments[@]}"

toolchain-check: ## Verify the SDK selected for this repository
	@"$(TOOLCHAIN_VERIFY)"

toolchain-info: ## Display SDK resolution and complete host information
	@source "$(COMMON_SCRIPT)"
	section "Repository .NET toolchain"
	"$(TOOLCHAIN_RESOLVER)" --json
	"$(DOTNET)" --info

toolchain-clean: ## Remove only the repository-local SDK installation
	@source "$(COMMON_SCRIPT)"
	section "Cleaning repository-local .NET SDK"
	local_install="$(ROOT)/.dotnet"
	bootstrap_lock="$(ROOT)/.dotnet.bootstrap.lock"
	if [[ ! -e "$$local_install" && ! -e "$$bootstrap_lock" ]]; then
		info "No repository-local SDK installation was found."
		exit 0
	fi
	rm -rf -- "$$local_install" "$$bootstrap_lock"
	success "Repository-local SDK installation removed."

toolchain-self-test: ## Test SDK resolution, bootstrap, and project integration
	@"$(TOOLCHAIN_TESTS)"
	@"$(TOOLCHAIN_INTEGRATION_TESTS)"

hooks-install: ## Install the repository Git hooks for this worktree
	@"$(HOOK_INSTALLER)" install

hooks-check: ## Verify the repository Git hooks for this worktree
	@"$(HOOK_INSTALLER)" check

clean: toolchain-check ## Remove .NET build outputs
	@source "$(COMMON_SCRIPT)"
	section "Cleaning solution"
	"$(DOTNET)" clean "$(SOLUTION)" --configuration "$(CONFIGURATION)"
	success "Solution cleaned."

restore: toolchain-check ## Restore NuGet dependencies
	@source "$(COMMON_SCRIPT)"
	section "Restoring dependencies"
	"$(DOTNET)" restore "$(SOLUTION)"
	success "Dependencies restored."

build: restore ## Build the complete solution
	@source "$(COMMON_SCRIPT)"
	section "Building solution"
	"$(DOTNET)" build "$(SOLUTION)" \
		--configuration "$(CONFIGURATION)" \
		--no-restore
	success "Solution built."

rebuild: clean build ## Clean and rebuild the complete solution

run: build ## Build and run the Avalonia application
	@source "$(COMMON_SCRIPT)"
	section "Running application"
	"$(DOTNET)" run \
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
		"$(DOTNET)" test \
			--solution "$(SOLUTION)" \
			--configuration "$(CONFIGURATION)" \
			--no-build \
			--no-restore
	else
		info "Test runner: VSTest compatibility mode"
		"$(DOTNET)" test "$(SOLUTION)" \
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

worktree-clean: ## Reject pending staged, unstaged, or untracked changes
	@"$(CHECKS_DIRECTORY)/worktree-clean.sh"

staged: ## Validate staged file safety
	@"$(CHECKS_DIRECTORY)/staged-files.sh"

syntax: ## Validate Bash, Python, JSON, XML, and Makefile syntax
	@"$(CHECKS_DIRECTORY)/syntax.sh"

format: restore ## Apply .NET formatting and analyzer fixes
	@source "$(COMMON_SCRIPT)"
	section "Applying .NET formatting"
	"$(DOTNET)" format "$(SOLUTION)" --no-restore
	success "Formatting completed."

format-check: restore ## Verify .NET formatting without modifying files
	@source "$(COMMON_SCRIPT)"
	section "Verifying .NET formatting"
	"$(DOTNET)" format "$(SOLUTION)" --verify-no-changes --no-restore
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

worktree-clean-self-test: ## Test clean and dirty worktree detection
	@"$(CHECKS_DIRECTORY)/tests/worktree-clean-test.sh"

verify-fast: ## Run fast checks suitable before a commit
	@$(MAKE) --no-print-directory \
		branch-check staged syntax toolchain-check format-check

verify: ## Run the complete local quality gate
	@$(MAKE) --no-print-directory \
		branch-check toolchain-check clean restore build test \
		syntax lint format-check audit toolchain-self-test \
		worktree-clean-self-test \
		signatures linear-history

verify-push: ## Require a clean repository before the complete quality gate
	@$(MAKE) --no-print-directory worktree-clean
	@$(MAKE) --no-print-directory verify
