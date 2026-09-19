namespace H.Workbench.Core;

/// <summary>
/// Workbench 工具运行时配置，绑定 appsettings 的 "Workbench" 节
/// </summary>
public class WorkbenchToolOptions
{
    public const string SectionName = "Workbench";

    /// <summary>
    /// Git 工具配置（可执行文件、服务端工作目录等）
    /// </summary>
    public GitToolOptions Git { get; set; } = new();

    /// <summary>
    /// 工具执行兜底超时（秒）。须大于最耗时工具的内部超时（git clone 默认 600 秒）
    /// </summary>
    public int ToolTimeoutSeconds { get; set; } = 660;

    /// <summary>
    /// 员工级工具隔离开关：true 时绑定了技能的员工只看到自己的技能工具 + MCP 工具
    /// </summary>
    public bool ToolIsolationEnabled { get; set; } = true;
}

public class GitToolOptions
{
    public string ExecutablePath { get; set; } = "git";

    /// <summary>
    /// 服务端仓库工作目录（所有 clone 落在此目录内）；为空时 git/工作区工具拒绝执行
    /// </summary>
    public string WorkDir { get; set; } = string.Empty;

    public int CloneTimeoutSeconds { get; set; } = 600;

    public int DefaultTimeoutSeconds { get; set; } = 120;

    public string AuthorName { get; set; } = "workbench-agent";

    public string AuthorEmail { get; set; } = "workbench-agent@local";
}
