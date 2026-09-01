namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试报告
/// </summary>
public class TestReportDto
{
    public long ProjectId { get; set; }

    /// <summary>统计天数范围</summary>
    public int Days { get; set; }

    /// <summary>总体统计</summary>
    public ExecutionStatistics Overview { get; set; } = new();

    /// <summary>每日执行趋势（按日期升序）</summary>
    public List<DailyExecutionStat> DailyTrend { get; set; } = new();

    /// <summary>失败次数最多的用例（降序）</summary>
    public List<CaseFailureStat> TopFailedCases { get; set; } = new();

    /// <summary>按环境统计</summary>
    public List<EnvExecutionStat> EnvironmentStats { get; set; } = new();
}

/// <summary>
/// 每日执行统计
/// </summary>
public class DailyExecutionStat
{
    /// <summary>日期（yyyy-MM-dd）</summary>
    public string Date { get; set; } = string.Empty;

    public int Total { get; set; }

    public int Success { get; set; }

    public int Failed { get; set; }

    public int Cancelled { get; set; }

    /// <summary>平均耗时（毫秒）</summary>
    public double AverageDuration { get; set; }
}

/// <summary>
/// 用例失败统计
/// </summary>
public class CaseFailureStat
{
    public long CaseId { get; set; }

    public string CaseName { get; set; } = string.Empty;

    /// <summary>执行总次数</summary>
    public int Total { get; set; }

    /// <summary>失败次数</summary>
    public int Failed { get; set; }

    /// <summary>成功率（百分比）</summary>
    public double SuccessRate { get; set; }

    /// <summary>最近一次执行时间</summary>
    public DateTime LastExecutionTime { get; set; }

    /// <summary>最近一次失败的错误信息</summary>
    public string? LastErrorMessage { get; set; }
}

/// <summary>
/// 环境执行统计
/// </summary>
public class EnvExecutionStat
{
    public long EnvId { get; set; }

    public string EnvName { get; set; } = string.Empty;

    public int Total { get; set; }

    public int Success { get; set; }

    public int Failed { get; set; }

    /// <summary>成功率（百分比）</summary>
    public double SuccessRate { get; set; }
}
