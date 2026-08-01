using H.Notification.Application.Contracts;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Notification.EntityFrameworkCore;

/// <summary>
/// 通知业务实体（事件类型）
/// </summary>
public class NotificationBusinessEntity : AuditedEntity<Guid>, IMultiTenant
{
    public NotificationBusinessEntity()
    {
    }

    public NotificationBusinessEntity(Guid id) : base(id)
    {
    }

    public virtual Guid? TenantId { get; set; }

    /// <summary>
    /// 所属分类ID
    /// </summary>
    public long CategoryId { get; set; }

    /// <summary>
    /// 业务名称
    /// </summary>
    public string BusinessName { get; set; } = string.Empty;

    /// <summary>
    /// 完整业务编码（分类Id-编码后缀）
    /// </summary>
    public string BusinessCode { get; set; } = string.Empty;

    /// <summary>
    /// 业务描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 默认通知级别
    /// </summary>
    public NotificationLevel DefaultLevel { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 通知规则（各级别渠道与阈值配置）
    /// </summary>
    public virtual ICollection<NotificationSpecEntity> Specs { get; set; } = new List<NotificationSpecEntity>();

    /// <summary>
    /// 通知模板
    /// </summary>
    public virtual ICollection<NotificationTemplateEntity> Templates { get; set; } = new List<NotificationTemplateEntity>();

    /// <summary>
    /// 绑定的联系人组
    /// </summary>
    public virtual ICollection<NotificationBusinessGroupEntity> Groups { get; set; } = new List<NotificationBusinessGroupEntity>();
}

/// <summary>
/// 通知规则实体（某业务某级别的渠道与阈值告警配置）
/// </summary>
public class NotificationSpecEntity : Entity<Guid>, IMultiTenant
{
    public NotificationSpecEntity()
    {
    }

    public NotificationSpecEntity(Guid id) : base(id)
    {
    }

    public virtual Guid? TenantId { get; set; }

    public Guid BusinessId { get; set; }

    /// <summary>
    /// 通知级别
    /// </summary>
    public NotificationLevel Level { get; set; }

    /// <summary>
    /// 是否启用该级别
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 该级别触发的渠道（逗号分隔的渠道类型值）
    /// </summary>
    public string? Channels { get; set; }

    /// <summary>
    /// 连续周期数
    /// </summary>
    public int ConsecutivePeriods { get; set; }

    /// <summary>
    /// 单周期时长（分钟）
    /// </summary>
    public int PeriodMinutes { get; set; }

    /// <summary>
    /// 聚合方式
    /// </summary>
    public AggregationType Aggregation { get; set; }

    /// <summary>
    /// 比较运算符
    /// </summary>
    public ComparisonOperator Comparison { get; set; }

    /// <summary>
    /// 阈值
    /// </summary>
    public decimal? Threshold { get; set; }

    public virtual NotificationBusinessEntity? Business { get; set; }
}

/// <summary>
/// 通知模板实体
/// </summary>
public class NotificationTemplateEntity : AuditedEntity<Guid>, IMultiTenant
{
    public NotificationTemplateEntity()
    {
    }

    public NotificationTemplateEntity(Guid id) : base(id)
    {
    }

    public virtual Guid? TenantId { get; set; }

    public Guid BusinessId { get; set; }

    public NotificationChannelType ChannelType { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public bool IsEnabled { get; set; }

    public virtual NotificationBusinessEntity? Business { get; set; }
}

/// <summary>
/// 业务-联系人组绑定实体
/// </summary>
public class NotificationBusinessGroupEntity : Entity<Guid>, IMultiTenant
{
    public NotificationBusinessGroupEntity()
    {
    }

    public NotificationBusinessGroupEntity(Guid id) : base(id)
    {
    }

    public virtual Guid? TenantId { get; set; }

    public Guid BusinessId { get; set; }

    public long GroupId { get; set; }

    public virtual NotificationBusinessEntity? Business { get; set; }
}
