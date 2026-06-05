# Project Status

This public status document tracks user-facing progress from the current MVP toward the next release.

Private planning files such as `project.md`, `plan.md`, and `docs/development-notes.md` are intentionally local-only and ignored by Git.

## Current Version

Current release: `0.1.0`

Goal: keep the polished MVP stable while preparing configuration support for `v0.2.0`.

## Completed In v0.1.0

- .NET 10 target framework.
- Package metadata for the CLI .NET tool.
- Local installation verified from the generated `.nupkg`.
- Text and JSON output.
- Ten built-in rules.
- Include/exclude filtering.
- Strict mode.
- `--fail-on` exit-code behavior.
- Source locations for many findings.
- CI build and test workflow.
- CI self-scan step.
- README install and usage documentation.
- Workflow hardening checklist.
- Public release files:
  - `CHANGELOG.md`
  - `CONTRIBUTING.md`
  - `CODE_OF_CONDUCT.md`
  - `SECURITY.md`
- Initial rule docs:
  - `missing-permissions`
  - `missing-timeout`
  - `mutable-action-reference`
  - `action-not-sha-pinned`
  - `risky-pull-request-target`
- Rule docs for all current rules.
- Parser and reporter test coverage.
- CI is green for the release preparation commit.
- README CI badge has been added.
- GitHub release `v0.1.0-preview.1` is published.
- The generated `.nupkg` is attached to the GitHub release.
- GitHub release `v0.1.0` is prepared from the polished MVP state.

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
- Administrators are not locked out, so the maintainer keeps emergency bypass ability.
- Squash merge is the intended merge strategy.
- The most recent push must be approved by someone other than the pusher.
- GitHub Copilot is configured for automatic pull request review on pushes to PRs targeting `main`.

## Remaining Before v0.2.0

- Add `.gh-actions-doctor.yml` configuration support.
- Add rule disabling and severity overrides.
- Add path and rule ignores.
- Add config parse validation and CLI error output.
- Document config examples.

NuGet publishing is intentionally separate and can be done after the GitHub release is reviewed.

## Next Release

The next milestone is `v0.2.0`, focused on:

- repository-local configuration
- rule and path ignores
- severity overrides
- config precedence with CLI flags
- config-focused tests and documentation

## Validation Commands

```bash
dotnet build GhActionsDoctor.sln --configuration Release
dotnet test GhActionsDoctor.sln --configuration Release --no-build
dotnet pack src/GhActionsDoctor.Cli --configuration Release --no-build
dotnet tool install --tool-path .tmp/tools gh-actions-doctor --version 0.1.0 --add-source src/GhActionsDoctor.Cli/bin/Release
dotnet run --project src/GhActionsDoctor.Cli -- scan --path samples/bad --fail-on none
```
