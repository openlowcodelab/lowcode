using System.ComponentModel.DataAnnotations;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试数据集（数据驱动测试的参数化数据）
/// </summary>
public class TestDatasetDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "所属项目不能为空")]
    public long ProjectId { get; set; }

    [Required(ErrorMessage = "数据集名称不能为空")]
    [StringLength(50, ErrorMessage = "数据集名称长度不能超过50个字符")]
    public string Name { get; set; } = string.Empty;

    /// <summary>数据行：每行为 列名→值 的字典，执行时注入为变量</summary>
    public List<Dictionary<string, string>> Rows { get; set; } = new();

    /// <summary>数据行数（列表接口不返回明细时也有值）</summary>
    public int RowCount { get; set; }

    /// <summary>数据列名（按首行顺序）</summary>
    public List<string> Columns { get; set; } = new();

    public DateTime CreationTime { get; set; }
}
