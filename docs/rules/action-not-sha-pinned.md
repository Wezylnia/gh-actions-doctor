# action-not-sha-pinned

## What It Detects

Reports third-party actions that are pinned to a tag or branch instead of a full 40-character commit SHA.

First-party `actions/*` references are currently allowed by default because version tags such as `actions/checkout@v4` are common and expected in many repositories.

## Why It Matters

Tags can be moved. Pinning third-party actions to a commit SHA gives stronger supply-chain protection when a repository needs strict reproducibility.

## Bad Example

```yaml
steps:
  - uses: docker/login-action@v3
```

## Better Example

```yaml
steps:
  - uses: docker/login-action@0123456789abcdef0123456789abcdef01234567
```

## Default Severity

`info`

## Strict Mode

In `--strict` mode this rule is promoted to `warning`.

## False Positive Notes

Many teams accept trusted third-party version tags. This rule is intentionally informational by default. Future configuration support will allow allowlists and severity overrides.
