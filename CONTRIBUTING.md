# Contributing

Thanks for considering a contribution to `gh-actions-doctor`.

The project is intentionally small and rule-based. The best contributions are focused, well-tested changes that improve workflow scanning quality without making the tool heavy.

## Development Setup

Requirements:

- .NET SDK 10

Restore, build, and test:

```bash
dotnet restore
dotnet build GhActionsDoctor.sln
dotnet test GhActionsDoctor.sln
```

Run the CLI locally:

```bash
dotnet run --project src/GhActionsDoctor.Cli -- scan --path samples/bad --fail-on none
```

## Contribution Guidelines

- Keep changes small and focused.
- Add or update tests for behavior changes.
- Prefer a new rule class for a new rule.
- Keep rules deterministic and static-analysis only.
- Avoid network calls in core analysis.
- Avoid auto-fixing workflows unless working on the explicit future `fix` command.
- Update public docs when behavior changes.

## Adding a Rule

See [docs/contributing/adding-a-rule.md](docs/contributing/adding-a-rule.md) for a step-by-step guide.

A typical rule contribution should include:

- a new rule class under `src/GhActionsDoctor.Core/Rules`
- rule registration in `RuleCatalog`
- focused tests under `tests/GhActionsDoctor.Tests/Rules`
- a rule document under `docs/rules`
- README rule table updates if needed

## Commit Style

Use clear commit messages such as:

```txt
feat: add setup-python cache rule
fix: handle empty workflow directories
docs: document missing-timeout rule
test: add parser tests for invalid yaml
```
