# yaml-parse-error

## Summary

Reports workflow YAML files that cannot be parsed.

## Why It Matters

Invalid workflow YAML can prevent CI from running or hide other workflow issues. The scanner reports parse errors as findings instead of crashing the whole scan.

## Recommendation

Fix the YAML syntax and rerun the scanner. When possible, validate the workflow in a small pull request before merging.

## Severity

`error`

## Category

`correctness`
