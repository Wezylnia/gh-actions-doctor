# Roadmap

This roadmap describes the public path from the current preview release to `v1.0.0`.

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

## v0.2.0 - shipped

Focus: configuration support.

- Add `.gh-actions-doctor.yml`.
- Allow rule disabling.
- Allow severity overrides.
- Add config parse validation.
- Document config precedence with `--include`, `--exclude`, and `--strict`.

## v0.3.0 - shipped

Focus: GitHub Actions integration.

- Add GitHub annotation output.
- Add an official GitHub Action wrapper.

## v0.4.0 - shipped

Focus: SARIF output.

- Add `--format sarif`.
- Include rule metadata and source locations.
- Document GitHub code scanning usage.

## v0.5.0 - shipped

Focus: safe auto-fix.

- Add a conservative `fix` command.
- Support dry-run and apply mode.
- Add safe fixes for missing permissions and timeouts.

## v0.6.0 - shipped

Focus: advanced security rules.

- Add overbroad OIDC permission detection.
- Add unsafe `pull_request_target` checkout detection.
- Add untrusted expression usage detection in shell commands.

## v0.7.0 - shipped

Focus: adoption controls.

- Add baseline suppression support.
- Add baseline generation.
- Add inline suppression comments.

## v0.8.0 - shipped

Focus: existing feature hardening.

- Split fix command into dedicated parser.
- Make fixer rule-scoped and parse-aware (skip invalid/complex YAML).
- Add baseline fingerprints and prune.
- Add `--show-suppressions`.

## v0.9.0 - shipped

Focus: security coverage and v1 readiness.

- Add release workflow overprivileged token rule.
- Add remote script execution rule.
- Add secret echo risk rule.
- Add SARIF `partialFingerprints`.
- Add machine-readable JSON schemas.

## v1.0.0 - shipped

Focus: first stable public release.

- Stabilize CLI, JSON, SARIF, configuration, baseline, and suppression behavior.
- Refresh documentation and project metadata for stable release.

## After v1.0

- Stable CLI.
- Stable JSON and SARIF behavior.
- Stable config schema.
- Documented rules.
- CI-ready output formats.
- Reliable tests across supported platforms.
