using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Testing.EntityFrameworkCore;

/// <summary>
/// 测试用例
/// </summary>
public class CaseEntity : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属项目ID</summary>
    public long ProjectId { get; set; }

    /// <summary>分类ID</summary>
    public long? CategoryId { get; set; }

    /// <summary>关联模板用例ID</summary>
    public long? TemplateId { get; set; }

    public string CaseName { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>是否为测试模板</summary>
    public bool IsTemplate { get; set; }

    /// <summary>用例级别（CaseLevel）</summary>
    public int Level { get; set; }

    /// <summary>排序</summary>
    public int Order { get; set; }

    /// <summary>用例状态</summary>
    public int Status { get; set; }

    /// <summary>上一次执行结果</summary>
    public int? LastExecutionResult { get; set; }

    /// <summary>上一次执行时间</summary>
    public DateTime? LastExecutionTime { get; set; }

    /// <summary>关联的数据集ID列表（JSON 数组，数据驱动执行时使用）</summary>
    public string? DatasetIdsJson { get; set; }
}

/// <summary>
/// 测试用例分类（树形，自引用 ParentId）
/// </summary>
public class CaseCategoryEntity : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属项目ID</summary>
    public long ProjectId { get; set; }

    /// <summary>父分类ID（根为 null）</summary>
    public long? ParentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }
}
