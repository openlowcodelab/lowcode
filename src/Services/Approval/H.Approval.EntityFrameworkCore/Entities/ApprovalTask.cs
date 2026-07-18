using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace H.Approval.EntityFrameworkCore;

/// <summary>
/// 审批任务实体
/// </summary>
public class ApprovalTask : Entity<string>, IMultiTenant
{
    protected ApprovalTask()
    {
    }

    public ApprovalTask(string id)
    {
        Id = id;
    }

    /// <summary>租户ID（多租户）</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>审批实例ID</summary>
    public virtual string InstanceId { get; set; } = string.Empty;

    /// <summary>审批定义名称</summary>
    public virtual string ApprovalName { get; set; } = string.Empty;

    /// <summary>审批实例标题</summary>
    public virtual string InstanceTitle { get; set; } = string.Empty;

    /// <summary>工作流节点ID(用于会签/或签分组)</summary>
    public virtual string NodeId { get; set; } = string.Empty;

    /// <summary>当前节点名称</summary>
    public virtual string NodeName { get; set; } = string.Empty;

    /// <summary>审批人ID</summary>
    public virtual string AssigneeId { get; set; } = string.Empty;

    /// <summary>审批人姓名</summary>
    public virtual string AssigneeName { get; set; } = string.Empty;

    /// <summary>任务状态: 0-待审批, 1-已通过, 2-已驳回, 3-已取消</summary>
    public virtual int Status { get; set; }

    /// <summary>创建时间</summary>
    public virtual DateTime CreationTime { get; set; }

    /// <summary>审批时间</summary>
    public virtual DateTime? ApprovalTime { get; set; }

    /// <summary>审批意见</summary>
    public virtual string? Comment { get; set; }
}
