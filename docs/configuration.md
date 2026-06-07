# Configuration

`gh-actions-doctor` reads `.gh-actions-doctor.yml` or `.gh-actions-doctor.yaml` from the current repository root by default.

Use `--config <path>` to load a specific file, or `--config none` to disable config loading.

## Example

```yaml
path: .github/workflows
format: text
failOn: warning
strict: true
include:
  - missing-permissions
  - missing-timeout
exclude:
  - action-not-sha-pinned
severity:
  missing-permissions: error
baseline: .gh-actions-doctor-baseline.json
```

## Fields

| Field | Description |
| --- | --- |
| `path` or `workflowPath` | Workflow file or directory to scan. |
| `format` | `text`, `json`, `github-annotations`, or `sarif`. |
| `failOn` or `fail-on` | `none`, `info`, `warning`, or `error`. |
| `strict` | `true` or `false`. |
| `include` | Rule IDs to run. May be a YAML list or comma-separated string. |
| `exclude` | Rule IDs to skip. May be a YAML list or comma-separated string. |
| `disabledRules` | Alias for `exclude`. |
| `severity` or `severityOverrides` | Mapping of rule IDs to `info`, `warning`, or `error`. |
| `baseline` | Path to a baseline JSON file for suppressing known findings. |

Suppressed findings can be shown at runtime with `--show-suppressions`. Baselines can be pruned with `--prune-baseline` when a baseline path is configured or passed with `--baseline`.

CLI-provided values take precedence over matching config fields. Fields that are not provided on the CLI continue to come from config.
