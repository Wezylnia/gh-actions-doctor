# GitHub Action

Use the composite action to scan workflow files in pull requests.

```yaml
- uses: actions/checkout@v6
- uses: Wezylnia/gh-actions-doctor@v1.0.0
  with:
    path: .github/workflows
    format: github-annotations
    fail-on: warning
```

## Inputs

| Input | Default | Description |
| --- | --- | --- |
| `path` | `.github/workflows` | Workflow file or directory to scan. |
| `format` | `github-annotations` | `text`, `json`, `github-annotations`, or `sarif`. |
| `fail-on` | `warning` | Severity threshold for non-zero exit code. |
| `config` | empty | Optional config file path. |
| `dotnet-version` | `10.0.x` | .NET SDK version. |
| `output` | empty | Optional path where scanner output is written. |
