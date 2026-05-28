using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// Agent 定义 DTO
/// </summary>
public class AgentDefinitionDto : FullAuditedEntityDto<Guid>
{
    public string AgentType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool SupportsStreaming { get; set; }
    public float Temperature { get; set; }
    public int MaxTokens { get; set; }
    public Guid? DefaultModelConfigId { get; set; }
    public string? Metadata { get; set; }
    public List<string> Skills { get; set; } = new();
}

/// <summary>
/// 创建 Agent 输入 DTO
/// </summary>
public class CreateAgentDefinitionDto
{
    [Required]
    [StringLength(100)]
    public string AgentType { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(4000)]
    public string SystemPrompt { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;
    public bool SupportsStreaming { get; set; } = true;
    
    [Range(0, 1)]
    public float Temperature { get; set; } = 0.7f;
    
    public int MaxTokens { get; set; } = 2000;
    public Guid? DefaultModelConfigId { get; set; }
    public string? Metadata { get; set; }
    public List<Guid> SkillIds { get; set; } = new();
}

/// <summary>
/// 更新 Agent 输入 DTO
/// </summary>
public class UpdateAgentDefinitionDto
{
    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(4000)]
    public string SystemPrompt { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;
    public bool SupportsStreaming { get; set; } = true;
    
    [Range(0, 1)]
    public float Temperature { get; set; } = 0.7f;
    
    public int MaxTokens { get; set; } = 2000;
    public Guid? DefaultModelConfigId { get; set; }
    public string? Metadata { get; set; }
    public List<Guid> SkillIds { get; set; } = new();
}

/// <summary>
/// Agent 查询 DTO
/// </summary>
public class AgentDefinitionQueryDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsEnabled { get; set; }
}
