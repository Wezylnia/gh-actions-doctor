# mutable-action-reference

## What It Detects

Reports action references that use mutable branch-like refs such as:

- `main`
- `master`
- `latest`
- `dev`
- `develop`
- `trunk`

## Why It Matters

Mutable refs can change without notice. A workflow that worked yesterday can run different action code today, which creates reliability and supply-chain risk.

## Bad Example

```yaml
steps:
  - uses: owner/action@main
```

## Better Example

```yaml
steps:
  - uses: owner/action@v1
```

For stricter security, pin to a full commit SHA:

```yaml
steps:
  - uses: owner/action@0123456789abcdef0123456789abcdef01234567
```

## Default Severity

`warning`

## Strict Mode

In `--strict` mode this rule is promoted to `error`.

## False Positive Notes

Some internal repositories intentionally track a moving branch. In those cases, use `--exclude mutable-action-reference` for now. Configuration-file support is planned for a future release.
