# Project Status

This public status document tracks user-facing progress from the current MVP toward the next release.

Private planning files such as `project.md`, `plan.md`, and `docs/development-notes.md` are intentionally local-only and ignored by Git.

## Current Version Target

Current preview: `0.1.0-preview.1`

Goal: make the current tool installable, documented, and safe to share publicly.

## Completed For Preview

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
- Public release files:
  - `CHANGELOG.md`
  - `CONTRIBUTING.md`
  - `CODE_OF_CONDUCT.md`
  - `SECURITY.md`
- First rule docs:
  - `missing-permissions`
  - `missing-timeout`
  - `mutable-action-reference`
  - `action-not-sha-pinned`
  - `risky-pull-request-target`
- CI is green for the preview preparation commit.
- README CI badge has been added.
- GitHub release `v0.1.0-preview.1` is prepared from the current release notes.

## Remaining For Preview

No required repository tasks remain for `v0.1.0-preview.1`.

NuGet publishing is intentionally separate and can be done after the GitHub preview release is reviewed.

## Next After Preview

The next milestone is `v0.1.0`, focused on:

- docs for all 10 current rules
- parser-specific tests
- reporter-specific tests
- CLI end-to-end tests
- remaining source-location polish
- first polished MVP release

## Validation Commands

```bash
dotnet build GhActionsDoctor.sln --configuration Release
dotnet test GhActionsDoctor.sln --configuration Release --no-build
dotnet pack src/GhActionsDoctor.Cli --configuration Release --no-build
dotnet tool install --tool-path .tmp/tools gh-actions-doctor --version 0.1.0-preview.1 --add-source src/GhActionsDoctor.Cli/bin/Release
dotnet run --project src/GhActionsDoctor.Cli -- scan --path samples/bad --fail-on none
```
