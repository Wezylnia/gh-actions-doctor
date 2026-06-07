# release-workflow-overprivileged-token

## Summary

Detects workflows triggered by `release`, `workflow_dispatch`, or tag push patterns (like `v*`) that grant overly broad token permissions (`write-all`, `contents: write`, `packages: write`) without a legitimate release/publish step.

## Why it matters

Release workflows have access to sensitive repository operations. Granting `write-all` or broad write permissions without a clear publishing step increases the attack surface. An attacker who compromises a release workflow could modify releases, push tags, or publish packages.

## How to fix

Scope permissions to the minimum required. If publishing releases, grant `contents: write` only in the specific job that needs it:

```yaml
permissions:
  contents: read

jobs:
  release:
    permissions:
      contents: write
    steps:
      - uses: softprops/action-gh-release@v2
```

## Default severity

`warning`
