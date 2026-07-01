namespace H.Enterprise.EntityFrameworkCore.Entities;

/// <summary>
/// 企业状态枚举
/// </summary>
public enum EnterpriseStatus
{
    /// <summary>
    /// 待审核
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 已激活
    /// </summary>
    Active = 1,

    /// <summary>
    /// 已禁用
    /// </summary>
    Disabled = 2
}

/// <summary>
/// 数据库模式枚举
/// </summary>
public enum DatabaseMode
{
    /// <summary>
    /// 共享数据库（通过 TenantId 逻辑隔离）
    /// </summary>
    Shared = 0,

    /// <summary>
    /// 独立数据库（每个企业独立数据库实例）
    /// </summary>
    Independent = 1
}
