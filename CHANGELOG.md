# Changelog

All notable changes to this project will be documented in this file.

The project follows preview releases until the first stable `1.0.0`.

## 0.1.0-preview.1 - Unreleased

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
