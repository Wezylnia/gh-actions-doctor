# missing-concurrency

## Summary

Reports workflows that are likely to benefit from a `concurrency` group.

## Why It Matters

Without `concurrency`, repeated pushes to the same branch can leave obsolete runs consuming runner minutes. This is especially noisy for pull requests and active feature branches.

## Recommendation

Add a workflow-level concurrency group when cancellation is safe:

```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true
```

Use caution for deployment workflows where canceling an in-progress run may be unsafe.

## Severity

`info`

## Category

`cost`
