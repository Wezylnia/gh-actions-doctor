# Roadmap

This roadmap describes the public path from the current MVP to `v1.0.0`.

## v0.1.0-preview.1 - shipped

Focus: package and documentation readiness.

- Pack as a .NET global tool.
- Document installation and quick start.
- Add initial rule docs.
- Add release files.
- Run the tool against this repository in CI.

## v0.1.0 - shipped

Focus: polished MVP.

- Document all current rules.
- Add focused parser tests.
- Add focused reporter tests.
- Add CLI end-to-end tests.
- Improve remaining source locations.
- Publish the first polished release.
- Refresh package dependencies and release metadata.
- Link the workflow hardening checklist from user-facing docs.

## v0.2.0

Focus: configuration support.

- Add `.gh-actions-doctor.yml`.
- Allow rule disabling.
- Allow severity overrides.
- Allow file/rule ignores.
- Add config parse validation.
- Document config precedence with `--include`, `--exclude`, and `--strict`.

## v0.3.0

Focus: GitHub Actions integration.

- Add GitHub annotation output.
- Add an official GitHub Action wrapper.

## v0.4.0

Focus: SARIF output.

- Add `--format sarif`.
- Include rule metadata and source locations.
- Document GitHub code scanning usage.

## v0.5.0

Focus: safe auto-fix.

- Add a conservative `fix` command.
- Support dry-run.
- Preserve workflow YAML as much as practical.

## v1.0.0

Focus: stable public release.

- Stable CLI.
- Stable JSON schema.
- Stable config schema.
- Documented rules.
- CI-ready output formats.
- Reliable tests across supported platforms.
