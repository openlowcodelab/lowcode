using System.ComponentModel.DataAnnotations;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试执行记录
/// </summary>
public class CaseExecutionRecordDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "测试用例ID不能为空")]
    public long CaseId { get; set; }

    [Required(ErrorMessage = "测试用例名称不能为空")]
    public string CaseName { get; set; } = string.Empty;

    [Required(ErrorMessage = "所属项目不能为空")]
    public long ProjectId { get; set; }

    [Required(ErrorMessage = "执行环境不能为空")]
    public long EnvId { get; set; }

    public string EnvName { get; set; } = string.Empty;

    /// <summary>
    /// 执行状态
    /// </summary>
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Running;

    /// <summary>
    /// 执行开始时间
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 执行结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 执行总耗时（毫秒）
    /// </summary>
    public long Duration { get; set; }

    /// <summary>
    /// 总步骤数
    /// </summary>
    public int TotalSteps { get; set; }

    /// <summary>
    /// 成功步骤数
    /// </summary>
    public int SuccessSteps { get; set; }

    /// <summary>
    /// 失败步骤数
    /// </summary>
    public int FailedSteps { get; set; }

    /// <summary>
    /// 跳过步骤数
    /// </summary>
    public int SkippedSteps { get; set; }

    /// <summary>
    /// 执行步骤记录
    /// </summary>
    public List<StepExecutionRecord> StepRecords { get; set; } = new();

    /// <summary>
    /// 执行错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 执行人
    /// </summary>
    public string ExecutedBy { get; set; } = "System";

    /// <summary>
    /// 执行标签（数据驱动数据行 / 浏览器标识，如 "chrome · 数据行 2/5"）
    /// </summary>
    public string? DataTag { get; set; }

    /// <summary>
    /// 执行环境配置快照
    /// </summary>
    public Dictionary<string, object> EnvSnapshot { get; set; } = new();
}

/// <summary>
/// 步骤执行记录
/// </summary>
public class StepExecutionRecord
{
    public string Id { get; set; } = string.Empty;

    public string StepId { get; set; } = string.Empty;

    public string StepName { get; set; } = string.Empty;

    public StepType StepType { get; set; }

    public int Order { get; set; }

    /// <summary>
    /// 步骤执行状态
    /// </summary>
    public StepExecutionStatus Status { get; set; } = StepExecutionStatus.Pending;

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public long Duration { get; set; }

    /// <summary>
    /// 执行参数（步骤类型相关：API 请求信息 / UI 操作参数 / 脚本类型等）
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// 请求数据
    /// </summary>
    public string? RequestData { get; set; }

    /// <summary>
    /// 响应数据
    /// </summary>
    public string? ResponseData { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 断言结果
    /// </summary>
    public List<AssertionResult> AssertionResults { get; set; } = new();

    /// <summary>
    /// 变量提取结果
    /// </summary>
    public Dictionary<string, string> ExtractedVariables { get; set; } = new();

    /// <summary>
    /// 执行日志
    /// </summary>
    public List<string> Logs { get; set; } = new();
}

/// <summary>
/// 断言结果
/// </summary>
public class AssertionResult
{
    public string Expression { get; set; } = string.Empty;

    public string Expected { get; set; } = string.Empty;

    public string Actual { get; set; } = string.Empty;

    public string Operator { get; set; } = string.Empty;

    public bool Passed { get; set; }

    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 执行状态
/// </summary>
public enum ExecutionStatus
{
    Pending = 0,
    Running = 1,
    Success = 2,
    Failed = 3,
    Cancelled = 4
}

/// <summary>
/// 步骤执行状态
/// </summary>
public enum StepExecutionStatus
{
    Pending = 1,
    Running = 2,
    Success = 3,
    Failed = 4,
    Skipped = 5
}

/// <summary>
/// 用例执行选项（数据驱动、多浏览器等）
/// </summary>
public class ExecutionOptions
{
    /// <summary>浏览器：chromium / chrome / msedge / firefox / webkit</summary>
    public string Browser { get; set; } = "chromium";

    /// <summary>是否无头模式执行</summary>
    public bool Headless { get; set; }

    /// <summary>数据驱动变量（覆盖同名环境变量）</summary>
    public Dictionary<string, string>? DataVariables { get; set; }

    /// <summary>执行标签（记录到执行记录，标识数据行/浏览器）</summary>
    public string? DataTag { get; set; }
}