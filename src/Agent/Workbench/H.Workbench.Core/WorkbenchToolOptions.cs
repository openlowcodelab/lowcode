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

    public BudgetOptions Budget { get; set; } = new();

    public VerificationOptions Verification { get; set; } = new();

    /// <summary>
    /// 数据库工具可引用的数据源白名单：模型只能按名字引用，不能自带连接串。
    /// 连接串目前仍是明文配置（AES 加密/凭据库属后续项），因此这里只应放低权限账号。
    /// </summary>
    public List<DataSourceOptions> DataSources { get; set; } = new();
}

/// <summary>
/// 登记给数据库工具使用的数据源
/// </summary>
public class DataSourceOptions
{
    /// <summary>模型引用它时用的名字（如 orderdb）</summary>
    public string Name { get; set; } = string.Empty;

    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>只读源会拒绝 ExecuteCommandAsync 的写操作</summary>
    public bool ReadOnly { get; set; } = true;
}

/// <summary>
/// 单次运行预算与配额：无人值守执行（定时任务/工作流）失控时的兜底闸门
/// </summary>
public class BudgetOptions
{
    /// <summary>
    /// 单次执行的 token 上限（输入+输出累计），0=不限。
    /// 默认给足正常工作的余量，只拦失控循环
    /// </summary>
    public int MaxTokensPerRun { get; set; } = 400000;

    /// <summary>
    /// 单次执行挂钟上限（秒），须大于 ToolTimeoutSeconds，否则长工具被提前掐
    /// </summary>
    public int MaxWallClockSeconds { get; set; } = 3600;

    /// <summary>
    /// 宿主内并发执行上限（含工作流与定时任务）
    /// </summary>
    public int MaxConcurrentRuns { get; set; } = 4;
}

/// <summary>
/// 结果验收（Verifier）配置：员工自评为什么默认开——没有裁决的"完成"只是员工的说法
/// </summary>
public class VerificationOptions
{
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 送审的结果/证据截断长度，控制验收本身的 token 成本
    /// </summary>
    public int MaxEvidenceChars { get; set; } = 4000;
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
