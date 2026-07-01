using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace H.Approval.EntityFrameworkCore;

/// <summary>
/// 审批定义实体
/// </summary>
public class ApprovalDefinition : Entity<string>, IMultiTenant
{
    protected ApprovalDefinition()
    {
    }
    
    public ApprovalDefinition(string id)
    {
        Id = id;
    }

    /// <summary>
    /// 审批名称
    /// </summary>
    public virtual string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批描述
    /// </summary>
    public virtual string? Description { get; set; }
    
    /// <summary>
    /// 审批版本
    /// </summary>
    public virtual int Version { get; set; } = 1;
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public virtual bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// 审批定义JSON
    /// </summary>
    public virtual string DefinitionJson { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批图标(emoji)
    /// </summary>
    public virtual string? Icon { get; set; }
    
    /// <summary>
    /// 所在分组ID
    /// </summary>
    public virtual string? CategoryId { get; set; }
    
    /// <summary>
    /// 所在分组名称
    /// </summary>
    public virtual string? CategoryName { get; set; }
    
    /// <summary>
    /// 谁可以发起: All-全部, Specified-指定成员
    /// </summary>
    public virtual string WhoCanStart { get; set; } = "All";
    
    /// <summary>
    /// 指定发起人员工ID列表(JSON数组)
    /// </summary>
    public virtual string? SpecifiedStarters { get; set; }
    
    /// <summary>
    /// 表单管理员类型: All-全部OA审批管理员, Specified-指定管理员
    /// </summary>
    public virtual string AdminType { get; set; } = "All";
    
    /// <summary>
    /// 指定管理员ID列表(JSON数组)
    /// </summary>
    public virtual string? SpecifiedAdmins { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime CreationTime { get; set; }
    
    /// <summary>
    /// 最后修改时间
    /// </summary>
    public virtual DateTime? LastModificationTime { get; set; }
    
    /// <summary>
    /// 租户 ID（多租户）
    /// </summary>
    public virtual Guid? TenantId { get; set; }
}
