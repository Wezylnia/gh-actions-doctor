# duplicate-workflow-name

## Summary

Reports repeated workflow names across multiple workflow files.

## Why It Matters

Duplicate workflow names make the Actions UI harder to scan and can confuse status checks, branch protection rules, and contributors reading CI results.

## Recommendation

Give each workflow a clear, unique name:

```yaml
name: Build
```

```yaml
name: Release
```

## Severity

`info`

## Category

`maintainability`
