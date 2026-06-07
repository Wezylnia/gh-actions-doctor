# Code Scanning

`gh-actions-doctor` can emit SARIF 2.1.0 for GitHub Code Scanning.

```yaml
name: gh-actions-doctor

on:
  pull_request:
  push:
    branches: [main]

permissions:
  contents: read
  security-events: write

jobs:
  scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v6
      - uses: Wezylnia/gh-actions-doctor@v0.7.0
        with:
          format: sarif
          fail-on: none
          output: gh-actions-doctor.sarif
      - uses: github/codeql-action/upload-sarif@v4
        with:
          sarif_file: gh-actions-doctor.sarif
```
