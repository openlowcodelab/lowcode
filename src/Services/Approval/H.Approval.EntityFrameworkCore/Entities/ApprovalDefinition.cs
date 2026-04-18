using System;
using Volo.Abp.Domain.Entities;

namespace H.Approval.EntityFrameworkCore.Entities;

/// <summary>
/// 审批定义实体
/// </summary>
public class ApprovalDefinition : Entity<string>
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
    /// 创建时间
    /// </summary>
    public virtual DateTime CreationTime { get; set; }
    
    /// <summary>
    /// 最后修改时间
    /// </summary>
    public virtual DateTime? LastModificationTime { get; set; }
}
