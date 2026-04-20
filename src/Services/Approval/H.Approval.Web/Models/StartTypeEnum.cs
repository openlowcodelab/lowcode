namespace H.Approval.Web;

/// <summary>
/// 发起人类型枚举
/// </summary>
public enum StartTypeEnum
{
    /// <summary>
    /// 全员可发起
    /// </summary>
    All = 0,
    
    /// <summary>
    /// 指定成员可发起
    /// </summary>
    Specified = 1,
    
    /// <summary>
    /// 指定角色可发起
    /// </summary>
    Role = 2
}
