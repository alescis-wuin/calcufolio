# GitHub Actions and branch rulesets

## Branch promotion model

```text
feature/*, fix/*, chore/*, ...
              |
              v
           develop
              |
              v
           testing
              |
              v
             main
```

- Work branches open pull requests against `develop`.
- Only `develop` may open a pull request against `testing`.
- Only `testing` may open a pull request against `main`.
- The testing and main workflows enforce those promotion paths.

## Required checks

| Target branch | Required status check |
|---|---|
| `develop` | `Develop / Quality Gate` |
| `testing` | `Testing / Required` |
| `main` | `Main / Required` |

The aggregate checks on `testing` and `main` depend on every required validation job. This keeps ruleset configuration stable even if internal matrix jobs evolve.

## Initial activation order

1. Merge the GitHub Actions workflows into `develop`.
2. Promote `develop` to `testing` and then `testing` to `main` so every branch contains the workflows.
3. Let each workflow run successfully at least once.
4. Create and activate the three rulesets below.
5. Test each protected branch with a pull request.

Do not require a status check before GitHub has registered its context at least once.

## Shared security settings

Apply these settings to all three rulesets unless a branch-specific section says otherwise.

- Enforcement status: **Active**.
- Target: **Branch**.
- Block branch deletion.
- Block force pushes.
- Require a pull request before merging.
- Dismiss stale approvals when new commits are pushed.
- Require all review conversations to be resolved.
- Require signed commits.
- Require status checks to pass before merging.
- Require branches to be up to date before merging.
- Do not allow direct pushes.
- Do not add broad bypass permissions.

For a repository maintained by one person, use **0 required approvals**. Requiring one approval would prevent the repository owner from merging their own pull requests. Increase this value when another regular reviewer joins the project.

An emergency repository-admin bypass may be configured, but it should be used only to repair an unusable ruleset or broken required check.

## Ruleset: develop

### Target

Include default branch pattern:

```text
refs/heads/develop
```

### Rules

- Restrict deletions.
- Block force pushes.
- Require linear history.
- Require signed commits.
- Require pull request:
  - required approvals: `0`;
  - dismiss stale approvals: enabled;
  - require approval of the most recent push: disabled;
  - require code-owner review: disabled;
  - require conversation resolution: enabled;
  - allowed merge methods: **Squash** and **Rebase**.
- Require status check:

```text
Develop / Quality Gate
```

### Purpose

`develop` is the integration branch for short-lived work branches. A linear history keeps feature integration readable while the CI checks syntax, formatting, compilation, tests, shell lint and dependency vulnerabilities.

## Ruleset: testing

### Target

```text
refs/heads/testing
```

### Rules

- Restrict deletions.
- Block force pushes.
- Require signed commits.
- Require pull request:
  - required approvals: `0`;
  - dismiss stale approvals: enabled;
  - require approval of the most recent push: disabled;
  - require code-owner review: disabled;
  - require conversation resolution: enabled;
  - allowed merge method: **Merge commit**.
- Require status check:

```text
Testing / Required
```

### Purpose

`testing` receives complete promotions from `develop`. Merge commits preserve the exact promotion boundary and avoid rewriting the integration history. The workflow also verifies Release builds and tests on Linux, Windows and macOS.

## Ruleset: main

### Target

```text
refs/heads/main
```

### Rules

- Restrict deletions.
- Block force pushes.
- Require signed commits.
- Require pull request:
  - required approvals: `0`;
  - dismiss stale approvals: enabled;
  - require approval of the most recent push: disabled;
  - require code-owner review: disabled;
  - require conversation resolution: enabled;
  - allowed merge method: **Merge commit**.
- Require status check:

```text
Main / Required
```

### Purpose

`main` receives only validated promotions from `testing`. Its workflow repeats the Release validation, audits dependencies and verifies that the Avalonia desktop project can be published.

## Repository merge settings

In **Settings → General → Pull Requests**:

- Keep **Allow squash merging** enabled for work branches targeting `develop`.
- Keep **Allow rebase merging** enabled if desired for small, already-clean work branches.
- Keep **Allow merge commits** enabled for `develop → testing` and `testing → main` promotions.
- Enable **Automatically delete head branches**.
- Disable automatic merging unless explicitly needed.

The branch-specific rulesets determine which merge methods are accepted for each protected branch.

## Recommended pull-request flow

### Work to develop

```text
feature/calculator-keyboard -> develop
```

Use squash merge unless retaining the complete commit series is useful.

### Develop to testing

```text
develop -> testing
```

Use a merge commit after `Testing / Required` succeeds.

### Testing to main

```text
testing -> main
```

Use a merge commit after `Main / Required` succeeds.

## Checks intentionally excluded from hosted CI

The local `make verify` target also validates branch naming, work-branch signatures and branch-local linear history. Those checks depend on a developer worktree and are therefore not run unchanged on protected-branch GitHub runners.

Equivalent repository-level guarantees are provided by:

- GitHub branch rulesets;
- required signed commits;
- required pull requests;
- restricted promotion paths;
- required status checks;
- blocked force pushes and deletions.
