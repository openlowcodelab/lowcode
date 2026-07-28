using System.ComponentModel.DataAnnotations;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 更新知识库节点 DTO
/// </summary>
public class UpdateKnowledgeNodeDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    [StringLength(20)]
    public string? NodeType { get; set; }
}
