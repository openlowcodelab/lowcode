using System.ComponentModel.DataAnnotations;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试计划
/// </summary>
public class TestPlanDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "所属项目不能为空")]
    public long ProjectId { get; set; }

    [Required(ErrorMessage = "计划名称不能为空")]
    [StringLength(50, ErrorMessage = "计划名称长度不能超过50个字符")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "计划描述长度不能超过200个字符")]
    public string Description { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public TestPlanStatus Status { get; set; } = TestPlanStatus.NotStarted;

    // 以下为列表统计字段（服务端计算）

    /// <summary>计划内用例总数</summary>
    public int TotalCases { get; set; }

    /// <summary>已通过用例数</summary>
    public int PassedCases { get; set; }

    /// <summary>失败用例数</summary>
    public int FailedCases { get; set; }

    /// <summary>通过率（百分比）</summary>
    public double PassRate => TotalCases > 0 ? (double)PassedCases / TotalCases * 100 : 0;
}

/// <summary>
/// 测试计划详情
/// </summary>
public class TestPlanDetailDto
{
    public TestPlanDto Plan { get; set; } = new();

    /// <summary>计划内用例列表</summary>
    public List<PlanCaseDto> Cases { get; set; } = new();
}

/// <summary>
/// 计划内用例
/// </summary>
public class PlanCaseDto
{
    public long Id { get; set; }

    public long PlanId { get; set; }

    public long CaseId { get; set; }

    /// <summary>用例名称（服务端填充）</summary>
    public string CaseName { get; set; } = string.Empty;

    /// <summary>用例级别（服务端填充）</summary>
    public CaseLevel Level { get; set; }

    /// <summary>负责人</summary>
    public string Assignee { get; set; } = string.Empty;

    public PlanCaseStatus Status { get; set; } = PlanCaseStatus.NotStarted;

    public DateTime? LastExecutionTime { get; set; }
}

/// <summary>
/// 缺陷
/// </summary>
public class DefectDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "所属项目不能为空")]
    public long ProjectId { get; set; }

    [Required(ErrorMessage = "缺陷标题不能为空")]
    [StringLength(100, ErrorMessage = "缺陷标题长度不能超过100个字符")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "缺陷描述长度不能超过2000个字符")]
    public string Description { get; set; } = string.Empty;

    public DefectSeverity Severity { get; set; } = DefectSeverity.Major;

    public DefectStatus Status { get; set; } = DefectStatus.Open;

    /// <summary>关联用例ID</summary>
    public long? CaseId { get; set; }

    /// <summary>关联用例名称（服务端填充）</summary>
    public string? CaseName { get; set; }

    /// <summary>关联执行记录ID</summary>
    public long? RecordId { get; set; }

    /// <summary>负责人</summary>
    public string Assignee { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }
}
