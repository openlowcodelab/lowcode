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
}
