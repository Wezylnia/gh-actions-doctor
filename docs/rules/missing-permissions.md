# missing-permissions

## What It Detects

Reports workflow files that do not define a top-level `permissions` block.

## Why It Matters

Without an explicit `permissions` block, the workflow may receive broader `GITHUB_TOKEN` permissions than it needs. Least-privilege token permissions reduce the impact of compromised jobs or unsafe workflow changes.

## Bad Example

```yaml
name: CI

on:
  push:

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
```

## Better Example

```yaml
name: CI

on:
  push:

permissions:
  contents: read

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
```

## Default Severity

`warning`

## Strict Mode

In `--strict` mode this rule is promoted to `error`.

## False Positive Notes

Some workflows may rely on repository or organization defaults. The recommendation is still to make permissions explicit in the workflow for readability and least privilege.
