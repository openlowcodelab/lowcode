using Volo.Abp.Domain.Entities.Auditing;

namespace H.Workbench.EntityFrameworkCore;

/// <summary>
/// 项目实体（工作资料：任务与员工的归属上下文）
/// </summary>
public class ProjectEntity : AuditedEntity<Guid>
{
    /// <summary>
    /// 项目名称
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// 项目描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 代码仓库地址（HTTPS），员工绑定项目后 git 工具只能操作清单内的仓库
    /// </summary>
    public string? RepoUrl { get; set; }

    /// <summary>
    /// 默认分支
    /// </summary>
    public string? DefaultBranch { get; set; }
}
