namespace H.Workbench.Application.Contracts;

/// <summary>
/// 待人工验收的产物条目：产物本身不足以判断，得能一眼看到它是哪次执行、哪个任务留下的
/// </summary>
public class PendingReviewItemDto
{
    public Guid Id { get; set; }
    public Guid TaskLogId { get; set; }
    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Repo { get; set; }
    public string? Branch { get; set; }
    public string? CommitHash { get; set; }
    public string? FilePath { get; set; }
    public bool Success { get; set; }

    /// <summary>所属执行的验收裁决：Pass/Fail/Unclear/null，Fail 的产物更要人看</summary>
    public string? Verdict { get; set; }

    public string LogStatus { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; }
}
