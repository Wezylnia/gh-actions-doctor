# setup-node-cache-missing

## Summary

Checks `actions/setup-node` steps for a `cache` input.

## Why it's important

Without dependency caching, commands like `npm ci`, `yarn install`, or `pnpm install` must download packages from the registry on every workflow run. That makes CI slower and can waste runner minutes, especially on pull requests that run frequently.

`actions/setup-node` has built-in caching for common Node.js package managers, so most workflows can avoid re-downloading dependencies by adding one input.

## How to fix

Add `cache` to the action's `with` block and set it to the package manager used by the workflow:

- `npm`
- `yarn`
- `pnpm`

If the workflow installs dependencies from a non-standard lockfile path, also set `cache-dependency-path` so the cache key tracks the correct file.

## Examples

### Bad

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: 22
      - run: npm ci
      - run: npm test
```

### Good

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: 22
          cache: npm
      - run: npm ci
      - run: npm test
```

## Default Severity

`info`

## Strict Mode

Strict mode does not currently change this rule's severity.

## False Positive Notes

Some workflows intentionally avoid caching because dependency installation is trivial, the package manager cache is managed elsewhere, or the job uses a custom dependency setup. In those cases, exclude this rule for the workflow.
