# remote-script-execution

## Summary

Detects `run` steps that pipe a remotely downloaded script directly to a shell interpreter (e.g., `curl ... | bash`, `wget ... | sh`, `irm ... | iex`).

## Why it matters

Piping a remote script directly to a shell is dangerous because the script executes immediately without inspection. If the remote server is compromised or the connection is intercepted, arbitrary code runs in the CI environment with whatever permissions the workflow has.

## How to fix

Download the script to a file first, verify its integrity (e.g., checksum), then execute:

```yaml
- run: |
    curl -fsSL https://example.com/install.sh -o install.sh
    echo "expected-sha256 install.sh" | sha256sum -c
    bash install.sh
```

Or pin to a specific version and checksum.

## Default severity

`warning`

## Detection notes

- `curl -o file` or `wget -O file` without piping to a shell does not trigger this rule.
