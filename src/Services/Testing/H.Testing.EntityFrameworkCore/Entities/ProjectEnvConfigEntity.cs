using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Testing.EntityFrameworkCore;

/// <summary>
/// 环境服务配置（环境 + 服务 的 BaseUrl 绑定）
/// </summary>
public class ProjectEnvConfigEntity : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>环境ID</summary>
    public long EnvId { get; set; }

    /// <summary>项目服务ID</summary>
    public long ProjectServiceId { get; set; }

    /// <summary>服务基础URL</summary>
    public string? BaseUrl { get; set; }
}
