using Volo.Abp.Domain.Entities.Auditing;

namespace H.Workbench.EntityFrameworkCore;

/// <summary>
/// 项目资源实体：项目关联的工作资料（代码仓库、设计文件、文档、数据集等），
/// 员工绑定项目后按资源类型获得对应能力（如 code 类型注入 git 可操作仓库清单）
/// </summary>
public class ProjectResourceEntity : AuditedEntity<Guid>
{
    /// <summary>
    /// 所属项目
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// 资源类型：code / design / document / data / other
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// 资源名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 资源地址（仓库 URL、文件链接等）
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 类型相关的扩展配置（JSON），如代码仓库的 {"branch":"dev"}
    /// </summary>
    public string? Config { get; set; }
}
