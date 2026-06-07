# Adoption Guide

Use this flow when adding `gh-actions-doctor` to an existing repository.

## First Scan

Run without failing the build:

```bash
gh-actions-doctor scan --path .github/workflows --fail-on none
```

## Create A Baseline

Record existing findings:

```bash
gh-actions-doctor scan --write-baseline .gh-actions-doctor-baseline.json --fail-on none
```

Commit the baseline file only after reviewing it.

## Enforce New Regressions

Use the baseline in CI:

```bash
gh-actions-doctor scan --baseline .gh-actions-doctor-baseline.json --fail-on warning
```

New findings that are not in the baseline can fail the build.

## Use Inline Suppressions Sparingly

Supported comments:

```yaml
# gh-actions-doctor-disable-next-line missing-timeout
# gh-actions-doctor-disable-file action-not-sha-pinned
```

Prefer fixing the workflow. If suppression is necessary, add a nearby human-readable reason.

