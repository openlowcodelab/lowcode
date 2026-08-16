using System.ComponentModel.DataAnnotations;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试用例模型
/// </summary>
public class CaseDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "测试用例名称不能为空")]
    [StringLength(50, ErrorMessage = "测试用例名称长度不能超过50个字符")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "测试用例描述长度不能超过200个字符")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "所属项目不能为空")]
    public long ProjectId { get; set; }



    public long? CategoryId { get; set; }

    /// <summary>
    /// 是否为测试模板
    /// </summary>
    public bool IsTemplate { get; set; } = false;

    /// <summary>
    /// 关联的模板ID（如果是基于模板创建的用例）
    /// </summary>
    public long? TemplateId { get; set; }

    /// <summary>
    /// 用例级别（单选）
    /// </summary>
    public CaseLevel Level { get; set; } = CaseLevel.P1;

    /// <summary>
    /// 排序字段，数值越小排序越靠前
    /// </summary>
    public int Order { get; set; } = 0;

    public CaseStatus Status { get; set; } = CaseStatus.Active;

    /// <summary>
    /// 上一次执行结果
    /// </summary>
    public ExecutionStatus? LastExecutionResult { get; set; }

    /// <summary>
    /// 上一次执行时间
    /// </summary>
    public DateTime? LastExecutionTime { get; set; }
}

/// <summary>
/// 测试用例状态
/// </summary>
public enum CaseStatus
{
    Active = 1,
    Inactive = 2,
    Archived = 3,
    Draft = 4
}

/// <summary>
/// 用例级别
/// </summary>
public enum CaseLevel
{
    P0 = 1,
    P1 = 2,
    P2 = 3,
    P3 = 4
}
