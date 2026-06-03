# Workflow Hardening Checklist

Use this checklist before merging a new or updated GitHub Actions workflow.

## Permissions

- Add a top-level `permissions` block and grant only the scopes the workflow actually needs.
- Keep the default token read-only unless a job must write releases, checks, pull requests, or packages.
- See [`missing-permissions`](rules/missing-permissions.md) for the baseline permission rule.

## Action References

- Avoid mutable refs such as `@main`, `@master`, or `@latest`.
- Pin third-party actions to full commit SHAs for release workflows and other higher-trust paths.
- See [`mutable-action-reference`](rules/mutable-action-reference.md) and [`action-not-sha-pinned`](rules/action-not-sha-pinned.md).

## Runtime Limits

- Set `timeout-minutes` on each job so stuck runners do not burn minutes indefinitely.
- Add `concurrency` when repeated pushes or reruns should cancel older in-flight jobs.
- See [`missing-timeout`](rules/missing-timeout.md) and [`missing-concurrency`](rules/missing-concurrency.md).

## Trigger Scope

- Limit `push` triggers with branch, tag, or path filters when the workflow does not need to run on every change.
- Treat `pull_request_target` as high risk and avoid it unless the workflow truly needs base-repository privileges.
- See [`broad-push-trigger`](rules/broad-push-trigger.md) and [`risky-pull-request-target`](rules/risky-pull-request-target.md).

## Dependencies And Secrets

- Turn on dependency caching when installs run through `actions/setup-node`.
- Keep secrets out of workflow examples, logs, fixtures, and committed sample files.
- Prefer synthetic values in tests and documentation so examples stay safe to copy.
- See [`setup-node-cache-missing`](rules/setup-node-cache-missing.md) and the [Security Review Checklist](security-review-checklist.md).
