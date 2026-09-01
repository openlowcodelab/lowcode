using System.ComponentModel.DataAnnotations;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 定时执行计划
/// </summary>
public class TestScheduleDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "所属项目不能为空")]
    public long ProjectId { get; set; }

    [Required(ErrorMessage = "计划名称不能为空")]
    [StringLength(50, ErrorMessage = "计划名称长度不能超过50个字符")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "执行环境不能为空")]
    public long EnvId { get; set; }

    /// <summary>用例范围：All（全部用例）/ Selected（指定用例）</summary>
    public string CaseScope { get; set; } = "All";

    /// <summary>CaseScope=Selected 时的用例ID列表</summary>
    public List<long> SelectedCaseIds { get; set; } = new();

    [Required(ErrorMessage = "Cron 表达式不能为空")]
    public string CronExpression { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public DateTime? LastExecutionTime { get; set; }

    /// <summary>最近一次执行状态（对应 ExecutionStatus）</summary>
    public int? LastExecutionStatus { get; set; }
}
