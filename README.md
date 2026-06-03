# gh-actions-doctor

[![CI](https://github.com/Wezylnia/gh-actions-doctor/actions/workflows/ci.yml/badge.svg)](https://github.com/Wezylnia/gh-actions-doctor/actions/workflows/ci.yml)
[![GitHub release](https://img.shields.io/github/v/release/Wezylnia/gh-actions-doctor?include_prereleases)](https://github.com/Wezylnia/gh-actions-doctor/releases)
[![License](https://img.shields.io/github/license/Wezylnia/gh-actions-doctor)](LICENSE)

A lightweight .NET CLI that scans GitHub Actions workflows for security, reliability, performance, and cost issues.

## Try It In 30 Seconds

```bash
git clone https://github.com/Wezylnia/gh-actions-doctor.git
cd gh-actions-doctor
dotnet run --project src/GhActionsDoctor.Cli -- scan --path samples/bad --fail-on none
```

## Why It Exists

GitHub Actions workflows are often copied from project to project and then left alone for months. They may still work, but small problems pile up:

- overly broad `GITHUB_TOKEN` permissions,
- actions referenced by mutable branches like `@main`,
- third-party actions not pinned to commit SHAs,
- risky `pull_request_target` usage,
- jobs without timeouts,
- workflows without concurrency controls,
- dependency installs without caching,
- broad triggers that burn CI minutes unnecessarily.

`gh-actions-doctor` catches those issues early and prints practical suggestions.

## Status

This repository contains the first working preview. It can scan workflow files, report findings in text or JSON, and return CI-friendly exit codes.

Looking for a place to help? Start here:

- [Good first issues](https://github.com/Wezylnia/gh-actions-doctor/issues?q=is%3Aissue%20is%3Aopen%20label%3A%22good%20first%20issue%22)
- [Help wanted](https://github.com/Wezylnia/gh-actions-doctor/issues?q=is%3Aissue%20is%3Aopen%20label%3A%22help%20wanted%22)
- [Rule requests](https://github.com/Wezylnia/gh-actions-doctor/issues?q=is%3Aissue%20is%3Aopen%20label%3Aarea%3Arules)

## Requirements

- .NET SDK 10

This project currently targets `net10.0`.

## Install

The first public package is planned as `0.1.0-preview.1`.

When published to NuGet:

```bash
dotnet tool install --global gh-actions-doctor --version 0.1.0-preview.1
```

From a locally packed package:

```bash
dotnet pack src/GhActionsDoctor.Cli --configuration Release
dotnet tool install --tool-path .tmp/tools gh-actions-doctor --version 0.1.0-preview.1 --add-source src/GhActionsDoctor.Cli/bin/Release
.tmp/tools/gh-actions-doctor scan
```

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

Strict mode promotes selected security findings:

```bash
dotnet run --project src/GhActionsDoctor.Cli -- scan --strict
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
| [`missing-permissions`](docs/rules/missing-permissions.md) | warning | security | Reports workflows without a top-level `permissions` block. |
| [`mutable-action-reference`](docs/rules/mutable-action-reference.md) | warning | security | Reports actions using mutable refs such as `@main`, `@master`, or `@latest`. |
| [`action-not-sha-pinned`](docs/rules/action-not-sha-pinned.md) | info | security | Reports third-party actions that are not pinned to a full commit SHA. |
| [`risky-pull-request-target`](docs/rules/risky-pull-request-target.md) | warning/error | security | Reports `pull_request_target`, with errors for high-risk patterns. |
| [`missing-timeout`](docs/rules/missing-timeout.md) | warning | reliability | Reports jobs without `timeout-minutes`. |
| [`missing-concurrency`](docs/rules/missing-concurrency.md) | info | cost | Reports workflows that are likely to benefit from `concurrency`. |
| [`setup-node-cache-missing`](docs/rules/setup-node-cache-missing.md) | info | performance | Reports `actions/setup-node` usage without dependency caching. |
| [`broad-push-trigger`](docs/rules/broad-push-trigger.md) | info | cost | Reports workflows that run on every push without branch, tag, or path filters. |
| [`duplicate-workflow-name`](docs/rules/duplicate-workflow-name.md) | info | maintainability | Reports repeated workflow names across files. |
| [`yaml-parse-error`](docs/rules/yaml-parse-error.md) | error | correctness | Reports invalid workflow YAML without crashing the scan. |

Want to add the next rule? The rule system is intentionally small: one rule class, focused tests, one docs page, and a README update. See [Adding a Rule](docs/contributing/adding-a-rule.md).

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

## JSON Output

Use JSON output for automation:

```bash
dotnet run --project src/GhActionsDoctor.Cli -- scan --format json
```

The JSON payload includes:

- summary counts,
- finding file path,
- severity,
- rule ID,
- category,
- message,
- suggestion,
- source line and column when available.

## GitHub Actions Usage

Add `gh-actions-doctor` to your CI workflow to catch issues in every PR:

```yaml
- uses: actions/setup-dotnet@v5
  with:
    dotnet-version: 10.0.x
- run: dotnet run --project src/GhActionsDoctor.Cli -- scan --fail-on warning
```

For a full example, see the [CI workflow](.github/workflows/ci.yml) that scans this repository.

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
dotnet tool install --tool-path .tmp/tools gh-actions-doctor --version 0.1.0-preview.1 --add-source src/GhActionsDoctor.Cli/bin/Release
.tmp/tools/gh-actions-doctor scan
```

## Contributing

Contributions are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) and [docs/contributing/adding-a-rule.md](docs/contributing/adding-a-rule.md) for guidance.

Good contribution paths:

- Add a focused workflow rule with tests and docs.
- Improve source locations for findings.
- Add reporter output such as SARIF or GitHub annotations.
- Add sample workflows for common project types.
- Improve parser resilience for real-world workflow YAML.

[Good first issues](https://github.com/Wezylnia/gh-actions-doctor/issues?q=is%3Aissue%20state%3Aopen%20label%3A%22good%20first%20issue%22) | [Help wanted](https://github.com/Wezylnia/gh-actions-doctor/issues?q=is%3Aissue%20state%3Aopen%20label%3A%22help%20wanted%22)

## Roadmap

See [docs/roadmap.md](docs/roadmap.md) for the release roadmap, [docs/project-status.md](docs/project-status.md) for current preview readiness, and [docs/rules/README.md](docs/rules/README.md) for the rule catalog.

## Repository Governance

The public repository is set up for small, reviewable contributions. `main` is protected, CI is required, and `CODEOWNERS` routes review to the maintainer. Stale-review dismissal, last-push approval, conversation resolution, disabled force pushes/deletions, admin emergency bypass, and GitHub Copilot automatic PR review are configured for pull requests targeting `main`.

## License

MIT. See [LICENSE](LICENSE).
