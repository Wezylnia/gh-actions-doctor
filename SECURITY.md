# Security Policy

## Supported Versions

`gh-actions-doctor` is currently pre-1.0. Security fixes target the latest released `0.1.x` version until a stable version exists.

| Version | Supported |
| --- | --- |
| `0.1.x` | Yes |
| `0.1.x-preview` | Yes |

## Reporting a Vulnerability

If you find a security issue, please do not open a public issue with exploit details.

Use GitHub's private vulnerability reporting if it is enabled for the repository, or contact the maintainer through GitHub.

Please include:

- affected version or commit
- reproduction steps
- expected impact
- any suggested fix, if known

## Scope

Security reports are welcome for:

- crashes on malicious workflow YAML
- unsafe file handling
- incorrect recommendations that could create unsafe workflow behavior
- package or release process issues

This tool does not execute GitHub Actions workflows and should not make network calls during scanning.
