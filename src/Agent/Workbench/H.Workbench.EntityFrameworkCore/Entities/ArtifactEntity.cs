using Volo.Abp.Domain.Entities.Auditing;

namespace H.Workbench.EntityFrameworkCore;

/// <summary>
/// 任务执行产物实体：git 提交/推送、克隆、工作区文件变更、shell 命令等可验收成果的结构化记录，
/// 由执行轨迹事件流派生（工具返回值 JSON 解析），同文件多次写产生多行不合并
/// </summary>
public class ArtifactEntity : CreationAuditedEntity<Guid>
{
    public Guid TaskLogId { get; set; }

    public Guid TaskId { get; set; }

    /// <summary>
    /// 产物类型：GitCommit/GitPush/Clone/FileChange/ShellCommand
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// 人读摘要，如"提交 3 个文件到 feature/x"
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 工作目录名（派生名，已脱敏）
    /// </summary>
    public string? Repo { get; set; }

    /// <summary>
    /// 掩码后的远端地址（best-effort 反查项目资源）
    /// </summary>
    public string? RepoUrl { get; set; }

    public string? Branch { get; set; }

    public string? CommitHash { get; set; }

    /// <summary>
    /// 推送结果：Pushed/NotPushed/PushFailed
    /// </summary>
    public string? PushResult { get; set; }

    /// <summary>
    /// 仓库内相对路径（FileChange 类产物）
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// 变更类型：Created/Modified
    /// </summary>
    public string? ChangeType { get; set; }

    public bool Success { get; set; }

    /// <summary>
    /// 溯源到 TaskExecutionStep
    /// </summary>
    public string? ToolCallId { get; set; }

    public int Iteration { get; set; }

    /// <summary>
    /// 附加信息 JSON：exitCode/command/truncated/bytes/changes/durationMs 等
    /// </summary>
    public string? Payload { get; set; }
}
