namespace H.Approval.Web;

/// <summary>
/// 审批人类型枚举
/// </summary>
public enum ApproverTypeEnum
{
    /// <summary>
    /// 指定成员
    /// </summary>
    Specified = 0,
    
    /// <summary>
    /// 发起人自选
    /// </summary>
    StarterSelect = 1,
    
    /// <summary>
    /// 发起人自己
    /// </summary>
    StarterSelf = 2,
    
    /// <summary>
    /// 指定角色
    /// </summary>
    Role = 3,
    
    /// <summary>
    /// 部门主管
    /// </summary>
    DepartmentManager = 4
}
