using System;

namespace H.Approval.Application.Contracts;

/// <summary>
/// 审批定义 DTO
/// </summary>
public class ApprovalDefinitionDto
{
    /// <summary>
    /// 审批定义 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 审批版本
    /// </summary>
    public int Version { get; set; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// 审批定义 JSON
    /// </summary>
    public string DefinitionJson { get; set; } = string.Empty;
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; }
    
    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime? LastModificationTime { get; set; }
}

/// <summary>
/// 创建审批定义输入 DTO
/// </summary>
public class CreateApprovalDefinitionDto
{
    /// <summary>
    /// 审批名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 审批定义 JSON
    /// </summary>
    public string DefinitionJson { get; set; } = string.Empty;
}

/// <summary>
/// 更新审批定义输入 DTO
/// </summary>
public class UpdateApprovalDefinitionDto
{
    /// <summary>
    /// 审批定义 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 审批定义 JSON
    /// </summary>
    public string DefinitionJson { get; set; } = string.Empty;
}
