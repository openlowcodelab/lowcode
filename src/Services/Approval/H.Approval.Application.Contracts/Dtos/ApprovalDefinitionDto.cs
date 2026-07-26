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
    /// 表单设计 JSON(字段Schema)
    /// </summary>
    public string? FormJson { get; set; }
    
    /// <summary>
    /// 审批图标(emoji)
    /// </summary>
    public string? Icon { get; set; }
    
    /// <summary>
    /// 所在分组ID
    /// </summary>
    public string? CategoryId { get; set; }
    
    /// <summary>
    /// 所在分组名称
    /// </summary>
    public string? CategoryName { get; set; }
    
    /// <summary>
    /// 谁可以发起: All-全部, Specified-指定成员
    /// </summary>
    public string WhoCanStart { get; set; } = "All";
    
    /// <summary>
    /// 指定发起人员工ID列表(JSON数组)
    /// </summary>
    public string? SpecifiedStarters { get; set; }
    
    /// <summary>
    /// 表单管理员类型: All-全部OA审批管理员, Specified-指定管理员
    /// </summary>
    public string AdminType { get; set; } = "All";
    
    /// <summary>
    /// 指定管理员ID列表(JSON数组)
    /// </summary>
    public string? SpecifiedAdmins { get; set; }
    
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
    
    /// <summary>
    /// 表单设计 JSON(字段Schema)
    /// </summary>
    public string? FormJson { get; set; }
    
    /// <summary>
    /// 审批图标(emoji)
    /// </summary>
    public string? Icon { get; set; }
    
    /// <summary>
    /// 所在分组ID
    /// </summary>
    public string? CategoryId { get; set; }
    
    /// <summary>
    /// 所在分组名称
    /// </summary>
    public string? CategoryName { get; set; }
    
    /// <summary>
    /// 谁可以发起: All-全部, Specified-指定成员
    /// </summary>
    public string WhoCanStart { get; set; } = "All";
    
    /// <summary>
    /// 指定发起人员工ID列表(JSON数组)
    /// </summary>
    public string? SpecifiedStarters { get; set; }
    
    /// <summary>
    /// 表单管理员类型: All-全部OA审批管理员, Specified-指定管理员
    /// </summary>
    public string AdminType { get; set; } = "All";
    
    /// <summary>
    /// 指定管理员ID列表(JSON数组)
    /// </summary>
    public string? SpecifiedAdmins { get; set; }
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
    
    /// <summary>
    /// 表单设计 JSON(字段Schema)
    /// </summary>
    public string? FormJson { get; set; }
    
    /// <summary>
    /// 审批图标(emoji)
    /// </summary>
    public string? Icon { get; set; }
    
    /// <summary>
    /// 所在分组ID
    /// </summary>
    public string? CategoryId { get; set; }
    
    /// <summary>
    /// 所在分组名称
    /// </summary>
    public string? CategoryName { get; set; }
    
    /// <summary>
    /// 谁可以发起: All-全部, Specified-指定成员
    /// </summary>
    public string WhoCanStart { get; set; } = "All";
    
    /// <summary>
    /// 指定发起人员工ID列表(JSON数组)
    /// </summary>
    public string? SpecifiedStarters { get; set; }
    
    /// <summary>
    /// 表单管理员类型: All-全部OA审批管理员, Specified-指定管理员
    /// </summary>
    public string AdminType { get; set; } = "All";
    
    /// <summary>
    /// 指定管理员ID列表(JSON数组)
    /// </summary>
    public string? SpecifiedAdmins { get; set; }
}
