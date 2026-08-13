using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Testing.EntityFrameworkCore;

/// <summary>
/// 测试用例步骤
/// </summary>
public class CaseStepEntity : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属测试用例ID</summary>
    public long CaseId { get; set; }

    /// <summary>步骤业务标识（字符串 GUID，与前端及执行记录中的 StepId 对应）</summary>
    public string StepKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>步骤类型（StepType）</summary>
    public int Type { get; set; }

    /// <summary>排序</summary>
    public int Order { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>步骤参数（Dictionary 序列化）</summary>
    public string? ParametersJson { get; set; }

    public string? ExpectedResult { get; set; }

    /// <summary>API 步骤配置（ApiStepConfig 序列化）</summary>
    public string? ApiConfigJson { get; set; }

    /// <summary>UI 步骤配置（UiStepConfig 序列化）</summary>
    public string? UiConfigJson { get; set; }

    /// <summary>脚本步骤配置（ScriptStepConfig 序列化）</summary>
    public string? ScriptConfigJson { get; set; }
}
