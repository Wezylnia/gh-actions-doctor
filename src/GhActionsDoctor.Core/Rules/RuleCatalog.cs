namespace GhActionsDoctor.Core.Rules;

public static class RuleCatalog
{
    public static IReadOnlyList<IWorkflowRule> DefaultRules { get; } =
    [
        new MissingPermissionsRule(),
        new MutableActionReferenceRule(),
        new ActionNotShaPinnedRule(),
        new RiskyPullRequestTargetRule(),
        new MissingTimeoutRule(),
        new MissingConcurrencyRule(),
        new SetupNodeCacheMissingRule(),
        new BroadPushTriggerRule(),
        new DuplicateWorkflowNameRule(),
        new OverbroadIdTokenPermissionRule(),
        new PullRequestTargetUntrustedCheckoutRule(),
        new UntrustedExpressionInRunRule()
    ];
}
