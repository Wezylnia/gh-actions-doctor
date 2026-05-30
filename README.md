# gh-actions-doctor

`gh-actions-doctor` is a lightweight .NET CLI that scans GitHub Actions workflow files and reports common security, reliability, performance, maintainability, and cost issues.

It is built for maintainers who want quick CI/CD hygiene checks without adopting a full security platform.

## Why It Exists

GitHub Actions workflows are often copied from project to project and then left alone for months. They may still work, but small problems pile up:

- overly broad `GITHUB_TOKEN` permissions
- actions referenced by mutable branches like `@main`
- third-party actions not pinned to commit SHAs
- risky `pull_request_target` usage
- jobs without timeouts
- workflows without concurrency controls
- dependency installs without caching
- broad triggers that burn CI minutes unnecessarily

`gh-actions-doctor` catches those issues early and prints practical suggestions.

## Status

This repository contains the first working MVP. It can scan workflow files, report findings in text or JSON, and return CI-friendly exit codes.

## Requirements

- .NET SDK 10

This project currently targets `net10.0`.

## Quick Start

Scan the default workflow directory:

```bash
dotnet run --project src/GhActionsDoctor.Cli -- scan
```

Scan a custom directory:

```bash
dotnet run --project src/GhActionsDoctor.Cli -- scan --path samples/bad
```

Print JSON:

```bash
dotnet run --project src/GhActionsDoctor.Cli -- scan --path samples/bad --format json
```

Fail a build on warnings or errors:

```bash
dotnet run --project src/GhActionsDoctor.Cli -- scan --fail-on warning
```

Run only selected rules:

```bash
dotnet run --project src/GhActionsDoctor.Cli -- scan --include missing-permissions,missing-timeout
```

Skip selected rules:

```bash
dotnet run --project src/GhActionsDoctor.Cli -- scan --exclude action-not-sha-pinned
```

## Example Output

```txt
GitHub Actions Doctor

Scanned workflows: 2

build.yml
  [warning] Workflow does not define a top-level permissions block. (missing-permissions)
    Suggestion: Add a top-level permissions block and grant only the permissions required by the workflow.
  [warning] Job 'build' does not define timeout-minutes. (missing-timeout)
    Suggestion: Add a timeout-minutes value that reflects the expected maximum runtime for this job.
  [info] actions/setup-node is used without dependency caching. (setup-node-cache-missing)
    Suggestion: Add a cache value such as npm, yarn, or pnpm when the workflow installs Node.js dependencies.

Summary:
  0 errors
  2 warnings
  1 info
```

## Rules

| Rule | Severity | Category | Description |
| --- | --- | --- | --- |
| `missing-permissions` | warning | security | Reports workflows without a top-level `permissions` block. |
| `mutable-action-reference` | warning | security | Reports actions using mutable refs such as `@main`, `@master`, or `@latest`. |
| `action-not-sha-pinned` | info | security | Reports third-party actions that are not pinned to a full commit SHA. |
| `risky-pull-request-target` | warning/error | security | Reports `pull_request_target`, with errors for high-risk patterns. |
| `missing-timeout` | warning | reliability | Reports jobs without `timeout-minutes`. |
| `missing-concurrency` | info | cost | Reports workflows that are likely to benefit from `concurrency`. |
| `setup-node-cache-missing` | info | performance | Reports `actions/setup-node` usage without dependency caching. |
| `broad-push-trigger` | info | cost | Reports workflows that run on every push without branch, tag, or path filters. |
| `duplicate-workflow-name` | info | maintainability | Reports repeated workflow names across files. |
| `yaml-parse-error` | error | correctness | Reports invalid workflow YAML without crashing the scan. |

## CLI Reference

```txt
gh-actions-doctor scan [options]

Options:
  --path <path>                 Workflow directory or file. Defaults to ./.github/workflows.
  --format <text|json>          Output format. Defaults to text.
  --fail-on <error|warning|info|none>
                                Controls non-zero exit code. Defaults to error.
  --include <rule-id,...>       Run only selected rules.
  --exclude <rule-id,...>       Skip selected rules.
  --strict                      Promote selected security findings.
```

## Development

Restore, build, and test:

```bash
dotnet restore
dotnet build GhActionsDoctor.sln
dotnet test GhActionsDoctor.sln
```

Try the sample workflows:

```bash
dotnet run --project src/GhActionsDoctor.Cli -- scan --path samples/good --fail-on none
dotnet run --project src/GhActionsDoctor.Cli -- scan --path samples/bad --fail-on none
```

Pack as a local .NET tool:

```bash
dotnet pack src/GhActionsDoctor.Cli --configuration Release
dotnet tool install --global --add-source src/GhActionsDoctor.Cli/bin/Release GhActionsDoctor.Cli
gh-actions-doctor scan
```

## Roadmap

- SARIF output for GitHub code scanning
- configuration file support
- GitHub Actions annotation output
- safe autofix support
- official GitHub Action wrapper

## License

MIT
