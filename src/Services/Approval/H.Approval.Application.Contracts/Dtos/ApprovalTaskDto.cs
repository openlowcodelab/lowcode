using System;

namespace H.Approval.Application.Contracts;

/// <summary>
/// 审批任务 DTO
/// </summary>
public class ApprovalTaskDto
{
    /// <summary>
    /// 任务 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批实例 ID
    /// </summary>
    public string InstanceId { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批定义名称
    /// </summary>
    public string ApprovalName { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批实例标题
    /// </summary>
    public string InstanceTitle { get; set; } = string.Empty;
    
    /// <summary>
    /// 当前节点名称
    /// </summary>
    public string NodeName { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批人 ID
    /// </summary>
    public string AssigneeId { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批人姓名
    /// </summary>
    public string AssigneeName { get; set; } = string.Empty;
    
    /// <summary>
    /// 任务状态: 0-待审批, 1-已通过, 2-已驳回
    /// </summary>
    public int Status { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; }
    
    /// <summary>
    /// 审批时间
    /// </summary>
    public DateTime? ApprovalTime { get; set; }
    
    /// <summary>
    /// 审批意见
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// 审批任务操作输入 DTO
/// </summary>
public class ApprovalTaskActionDto
{
    /// <summary>
    /// 任务 ID
    /// </summary>
    public string TaskId { get; set; } = string.Empty;
    
    /// <summary>
    /// 操作类型: 1-通过, 2-驳回
    /// </summary>
    public int Action { get; set; }
    
    /// <summary>
    /// 审批意见
    /// </summary>
    public string? Comment { get; set; }
}
