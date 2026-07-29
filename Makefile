SHELL := /usr/bin/bash
.SHELLFLAGS := -Eeuo pipefail -c
.DEFAULT_GOAL := help
.ONESHELL:

ROOT := $(abspath $(dir $(lastword $(MAKEFILE_LIST))))
SOLUTION := $(ROOT)/Calcufolio.slnx
PRESENTATION_PROJECT := $(ROOT)/src/Calcufolio.Presentation/Calcufolio.Presentation.csproj
COMMON_SCRIPT := $(ROOT)/scripts/lib/common.sh
HOOK_INSTALLER := $(ROOT)/scripts/hooks/install.sh

CONFIGURATION ?= Debug

.PHONY: \
	help \
	doctor \
	hooks-install \
	hooks-check \
	clean \
	restore \
	build \
	rebuild \
	run \
	test \
	status

help: ## Show the available commands
	@printf '\nAvailable commands:\n\n'
	@awk 'BEGIN { FS = ":.*## " } /^[a-zA-Z0-9_.-]+:.*## / { printf "  %-20s %s\n", $$1, $$2 }' $(MAKEFILE_LIST)
	@printf '\nOptional variables:\n\n'
	@printf '  CONFIGURATION=Debug|Release\n\n'

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
	dotnet clean "$(SOLUTION)" \
		--configuration "$(CONFIGURATION)"
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
