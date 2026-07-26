using System;

namespace H.Approval.Application.Contracts;

/// <summary>
/// 审批实例 DTO
/// </summary>
public class ApprovalInstanceDto
{
    /// <summary>
    /// 审批实例 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批定义 ID
    /// </summary>
    public string DefinitionId { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批定义名称
    /// </summary>
    public string DefinitionName { get; set; } = string.Empty;

    /// <summary>
    /// 审批实例标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 审批状态
    /// </summary>
    public ApprovalStatusEnum Status { get; set; }

    /// <summary>
    /// 发起人 ID
    /// </summary>
    public string CreatorId { get; set; } = string.Empty;

    /// <summary>
    /// 发起人姓名
    /// </summary>
    public string CreatorName { get; set; } = string.Empty;

    /// <summary>
    /// 当前节点ID
    /// </summary>
    public string? CurrentNodeId { get; set; }

    /// <summary>
    /// 当前节点名称
    /// </summary>
    public string? CurrentNodeName { get; set; }

    /// <summary>
    /// 审批变量JSON(用于条件分支求值)
    /// </summary>
    public string? VariablesJson { get; set; }

    /// <summary>
    /// 当前审批任务列表
    /// </summary>
    public List<ApprovalTaskDto> Tasks { get; set; } = new();
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? CompletionTime { get; set; }
}

/// <summary>
/// 启动审批实例输入 DTO
/// </summary>
public class StartApprovalInstanceDto
{
    /// <summary>
    /// 审批定义 ID
    /// </summary>
    public string DefinitionId { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批实例标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 审批变量 (JSON 格式)
    /// </summary>
    public string? VariablesJson { get; set; }
}
