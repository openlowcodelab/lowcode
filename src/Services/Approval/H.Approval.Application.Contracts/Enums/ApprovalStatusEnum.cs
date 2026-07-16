namespace H.Approval.Application.Contracts;

/// <summary>
/// 工作流状态枚举
/// </summary>
public enum ApprovalStatusEnum
{
    /// <summary>
    /// 草稿
    /// </summary>
    Draft = 0,
    
    /// <summary>
    /// 运行中
    /// </summary>
    Running = 1,
    
    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 2,
    
    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// 已驳回
    /// </summary>
    Rejected = 4
}
