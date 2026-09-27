namespace H.Workbench.Core.Agents;

/// <summary>
/// 单次 ReAct 运行的资源预算。缺省值即"不设限"，由 Workbench:Budget 配置注入。
/// </summary>
public sealed record RunBudget(int MaxTotalTokens = 0, TimeSpan? WallClockLimit = null)
{
    public static readonly RunBudget Unlimited = new();

    public bool HasTokenLimit => MaxTotalTokens > 0;
    public bool HasWallClockLimit => WallClockLimit.HasValue;
}
