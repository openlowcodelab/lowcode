using System.ComponentModel.DataAnnotations;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试用例分类
/// </summary>
public class CaseCategoryDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "分类名称不能为空")]
    public string Name { get; set; } = string.Empty;

    public long ProjectId { get; set; }

    public long? ParentId { get; set; }

    public int Order { get; set; } = 0;

    public CaseCategoryDto[] Childrens { get; set; } = [];
}
