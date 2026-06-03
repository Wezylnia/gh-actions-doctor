# Security Review Checklist

Use this checklist for changes that touch scanning, parsing, reporting, packaging, or CI.

## Input Handling

- Treat workflow YAML and repository files as untrusted input.
- Do not execute workflow steps, shell scripts, package managers, Docker builds, or arbitrary repository code.
- Keep parsing resilient to malformed YAML.
- Avoid network calls in core scanning.

## Secret Safety

- Do not commit real tokens, webhook URLs, credentials, or private workflow content.
- Redact sensitive-looking values in test fixtures, examples, logs, and reports.
- Prefer synthetic values that do not trigger GitHub push protection.

## Rule Safety

- Keep security recommendations least-privilege.
- Use cautious wording for heuristic findings.
- Add false-positive tests for new rules.

## CI and Release

- Keep workflow permissions minimal.
- Keep dependency update PRs reviewable.
- Verify package metadata before release.
- Run build, tests, and a local sample scan before merging.
