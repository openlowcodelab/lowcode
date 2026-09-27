namespace H.Workbench.Application.Contracts;

/// <summary>
/// 审批裁决回传入参（SSE 执行期间前端点批准/拒绝时调用）
/// </summary>
public class ResumeApprovalInputDto
{
    public Guid ApprovalId { get; set; }
    public bool Approved { get; set; }
    public string? Reason { get; set; }

    /// <summary>批准的同时授予"本次运行内该工具免批"（只在当次执行内有效，不落库）</summary>
    public bool GrantForRun { get; set; }
}

/// <summary>
/// 审批裁决结果
/// </summary>
public class ApprovalOutcomeDto
{
    public Guid ApprovalId { get; set; }
    /// <summary>Approved/DeniedByUser/Unknown（过期或已裁决）</summary>
    public string Decision { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
