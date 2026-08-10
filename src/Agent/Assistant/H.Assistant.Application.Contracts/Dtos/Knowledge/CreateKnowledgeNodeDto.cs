using System.ComponentModel.DataAnnotations;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 创建知识库节点 DTO
/// </summary>
public class CreateKnowledgeNodeDto
{
    public Guid? ParentId { get; set; }

    /// <summary> 所属知识库 ID（根节点必传，子节点继承父节点） </summary>
    public Guid? KnowledgeBaseId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string NodeType { get; set; } = "Directory";

    public int SortOrder { get; set; }
}
