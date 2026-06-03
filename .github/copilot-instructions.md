# Copilot Instructions

This repository contains `gh-actions-doctor`, a .NET CLI for static GitHub Actions workflow analysis.

When reviewing or generating changes:

- Keep scanning static. Do not execute workflow steps, package installs, Docker builds, or arbitrary repository code.
- Prefer small rule classes under `src/GhActionsDoctor.Core/Rules`.
- Every new rule should include registration in `RuleCatalog`, focused tests, rule docs, and README updates when user-visible.
- Keep rule messages practical and cautious. Avoid overstating heuristic findings.
- Do not add real secrets, tokens, webhook URLs, or private workflow content to tests or docs.
- Preserve deterministic text and JSON output for CI usage.
- Keep security recommendations least-privilege and aligned with GitHub Actions documentation.
