using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Testing.EntityFrameworkCore;

/// <summary>
/// 测试数据集（数据驱动测试的参数化数据）
/// </summary>
public class TestDatasetEntity : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属项目ID</summary>
    public long ProjectId { get; set; }

    /// <summary>数据集名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>数据列名（JSON 数组，保持顺序）</summary>
    public string? ColumnsJson { get; set; }

    /// <summary>数据行（List&lt;Dictionary&lt;string,string&gt;&gt; 序列化）</summary>
    public string? RowsJson { get; set; }
}

/// <summary>
/// CI 触发的一次执行运行
/// </summary>
public class CiRunEntity : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属项目ID</summary>
    public long ProjectId { get; set; }

    /// <summary>执行环境ID</summary>
    public long EnvId { get; set; }

    /// <summary>用例ID列表（JSON 数组；null 表示全部用例）</summary>
    public string? CaseIdsJson { get; set; }

    /// <summary>浏览器列表（JSON 数组；null 表示 chromium）</summary>
    public string? BrowsersJson { get; set; }

    /// <summary>是否无头模式</summary>
    public bool Headless { get; set; }

    /// <summary>完成后的回调地址</summary>
    public string? WebhookUrl { get; set; }

    /// <summary>运行状态（对应 ExecutionStatus）</summary>
    public int Status { get; set; }

    public int TotalCases { get; set; }

    public int SuccessCases { get; set; }

    public int FailedCases { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? ErrorMessage { get; set; }
}
