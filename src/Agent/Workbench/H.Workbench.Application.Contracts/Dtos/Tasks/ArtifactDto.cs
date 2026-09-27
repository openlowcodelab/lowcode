namespace H.Workbench.Application.Contracts;

/// <summary>
/// 任务执行产物 DTO：git 提交/文件变更/shell 执行等可验收成果
/// </summary>
public class ArtifactDto
{
    public Guid Id { get; set; }
    public Guid TaskLogId { get; set; }
    public Guid TaskId { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Repo { get; set; }
    public string? RepoUrl { get; set; }
    public string? Branch { get; set; }
    public string? CommitHash { get; set; }
    public string? PushResult { get; set; }
    public string? FilePath { get; set; }
    public string? ChangeType { get; set; }
    public bool Success { get; set; }
    public string? ToolCallId { get; set; }
    public int Iteration { get; set; }
    public string? Payload { get; set; }
    public DateTime CreationTime { get; set; }

    /// <summary>人工验收状态：Pending/Accepted/Rejected</summary>
    public string ReviewStatus { get; set; } = "Pending";
    public string? ReviewNote { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

/// <summary>
/// 人工验收入参：一次执行的全部产物作为一个整体验收
/// （同一批提交/文件变更通常同源，逐条点验收只会让人懒得点）
/// </summary>
public class ReviewArtifactsInputDto
{
    public Guid TaskLogId { get; set; }

    /// <summary>Accepted（认可）| Rejected（不认可）| Pending（退回待验收）</summary>
    public string Status { get; set; } = "Accepted";

    public string? Note { get; set; }
}
