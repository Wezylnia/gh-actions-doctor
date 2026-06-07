# pull-request-target-untrusted-checkout

Reports `pull_request_target` workflows that check out untrusted pull request head code.

## Why It Matters

`pull_request_target` runs with privileges from the base repository. Checking out and running untrusted pull request code in that context can expose secrets or write permissions.

## How To Fix

Use `pull_request` for untrusted code checks, or keep `pull_request_target` workflows on the trusted base repository checkout.

