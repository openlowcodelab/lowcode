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

    /// <summary>用例编号（业务字符串）</summary>
    public string? CaseNumber { get; set; }

    public string CaseName { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>是否为测试模板</summary>
    public bool IsTemplate { get; set; }

    /// <summary>用例级别（List&lt;string&gt; 序列化）</summary>
    public string? LevelsJson { get; set; }

    /// <summary>标签（List&lt;string&gt; 序列化）</summary>
    public string? TagsJson { get; set; }

    /// <summary>步骤（List&lt;ProjectCaseStep&gt; 序列化）</summary>
    public string? StepsJson { get; set; }

    /// <summary>测试数据（Dictionary 序列化）</summary>
    public string? TestDataJson { get; set; }

    /// <summary>排序</summary>
    public int Order { get; set; }

    /// <summary>用例状态</summary>
    public int Status { get; set; }

    /// <summary>上一次执行结果</summary>
    public int? LastExecutionResult { get; set; }

    /// <summary>上一次执行时间</summary>
    public DateTime? LastExecutionTime { get; set; }
}
