using System;
using System.Collections.Generic;
using H.Approval.Application.Contracts;
using Volo.Abp.Domain.Entities;

namespace H.Approval.EntityFrameworkCore.Entities;

/// <summary>
/// 审批实例实体
/// </summary>
public class ApprovalInstance : Entity<string>
{
    protected ApprovalInstance()
    {
    }
    
    public ApprovalInstance(string id)
    {
        Id = id;
    }
    /// <summary>
    /// 审批定义ID
    /// </summary>
    public virtual string DefinitionId { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批定义名称
    /// </summary>
    public virtual string DefinitionName { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批状态
    /// </summary>
    public virtual ApprovalStatusEnum Status { get; set; }
    
    /// <summary>
    /// 发起人ID
    /// </summary>
    public virtual string CreatorId { get; set; } = string.Empty;
    
    /// <summary>
    /// 发起人姓名
    /// </summary>
    public virtual string CreatorName { get; set; } = string.Empty;
    
    /// <summary>
    /// 当前节点名称
    /// </summary>
    public virtual string? CurrentNodeName { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime CreationTime { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>
    public virtual DateTime? CompletionTime { get; set; }
    
    /// <summary>
    /// 审批任务列表
    /// </summary>
    public virtual ICollection<ApprovalTask> Tasks { get; set; } = new List<ApprovalTask>();
}
