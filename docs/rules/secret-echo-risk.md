# secret-echo-risk

## Summary

Detects `run` steps that appear to echo, print, or redirect GitHub Actions secrets to stdout or files, which may leak them in build logs.

## Why it matters

GitHub Actions automatically masks secrets in logs, but only when the exact secret value appears. If a secret is echoed, formatted, or written to a file, it may bypass masking and appear in plaintext in build logs. This is a common source of credential leaks.

## How to fix

Use `env` to pass secrets safely to commands instead of interpolating them directly:

```yaml
- run: |
    echo "Token length: ${#TOKEN}"
  env:
    TOKEN: ${{ secrets.MY_TOKEN }}
```

Never echo raw secret values or redirect them to files.

## Default severity

`warning`

## Detection notes

- Using `::add-mask::` with a secret does not trigger this rule.
- Passing secrets through `env` without echoing does not trigger this rule.
