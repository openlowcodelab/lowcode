using System.ComponentModel.DataAnnotations;
using H.Abp.Application.Contracts;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 技能 DTO
/// </summary>
public class SkillDto : FullAuditedEntityDto<Guid>
{
    public string SkillName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SkillType { get; set; } = string.Empty;
    public string? ImplementationClass { get; set; }
    public string? Config { get; set; }
    public string? ParameterSchema { get; set; }
    public bool IsEnabled { get; set; }
    public bool RequiresApproval { get; set; }
    public int UsageCount { get; set; }
    public DateTime? LastUsedTime { get; set; }
}

/// <summary>
/// 创建技能输入 DTO
/// </summary>
public class CreateSkillDefinitionDto
{
    [Required]
    [StringLength(100)]
    public string SkillName { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string SkillType { get; set; } = "Function";

    [StringLength(500)]
    public string? ImplementationClass { get; set; }

    public string? Config { get; set; }
    public string? ParameterSchema { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool RequiresApproval { get; set; }
}

/// <summary>
/// 更新技能输入 DTO
/// </summary>
public class UpdateSkillDefinitionDto
{
    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ImplementationClass { get; set; }

    public string? Config { get; set; }
    public string? ParameterSchema { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool RequiresApproval { get; set; }
}

/// <summary>
/// 技能查询 DTO
/// </summary>
public class SkillDefinitionQueryDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? SkillType { get; set; }
    public bool? IsEnabled { get; set; }
}
