using System.ComponentModel.DataAnnotations;

namespace H.Testing.Application.Contracts;

/// <summary>
/// CI 触发执行请求
/// </summary>
public class CiTriggerDto
{
    /// <summary>接入令牌（在"设置"页生成）</summary>
    [Required(ErrorMessage = "令牌不能为空")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "项目ID不能为空")]
    public long ProjectId { get; set; }

    [Required(ErrorMessage = "环境ID不能为空")]
    public long EnvId { get; set; }

    /// <summary>要执行的用例ID列表；为空表示执行项目全部用例</summary>
    public List<long>? CaseIds { get; set; }

    /// <summary>执行浏览器列表；为空使用 chromium</summary>
    public List<string>? Browsers { get; set; }

    /// <summary>是否无头模式（CI 环境建议开启）</summary>
    public bool Headless { get; set; } = true;

    /// <summary>执行完成后的回调地址（POST JSON 结果）</summary>
    public string? WebhookUrl { get; set; }
}

/// <summary>
/// CI 执行运行结果（轮询用）
/// </summary>
public class CiRunDto
{
    public long Id { get; set; }

    public long ProjectId { get; set; }

    public long EnvId { get; set; }

    /// <summary>运行状态（对应 ExecutionStatus：Pending/Running/Success/Failed）</summary>
    public ExecutionStatus Status { get; set; }

    public int TotalCases { get; set; }

    public int SuccessCases { get; set; }

    public int FailedCases { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? ErrorMessage { get; set; }

    /// <summary>各用例执行结果摘要</summary>
    public List<CiRunCaseResult> CaseResults { get; set; } = new();
}

/// <summary>
/// CI 运行中的单用例结果
/// </summary>
public class CiRunCaseResult
{
    public long CaseId { get; set; }

    public string CaseName { get; set; } = string.Empty;

    public ExecutionStatus Status { get; set; }

    public string? ErrorMessage { get; set; }

    /// <summary>执行标签（数据行/浏览器）</summary>
    public string? DataTag { get; set; }
}
