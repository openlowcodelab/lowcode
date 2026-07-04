namespace H.Enterprise.EntityFrameworkCore.Entities;

/// <summary>
/// 企业实体（对应多租户系统中的租户）
/// </summary>
public class EnterpriseEntity
{
    /// <summary>
    /// 企业 ID（同时作为 TenantId）
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 企业名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 企业编码
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 企业描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Logo URL
    /// </summary>
    public string? Logo { get; set; }

    /// <summary>
    /// 联系人姓名
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// 联系邮箱
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// 企业状态
    /// </summary>
    public EnterpriseStatus Status { get; set; } = EnterpriseStatus.Pending;

    /// <summary>
    /// 数据库模式（激活后不可更改）
    /// </summary>
    public DatabaseMode DatabaseMode { get; set; } = DatabaseMode.Shared;

    /// <summary>
    /// 独立数据库连接字符串（仅 Independent 模式）
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// 是否已激活
    /// </summary>
    public bool IsActivated { get; set; }

    /// <summary>
    /// 激活时间
    /// </summary>
    public DateTime? ActivatedAt { get; set; }

    /// <summary>
    /// 激活操作人 ID
    /// </summary>
    public Guid? ActivatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 创建者 ID
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// 更新者 ID
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 关联企业用户列表
    /// </summary>
    public virtual ICollection<EnterpriseUserEntity>? EnterpriseUsers { get; set; }
}
