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

    public ApprovalOptions Approval { get; set; } = new();

    public ShellToolOptions Shell { get; set; } = new();

    public TraceOptions Trace { get; set; } = new();

    public MemoryInjectionOptions Memory { get; set; } = new();
}

/// <summary>
/// 历史经验记忆运行时注入配置（记忆由会话自动抽取，检索回填形成学习闭环）
/// </summary>
public class MemoryInjectionOptions
{
    /// <summary>
    /// 默认开启：符合"默认即产出"原则；关闭仅用于排障
    /// </summary>
    public bool Enabled { get; set; } = true;

    public int TopN { get; set; } = 4;

    public int MaxCharsPerMemory { get; set; } = 300;
}

/// <summary>
/// 工具审批门配置
/// </summary>
public class ApprovalOptions
{
    /// <summary>
    /// 审批门总开关；false 时回退到阶段A行为（需审批工具直接执行）
    /// </summary>
    public bool Enabled { get; set; } = true;

    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// 单次执行最多打扰用户几次，超出后按 SkippedNonInteractive 处理
    /// </summary>
    public int MaxPerExecution { get; set; } = 3;

    /// <summary>
    /// 非交互路径（定时任务/立即执行）遇需审批工具的裁决：Deny | Allow
    /// </summary>
    public string NonInteractivePolicy { get; set; } = "Deny";
}

/// <summary>
/// workspace_shell 工具配置
/// </summary>
public class ShellToolOptions
{
    /// <summary>
    /// 远程命令执行总开关（高危能力，一键关停）
    /// </summary>
    public bool Enabled { get; set; } = true;

    public int DefaultTimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// 命令超时上限（秒），必须小于 ToolTimeoutSeconds，否则被兜底超时先掐
    /// </summary>
    public int MaxTimeoutSeconds { get; set; } = 600;

    public int MaxOutputChars { get; set; } = 8000;
}

/// <summary>
/// 执行轨迹落库配置
/// </summary>
public class TraceOptions
{
    /// <summary>
    /// false 时关闭轨迹/产物落库，仅保留 SSE 推送（排障用）
    /// </summary>
    public bool Enabled { get; set; } = true;

    public int MaxArgumentsChars { get; set; } = 4000;

    public int MaxContentChars { get; set; } = 8000;
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
