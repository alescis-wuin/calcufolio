# Repository Git hooks

This directory contains the Git hooks shared by the repository.

Install or refresh the hooks configuration with:

```bash
make hooks-install
```

Verify the worktree-local hooks configuration with:

```bash
make hooks-check
```

The installer enables Git worktree-specific configuration and stores the
absolute hooks path in the current worktree configuration. This avoids
changing the hooks path of other linked worktrees.

The hooks remain secondary to GitHub rulesets and continuous integration
checks because local hooks can be bypassed with `--no-verify`.
