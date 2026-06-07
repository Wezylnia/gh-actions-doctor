using GhActionsDoctor.Core.Fixing;

namespace GhActionsDoctor.Tests.Fixing;

public sealed class WorkflowFixerTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"gh-actions-doctor-fix-{Guid.NewGuid():N}");

    public WorkflowFixerTests()
    {
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public void Dry_run_reports_missing_permissions_and_timeout_without_writing()
    {
        var path = WriteWorkflow("""
        name: CI
        on:
          push:
        jobs:
          build:
            runs-on: ubuntu-latest
            steps:
              - run: dotnet test
        """);

        var original = File.ReadAllText(path);
        var result = new WorkflowFixer().Fix(Path.Combine(_root, ".github", "workflows"), apply: false);

        Assert.Equal(2, result.FixCount);
        Assert.Equal(original, File.ReadAllText(path));
    }

    [Fact]
    public void Apply_adds_permissions_and_timeout()
    {
        var path = WriteWorkflow("""
        name: CI
        on:
          push:
        jobs:
          build:
            runs-on: ubuntu-latest
            steps:
              - run: dotnet test
        """);

        var result = new WorkflowFixer().Fix(Path.Combine(_root, ".github", "workflows"), apply: true);
        var updated = File.ReadAllText(path);

        Assert.Equal(2, result.FixCount);
        Assert.Contains("permissions:", updated);
        Assert.Contains("  contents: read", updated);
        Assert.Contains("    timeout-minutes: 30", updated);
    }

    [Fact]
    public void Apply_preserves_existing_timeout()
    {
        var path = WriteWorkflow("""
        name: CI
        on:
          push:
        permissions:
          contents: read
        jobs:
          build:
            runs-on: ubuntu-latest
            timeout-minutes: 10
            steps:
              - run: dotnet test
        """);

        var result = new WorkflowFixer().Fix(Path.Combine(_root, ".github", "workflows"), apply: true);
        var updated = File.ReadAllText(path);

        Assert.Equal(0, result.FixCount);
        Assert.Contains("timeout-minutes: 10", updated);
        Assert.DoesNotContain("timeout-minutes: 30", updated);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private string WriteWorkflow(string content)
    {
        var directory = Path.Combine(_root, ".github", "workflows");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "ci.yml");
        File.WriteAllText(path, content);
        return path;
    }
}
