using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 测试报告服务：基于执行记录的聚合统计
/// </summary>
public class TestReportAppService : ApplicationService, ITestReportAppService
{
    private readonly IRepository<CaseRecordEntity, long> _repository;

    public TestReportAppService(IRepository<CaseRecordEntity, long> repository)
    {
        _repository = repository;
    }

    public async Task<BaseOutput<TestReportDto>> GetReportAsync(long projectId, int days = 14)
    {
        days = Math.Clamp(days, 1, 90);
        var startDate = DateTime.Today.AddDays(-(days - 1));

        var query = await _repository.GetQueryableAsync();

        // 仅投影统计所需的轻量字段，避免加载步骤详情等大字段
        var records = await AsyncExecuter.ToListAsync(
            query.Where(r => r.ProjectId == projectId && r.StartTime >= startDate)
                 .Select(r => new RecordRow
                 {
                     CaseId = r.CaseId,
                     CaseName = r.CaseName,
                     EnvId = r.EnvId,
                     EnvName = r.EnvName,
                     Status = r.Status,
                     StartTime = r.StartTime,
                     Duration = r.Duration,
                     ErrorMessage = r.ErrorMessage
                 }));

        var report = new TestReportDto
        {
            ProjectId = projectId,
            Days = days,
            Overview = new ExecutionStatistics
            {
                TotalExecutions = records.Count,
                SuccessExecutions = records.Count(r => r.Status == (int)ExecutionStatus.Success),
                FailedExecutions = records.Count(r => r.Status == (int)ExecutionStatus.Failed),
                CancelledExecutions = records.Count(r => r.Status == (int)ExecutionStatus.Cancelled),
                AverageDuration = records.Count > 0 ? records.Average(r => r.Duration) : 0,
                TotalDuration = records.Sum(r => r.Duration)
            },
            DailyTrend = BuildDailyTrend(records, startDate, days),
            TopFailedCases = BuildTopFailedCases(records),
            EnvironmentStats = BuildEnvironmentStats(records)
        };

        return new(report);
    }

    private static List<DailyExecutionStat> BuildDailyTrend(List<RecordRow> records, DateTime startDate, int days)
    {
        var byDay = records.GroupBy(r => r.StartTime.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        var trend = new List<DailyExecutionStat>();
        for (var i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var stat = new DailyExecutionStat { Date = date.ToString("yyyy-MM-dd") };

            if (byDay.TryGetValue(date, out var dayRecords))
            {
                stat.Total = dayRecords.Count;
                stat.Success = dayRecords.Count(r => r.Status == (int)ExecutionStatus.Success);
                stat.Failed = dayRecords.Count(r => r.Status == (int)ExecutionStatus.Failed);
                stat.Cancelled = dayRecords.Count(r => r.Status == (int)ExecutionStatus.Cancelled);
                stat.AverageDuration = dayRecords.Average(r => r.Duration);
            }

            trend.Add(stat);
        }

        return trend;
    }

    private static List<CaseFailureStat> BuildTopFailedCases(List<RecordRow> records)
    {
        return records
            .GroupBy(r => r.CaseId)
            .Select(g =>
            {
                var lastFailed = g.Where(r => r.Status == (int)ExecutionStatus.Failed)
                    .OrderByDescending(r => r.StartTime)
                    .FirstOrDefault();
                var total = g.Count();
                var failed = g.Count(r => r.Status == (int)ExecutionStatus.Failed);

                return new CaseFailureStat
                {
                    CaseId = g.Key,
                    CaseName = g.First().CaseName ?? string.Empty,
                    Total = total,
                    Failed = failed,
                    SuccessRate = total > 0 ? (double)(total - failed) / total * 100 : 0,
                    LastExecutionTime = g.Max(r => r.StartTime),
                    LastErrorMessage = lastFailed?.ErrorMessage
                };
            })
            .Where(s => s.Failed > 0)
            .OrderByDescending(s => s.Failed)
            .Take(10)
            .ToList();
    }

    private static List<EnvExecutionStat> BuildEnvironmentStats(List<RecordRow> records)
    {
        return records
            .GroupBy(r => r.EnvId)
            .Select(g =>
            {
                var total = g.Count();
                var success = g.Count(r => r.Status == (int)ExecutionStatus.Success);
                var failed = g.Count(r => r.Status == (int)ExecutionStatus.Failed);

                return new EnvExecutionStat
                {
                    EnvId = g.Key,
                    EnvName = g.First().EnvName ?? string.Empty,
                    Total = total,
                    Success = success,
                    Failed = failed,
                    SuccessRate = total > 0 ? (double)success / total * 100 : 0
                };
            })
            .OrderByDescending(s => s.Total)
            .ToList();
    }

    /// <summary>
    /// 执行记录统计投影行
    /// </summary>
    private sealed class RecordRow
    {
        public long CaseId { get; init; }
        public string? CaseName { get; init; }
        public long EnvId { get; init; }
        public string? EnvName { get; init; }
        public int Status { get; init; }
        public DateTime StartTime { get; init; }
        public long Duration { get; init; }
        public string? ErrorMessage { get; init; }
    }
}
