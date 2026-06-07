# overbroad-id-token-permission

Reports workflows or jobs that grant `id-token: write` when no step appears to use OIDC authentication or signing.

## Why It Matters

OIDC tokens are powerful. They should be granted only to jobs that need cloud authentication or artifact signing.

## How To Fix

Remove `id-token: write`, or move it to the specific job that uses actions such as `aws-actions/configure-aws-credentials`, `google-github-actions/auth`, `azure/login`, or signing commands such as `cosign sign`.

