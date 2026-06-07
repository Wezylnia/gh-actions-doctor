# Changelog

All notable changes to this project will be documented in this file.

The project follows preview releases until the first stable `1.0.0`.

## 1.0.0 - 2026-06-07

### Added

- Stable release documentation and validation coverage.

### Changed

- Package version updated to `1.0.0`.
- Stabilized documented CLI, JSON, SARIF, configuration, baseline, suppression, and safe-fix behavior for public CI usage.

### Fixed

- Corrected stale installation and project status documentation from preview releases.

## 0.9.0 - 2026-06-07

### Added

- `release-workflow-overprivileged-token` security rule for overly broad token permissions in release-like workflows.
- `remote-script-execution` security rule for remote scripts piped directly to shells.
- `secret-echo-risk` security rule for steps that appear to echo or redirect secrets.
- SARIF `partialFingerprints` for improved Code Scanning alert tracking.
- Machine-readable JSON schemas for output, baseline, and configuration.

### Changed

- Package version updated to `0.9.0`.

## 0.8.0 - 2026-06-07

### Added

- Dedicated parser for the `fix` command.
- Rule-scoped safe fixes through `--rule missing-timeout,missing-permissions`.
- Parse-aware fixer behavior that skips invalid or complex YAML.
- Stable SHA-256 baseline fingerprints.
- `--prune-baseline` for removing stale baseline entries.
- `--show-suppressions` for optional visibility into baseline and inline suppressions.

### Changed

- Package version updated to `0.8.0`.

## 0.7.0 - 2026-06-07

### Added

- Baseline file support via `--baseline <path>` and `baseline` config field.
- Baseline generation via `--write-baseline <path>`.
- Inline suppression comments: `# gh-actions-doctor-disable-next-line` and `# gh-actions-doctor-disable-file`.
- Adoption controls for gradual rollout in repositories with existing workflow findings.

### Changed

- Package version updated to `0.7.0`.

## 0.6.0 - 2026-06-07

### Added

- `overbroad-id-token-permission` security rule for unused `id-token: write` permissions.
- `pull-request-target-untrusted-checkout` security rule for unsafe checkout of pull request head code.
- `untrusted-expression-in-run` security rule for untrusted event data interpolated directly into shell commands.

### Changed

- Package version updated to `0.6.0`.

## 0.5.0 - 2026-06-07

### Added

- Conservative `fix` command.
- Dry-run and apply modes for safe workflow edits.
- Safe fixer for missing top-level `permissions: contents: read`.
- Safe fixer for missing job `timeout-minutes: 30`.

### Changed

- Package version updated to `0.5.0`.

## 0.4.0 - 2026-06-07

### Added

- SARIF 2.1.0 output through `--format sarif`.
- SARIF rule metadata and source locations for GitHub Code Scanning.

### Changed

- Package version updated to `0.4.0`.

## 0.3.0 - 2026-06-07

### Added

- GitHub workflow annotation output through `--format github-annotations`.
- Official composite GitHub Action wrapper.
- CI smoke checks for GitHub-native usage.

### Changed

- Package version updated to `0.3.0`.

## 0.2.0 - 2026-06-05

### Added

- `.gh-actions-doctor.yml` and `.gh-actions-doctor.yaml` configuration loading from the current repository root.
- `--config <path|none>` CLI option.
- Config support for path, output format, fail threshold, strict mode, include/exclude rules, disabled rules, and per-rule severity overrides.
- Unit tests for config loading, config disabling, CLI precedence, and severity override behavior.
- Configuration reference documentation.

### Changed

- CLI-provided values now take precedence over matching config-file fields.
- Package version updated to `0.2.0`.

## 0.1.0 - 2026-06-05

### Added

- Expanded parser tests for workflow shape, scalar handling, and parse-error behavior.
- Direct reporter tests for stable text and JSON output.
- Workflow hardening checklist linked from the README.

### Changed

- Promoted the package version from `0.1.0-preview.1` to `0.1.0`.
- Updated `YamlDotNet` to `18.0.0`.
- Updated test dependencies: `Microsoft.NET.Test.Sdk` to `18.6.0`, `xunit` to `2.9.3`, and `coverlet.collector` to `10.0.1`.
- Pinned `xunit.runner.visualstudio` to the v2 adapter line to avoid duplicate test discovery for the current xUnit v2 suite.
- Refreshed README, project status, and roadmap for the polished MVP release.

## 0.1.0-preview.1 - 2026-05-30

### Added

- Initial .NET CLI tool package metadata.
- Workflow discovery for `.github/workflows/*.yml` and `.github/workflows/*.yaml`.
- YAML parsing with safe parse-error findings.
- Text and JSON report output.
- `--path`, `--format`, `--fail-on`, `--include`, `--exclude`, and `--strict` scan options.
- Source line and column information for many findings.
- Ten built-in workflow hygiene rules.
- Rule, CLI, scanner, and reporter tests.
- Sample good and bad workflows.
- Public rule documentation for the first preview rules.

### Known Gaps At 0.1.0

- SARIF output is not implemented yet.
- Configuration file support is not implemented yet.
- GitHub annotation output is not implemented yet.
- The official GitHub Action wrapper is not implemented yet.
