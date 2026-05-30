# How to add a rule

This guide walks through adding a new workflow scanning rule to `gh-actions-doctor`.

## Overview

A rule is a class that implements `IWorkflowRule` and inspects workflow YAML to produce findings. Each rule lives in its own file under `src/GhActionsDoctor.Core/Rules/`.

## Steps

### 1. Create the rule class

Add a new file in `src/GhActionsDoctor.Core/Rules/`. Use this template:

```csharp
using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Parsing;
using GhActionsDoctor.Core.Yaml;

namespace GhActionsDoctor.Core.Rules;

public class MyNewRule : IWorkflowRule
{
    public string Id => "my-new-rule";
    public string Name => "My New Rule";
    public RuleCategory Category => RuleCategory.Security; // pick the right category
    public RuleSeverity DefaultSeverity => RuleSeverity.Warning;

    public string Description =>
        "Short description of what this rule detects.";

    public string Suggestion =>
        "Actionable suggestion for how to fix the issue.";

    public IReadOnlyList<Finding> Evaluate(
        WorkflowFile workflowFile,
        RuleContext context)
    {
        var findings = new List<Finding>();

        // Inspect workflowFile and add findings here.

        return findings;
    }
}
```

### 2. Choose the right category and severity

| Category | When to use |
| --- | --- |
| `Security` | The finding relates to permissions, secrets, or attack surface. |
| `Reliability` | The finding could cause unexpected failures or hangs. |
| `Performance` | The finding slows down workflow execution. |
| `Cost` | The finding wastes GitHub Actions minutes unnecessarily. |
| `Maintainability` | The finding makes workflows harder to read or maintain. |
| `Correctness` | The workflow is malformed or invalid. |

Severity guidelines:

| Severity | When to use |
| --- | --- |
| `Error` | The workflow will definitely fail or has a serious security risk. |
| `Warning` | The workflow is likely misconfigured or risky. |
| `Info` | A best-practice suggestion that may be optional. |

### 3. Write tests

Add tests in `tests/GhActionsDoctor.Tests/Rules/`:

```csharp
[Fact]
public void MyNewRule_FlagsProblematicPattern()
{
    var yaml = @"
name: test
on: push
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
";

    var workflowFile = WorkflowParser.Parse("test.yml", yaml);
    var rule = new MyNewRule();
    var context = new RuleContext(new ScanOptions());

    var findings = rule.Evaluate(workflowFile, context);

    Assert.NotEmpty(findings);
    Assert.Equal("my-new-rule", findings[0].RuleId);
}
```

### 4. Register the rule

Open `src/GhActionsDoctor.Core/Rules/RuleCatalog.cs` and add your rule to the `All` list:

```csharp
new MyNewRule(),
```

### 5. Add rule documentation

Create `docs/rules/my-new-rule.md` with this structure:

```markdown
# my-new-rule

## What it detects

## Why it matters

## Example

### Bad

```yaml
```

### Good

```yaml
```

## False positives

## See also
```

### 6. Update the README rule table

Add a row to the rules table in `README.md`.

## Rule checklist

- [ ] Rule class added under `src/GhActionsDoctor.Core/Rules/`
- [ ] Rule implements `IWorkflowRule`
- [ ] Category and severity are appropriate
- [ ] Unit tests added under `tests/GhActionsDoctor.Tests/Rules/`
- [ ] Rule registered in `RuleCatalog`
- [ ] Rule documentation added under `docs/rules/`
- [ ] README rule table updated
- [ ] False positive scenarios considered and documented

## Running locally

```bash
dotnet restore
dotnet build GhActionsDoctor.sln
dotnet test GhActionsDoctor.sln
dotnet run --project src/GhActionsDoctor.Cli -- scan --path samples/bad --fail-on none
```

## Need help?

Open a [Discussions](https://github.com/Wezylnia/gh-actions-doctor/discussions) post or comment on a rule-related issue.
