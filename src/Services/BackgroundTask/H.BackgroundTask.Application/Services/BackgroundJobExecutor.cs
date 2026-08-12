using H.BackgroundTask.Application.Contracts;
using H.BackgroundTask.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace H.BackgroundTask.Application.Services;

/// <summary>
/// 后台任务执行器：由 Hangfire 作业服务器在后台线程调用。
/// Hangfire 运行在 ABP 环境之外，因此需要手动开启工作单元来读写数据库。
/// </summary>
public class BackgroundJobExecutor : IBackgroundJobExecutor
{
    private const int MaxResultLength = 8000;

    private readonly IRepository<BackgroundJobEntity, Guid> _jobRepository;
    private readonly IRepository<JobExecutionRecordEntity, Guid> _recordRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ILogger<BackgroundJobExecutor> _logger;

    public BackgroundJobExecutor(
        IRepository<BackgroundJobEntity, Guid> jobRepository,
        IRepository<JobExecutionRecordEntity, Guid> recordRepository,
        IHttpClientFactory httpClientFactory,
        IUnitOfWorkManager unitOfWorkManager,
        ILogger<BackgroundJobExecutor> logger)
    {
        _jobRepository = jobRepository;
        _recordRepository = recordRepository;
        _httpClientFactory = httpClientFactory;
        _unitOfWorkManager = unitOfWorkManager;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid jobId)
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true);

        var job = await _jobRepository.FindAsync(jobId);
        if (job is null)
        {
            _logger.LogWarning("后台任务 {JobId} 不存在，跳过执行", jobId);
            await uow.CompleteAsync();
            return;
        }

        var startTime = DateTime.Now;
        var stopwatch = Stopwatch.StartNew();
        var status = JobExecutionStatus.Success;
        string? result = null;
        string? errorMessage = null;

        try
        {
            result = (JobExecuteType)job.ExecuteType switch
            {
                JobExecuteType.Api => await ExecuteApiAsync(job),
                JobExecuteType.Sql => await ExecuteSqlAsync(job),
                _ => throw new NotSupportedException($"不支持的执行类型：{job.ExecuteType}")
            };
        }
        catch (Exception ex)
        {
            status = JobExecutionStatus.Failed;
            errorMessage = ex.Message;
            _logger.LogError(ex, "后台任务 {JobName}({JobId}) 执行失败", job.Name, jobId);
        }
        finally
        {
            stopwatch.Stop();
        }

        var record = new JobExecutionRecordEntity
        {
            JobId = job.Id,
            JobName = job.Name,
            ExecuteType = job.ExecuteType,
            Status = (int)status,
            StartTime = startTime,
            EndTime = DateTime.Now,
            DurationMs = stopwatch.ElapsedMilliseconds,
            Result = Truncate(result),
            ErrorMessage = Truncate(errorMessage)
        };
        await _recordRepository.InsertAsync(record);

        job.LastExecutionTime = startTime;
        job.LastExecutionStatus = (int)status;
        await _jobRepository.UpdateAsync(job);

        await uow.CompleteAsync();
    }

    private async Task<string> ExecuteApiAsync(BackgroundJobEntity job)
    {
        if (string.IsNullOrWhiteSpace(job.ApiUrl))
        {
            throw new ArgumentException("API 任务必须配置 API 地址");
        }

        var method = new HttpMethod(string.IsNullOrWhiteSpace(job.ApiHttpMethod) ? "GET" : job.ApiHttpMethod!.Trim().ToUpperInvariant());
        using var request = new HttpRequestMessage(method, job.ApiUrl);

        // 请求头
        if (!string.IsNullOrWhiteSpace(job.ApiHeaders))
        {
            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(job.ApiHeaders!);
            if (headers is not null)
            {
                foreach (var (key, value) in headers)
                {
                    request.Headers.TryAddWithoutValidation(key, value);
                }
            }
        }

        // 请求体（非 GET/HEAD 时携带）
        if (!string.IsNullOrWhiteSpace(job.ApiBody) && method != HttpMethod.Get && method != HttpMethod.Head)
        {
            request.Content = new StringContent(job.ApiBody!, Encoding.UTF8, "application/json");
        }

        var client = _httpClientFactory.CreateClient("BackgroundTask");
        client.Timeout = TimeSpan.FromMinutes(2);

        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}：{body}");
        }

        return $"HTTP {(int)response.StatusCode}\n{body}";
    }

    private static async Task<string> ExecuteSqlAsync(BackgroundJobEntity job)
    {
        if (string.IsNullOrWhiteSpace(job.SqlConnectionString))
        {
            throw new ArgumentException("SQL 任务必须配置数据源连接字符串");
        }
        if (string.IsNullOrWhiteSpace(job.SqlStatement))
        {
            throw new ArgumentException("SQL 任务必须配置 SQL 语句");
        }

        await using var connection = new SqlConnection(job.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = job.SqlStatement;
        command.CommandTimeout = 120;

        var affected = await command.ExecuteNonQueryAsync();
        return $"执行成功，影响行数：{affected}";
    }

    private static string? Truncate(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }
        return text.Length <= MaxResultLength ? text : text[..MaxResultLength] + "...（已截断）";
    }
}
