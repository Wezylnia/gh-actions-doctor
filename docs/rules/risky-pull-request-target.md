# risky-pull-request-target

## What It Detects

Reports workflows that use the `pull_request_target` trigger.

The rule reports an `error` when the workflow also uses high-risk patterns such as checking out code or running scripts with write permissions.

## Why It Matters

`pull_request_target` runs in the context of the base repository and can access privileged tokens or secrets depending on workflow configuration. It is useful for trusted automation, but risky when combined with untrusted pull request code.

## Risky Example

```yaml
name: Risky

on:
  pull_request_target:

permissions:
  contents: write

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: npm test
```

## Safer Direction

Use `pull_request` for workflows that build, test, or execute pull request code:

```yaml
on:
  pull_request:

permissions:
  contents: read
```

If `pull_request_target` is required, avoid checking out or executing untrusted pull request code and keep permissions minimal.

## Default Severity

- `warning` when `pull_request_target` is present
- `error` when a high-risk pattern is detected

## Strict Mode

Strict mode does not currently change this rule's severity because high-risk patterns are already reported as errors.

## False Positive Notes

Some labeler or triage workflows safely use `pull_request_target`. Review the finding and keep the workflow narrowly scoped.
