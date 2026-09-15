using Volo.Abp.Domain.Entities.Auditing;

namespace H.Assistant.EntityFrameworkCore;

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
}
