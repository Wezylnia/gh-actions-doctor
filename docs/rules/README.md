# Rule Catalog

`gh-actions-doctor` rules focus on GitHub Actions security, reliability, performance, cost, correctness, and maintainability.

| Rule | Category | Description |
| --- | --- | --- |
| [`missing-permissions`](missing-permissions.md) | security | Detects workflows without an explicit top-level permissions block. |
| [`mutable-action-reference`](mutable-action-reference.md) | security | Detects actions referenced by mutable refs such as `main`, `master`, or `latest`. |
| [`action-not-sha-pinned`](action-not-sha-pinned.md) | security | Detects third-party actions that are not pinned to a full commit SHA. |
| [`risky-pull-request-target`](risky-pull-request-target.md) | security | Detects risky `pull_request_target` usage. |
| [`missing-timeout`](missing-timeout.md) | reliability | Detects jobs without `timeout-minutes`. |
| [`missing-concurrency`](missing-concurrency.md) | cost | Detects workflows likely to benefit from `concurrency`. |
| [`setup-node-cache-missing`](setup-node-cache-missing.md) | performance | Detects `actions/setup-node` without dependency caching. |
| [`broad-push-trigger`](broad-push-trigger.md) | cost | Detects broad push triggers without branch, tag, or path filters. |
| [`duplicate-workflow-name`](duplicate-workflow-name.md) | maintainability | Detects repeated workflow names across files. |
| [`overbroad-id-token-permission`](overbroad-id-token-permission.md) | security | Detects unused `id-token: write` permissions. |
| [`pull-request-target-untrusted-checkout`](pull-request-target-untrusted-checkout.md) | security | Detects unsafe checkout of pull request head code in `pull_request_target`. |
| [`untrusted-expression-in-run`](untrusted-expression-in-run.md) | security | Detects untrusted event data interpolated directly into shell commands. |
| [`yaml-parse-error`](yaml-parse-error.md) | correctness | Reports invalid workflow YAML without crashing the scan. |

New rules should include focused tests, rule docs, and README updates.
