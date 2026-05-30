# missing-timeout

## What It Detects

Reports jobs that do not define `timeout-minutes`.

## Why It Matters

Jobs without timeouts can hang until GitHub's maximum runtime is reached. Explicit timeouts reduce wasted CI minutes and make failures easier to reason about.

## Bad Example

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - run: npm test
```

## Better Example

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    timeout-minutes: 15
    steps:
      - run: npm test
```

## Default Severity

`warning`

## Strict Mode

Strict mode does not currently change this rule's severity.

## False Positive Notes

Long-running release or integration jobs may need a larger timeout. The rule does not prescribe a specific value; it only asks for an explicit limit.
