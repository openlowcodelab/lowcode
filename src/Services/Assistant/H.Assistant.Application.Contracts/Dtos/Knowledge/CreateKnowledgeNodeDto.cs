using System.ComponentModel.DataAnnotations;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 创建知识库节点 DTO
/// </summary>
public class CreateKnowledgeNodeDto
{
    public Guid? ParentId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string NodeType { get; set; } = "Directory";

    public int SortOrder { get; set; }
}
