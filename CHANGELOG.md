# Changelog

All notable changes to this project will be documented in this file.

The project follows preview releases until the first stable `1.0.0`.

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

### Known Gaps

- SARIF output is not implemented yet.
- Configuration file support is not implemented yet.
- GitHub annotation output is not implemented yet.
- The official GitHub Action wrapper is not implemented yet.
