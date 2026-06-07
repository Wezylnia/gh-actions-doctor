# untrusted-expression-in-run

Reports shell commands that interpolate untrusted GitHub event data directly into `run` steps.

## Why It Matters

Pull request titles, issue bodies, and comment bodies can contain attacker-controlled text. Direct interpolation into shell commands may lead to command injection.

## How To Fix

Move untrusted values into `env` and quote the variable in the shell command.

