namespace H.Approval.Web;

/// <summary>
/// 多人审批方式枚举
/// </summary>
public enum ApproverModeEnum
{
    /// <summary>
    /// 依次审批(按顺序)
    /// </summary>
    Sequential = 0,
    
    /// <summary>
    /// 会签(需要所有人都同意)
    /// </summary>
    CounterSign = 1,
    
    /// <summary>
    /// 或签(其中一人同意即可)
    /// </summary>
    OrSign = 2
}
