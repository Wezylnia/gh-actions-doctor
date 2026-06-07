# Project Status

This public status document tracks user-facing progress from the first preview release through the current stable release.

## Current Version

Current release: `1.0.0`

Goal: first stable release. All documented CLI, JSON, SARIF, configuration, baseline, suppression, and safe-fix behavior is locked down.

## Completed In v1.0.0

- Stabilize documented CLI, JSON, SARIF, configuration, baseline, suppression, and safe-fix behavior for public CI usage.
- Refresh all documentation, project status, roadmap, package metadata, and install examples.

## Completed In v0.9.0

- Add `release-workflow-overprivileged-token`, `remote-script-execution`, and `secret-echo-risk` security rules.
- Add SARIF `partialFingerprints`.
- Add machine-readable JSON schemas.

## Completed In v0.8.0

- Split fix command into dedicated `FixOptionsParser`.
- Make `WorkflowFixer` rule-scoped and parse-aware (skip invalid/complex YAML).
- Add stable baseline fingerprints and `--prune-baseline`.
- Add `--show-suppressions`.

## Completed In v0.7.0

- Add baseline suppression, generation, and inline suppression comments.
- Add adoption guide.

## Completed In v0.6.0

- Add `overbroad-id-token-permission`, `pull-request-target-untrusted-checkout`, and `untrusted-expression-in-run` security rules.

- .NET 10 target framework.
- Package metadata for the CLI .NET tool.
- Text and JSON output.
- Ten built-in workflow hygiene rules.
- Include/exclude filtering.
- Strict mode.
- `--fail-on` exit-code behavior.
- Source locations for many findings.
- CI build and test workflow.
- README install and usage documentation.
- Workflow hardening checklist.
- Public release files and initial rule docs.

## Completed In v0.2.0

- Add `.gh-actions-doctor.yml` configuration support.
- Add rule disabling and severity overrides.
- Add path, format, fail threshold, strict mode, include, exclude, disabled rule, and severity override config fields.
- Add config parse validation and CLI error output.
- Document config examples.
- Add config behavior tests.

## Completed In v0.3.0

- Add GitHub annotation output.
- Add official composite GitHub Action wrapper.
- Add CI-focused usage and smoke coverage.

## Completed In v0.4.0

- Add SARIF 2.1.0 output.
- Add SARIF rule metadata and source locations.
- Add Code Scanning-ready output behavior.

## Completed In v0.5.0

- Add conservative `fix` command.
- Add dry-run and apply modes.
- Add safe fix for missing top-level `permissions: contents: read`.
- Add safe fix for missing job `timeout-minutes: 30`.

## Completed In v0.6.0

- Add `overbroad-id-token-permission`.
- Add `pull-request-target-untrusted-checkout`.
- Add `untrusted-expression-in-run`.
- Add focused tests and rule docs for advanced security findings.

## Completed In v0.7.0

- Add baseline suppression via `--baseline <path>` and config `baseline`.
- Add baseline generation via `--write-baseline <path>`.
- Add inline suppression comments.
- Refresh release documentation and validation coverage.

## Repository Protection

The repository is configured so public contribution should flow through pull requests:

- `main` is protected.
- The `build` GitHub Actions check is required before merge.
- Pull requests require at least one approving review.
- Code-owner review is required.
- `CODEOWNERS` assigns all files to `@Wezylnia`.
- Stale approvals are dismissed after new commits are pushed.
- Conversation resolution is required before merge.
- Force pushes and branch deletion are disabled.
- Administrators keep emergency bypass ability.
- GitHub Copilot is configured for automatic pull request review on pushes to PRs targeting `main`.

## Validation Commands

```bash
dotnet restore
dotnet build GhActionsDoctor.sln --configuration Release
dotnet test GhActionsDoctor.sln --configuration Release --no-build
dotnet pack src/GhActionsDoctor.Cli --configuration Release --no-build
dotnet run --project src/GhActionsDoctor.Cli -- scan --path samples/bad --fail-on none
dotnet run --project src/GhActionsDoctor.Cli -- scan --path samples/bad --format json --fail-on none
dotnet run --project src/GhActionsDoctor.Cli -- scan --path samples/bad --format github-annotations --fail-on none
dotnet run --project src/GhActionsDoctor.Cli -- scan --path samples/bad --format sarif --fail-on none
dotnet run --project src/GhActionsDoctor.Cli -- fix --path samples/bad --dry-run
```
