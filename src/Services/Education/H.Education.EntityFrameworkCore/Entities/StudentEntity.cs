using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Education.EntityFrameworkCore;

/// <summary>
/// 学生档案实体
/// </summary>
public class StudentEntity : AuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>学号，可留空；填写后全表唯一</summary>
    public string? StudentNo { get; set; }

    /// <summary>姓名</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>性别（0 未知、1 男、2 女）</summary>
    public int Gender { get; set; }

    /// <summary>年级，如 "四年级"</summary>
    public string Grade { get; set; } = string.Empty;

    /// <summary>班级，如 "三班"</summary>
    public string? ClassName { get; set; }

    /// <summary>家长姓名</summary>
    public string? GuardianName { get; set; }

    /// <summary>家长联系电话</summary>
    public string? GuardianPhone { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}
