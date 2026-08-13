using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Testing.EntityFrameworkCore;

/// <summary>
/// 项目环境
/// </summary>
public class ProjectEnvEntity : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属项目ID</summary>
    public long ProjectId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>环境类型</summary>
    public int Type { get; set; }

    /// <summary>环境变量（Dictionary 序列化）</summary>
    public string? VariablesJson { get; set; }

    /// <summary>请求头（Dictionary 序列化）</summary>
    public string? HeadersJson { get; set; }

    /// <summary>环境服务配置（Dictionary&lt;项目服务ID, BaseUrl&gt; 序列化）</summary>
    public string? ServiceConfigsJson { get; set; }
}
