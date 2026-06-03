# broad-push-trigger

## Summary

Reports workflows that run on every push without branch, tag, or path filters.

## Why It Matters

Broad push triggers can run expensive workflows for changes that do not need them. This increases queue time, runner cost, and noise for maintainers.

## Recommendation

Add filters that match the workflow's purpose:

```yaml
on:
  push:
    branches:
      - main
    paths:
      - "src/**"
      - "tests/**"
      - ".github/workflows/ci.yml"
```

Keep broad triggers when the workflow genuinely needs to validate every push.

## Severity

`info`

## Category

`cost`
