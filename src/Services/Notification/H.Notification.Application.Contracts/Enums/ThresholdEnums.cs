namespace H.Notification.Application.Contracts;

/// <summary>
/// 聚合方式（阈值告警用）
/// </summary>
public enum AggregationType
{
    /// <summary>
    /// 平均值
    /// </summary>
    Average = 0,

    /// <summary>
    /// 最大值
    /// </summary>
    Max = 1,

    /// <summary>
    /// 最小值
    /// </summary>
    Min = 2,

    /// <summary>
    /// 求和
    /// </summary>
    Sum = 3
}

/// <summary>
/// 比较运算符（阈值告警用）
/// </summary>
public enum ComparisonOperator
{
    /// <summary>
    /// 大于等于
    /// </summary>
    GreaterOrEqual = 0,

    /// <summary>
    /// 大于
    /// </summary>
    Greater = 1,

    /// <summary>
    /// 小于等于
    /// </summary>
    LessOrEqual = 2,

    /// <summary>
    /// 小于
    /// </summary>
    Less = 3,

    /// <summary>
    /// 等于
    /// </summary>
    Equal = 4
}
