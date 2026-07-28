using System.ComponentModel.DataAnnotations;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 创建记忆条目 DTO（供 AI 提取记忆使用）
/// </summary>
public class CreateMemoryEntryDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Category { get; set; }
}
