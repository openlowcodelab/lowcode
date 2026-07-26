namespace H.Notification.Application.Contracts;

/// <summary>
/// 通知规则DTO（某业务在某级别下的渠道与阈值告警配置）
/// </summary>
public class NotificationSpecDto
{
    public Guid Id { get; set; }

    /// <summary>
    /// 通知级别
    /// </summary>
    public NotificationLevel Level { get; set; }

    /// <summary>
    /// 是否启用该级别
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 该级别触发的渠道集合
    /// </summary>
    public List<NotificationChannelType> Channels { get; set; } = new();

    /// <summary>
    /// 连续周期数
    /// </summary>
    public int ConsecutivePeriods { get; set; } = 1;

    /// <summary>
    /// 单周期时长（分钟）
    /// </summary>
    public int PeriodMinutes { get; set; } = 1;

    /// <summary>
    /// 聚合方式
    /// </summary>
    public AggregationType Aggregation { get; set; } = AggregationType.Average;

    /// <summary>
    /// 比较运算符
    /// </summary>
    public ComparisonOperator Comparison { get; set; } = ComparisonOperator.GreaterOrEqual;

    /// <summary>
    /// 阈值
    /// </summary>
    public decimal? Threshold { get; set; }
}
