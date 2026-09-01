using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;
using Hangfire;
using H.Util.Base;
using System.Text.Json;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// CI 集成服务：流水线通过 HTTP 触发执行并轮询结果（令牌校验）
/// </summary>
public class CiAppService : ApplicationService, ICiAppService
{
    private readonly IRepository<CiRunEntity, long> _repository;
    private readonly IRepository<CaseRecordEntity, long> _recordRepository;
    private readonly ITestingSettingsAppService _settingsService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IProjectAppService _projectService;
    private readonly IEnvironmentAppService _environmentService;

    public CiAppService(
        IRepository<CiRunEntity, long> repository,
        IRepository<CaseRecordEntity, long> recordRepository,
        ITestingSettingsAppService settingsService,
        IBackgroundJobClient backgroundJobClient,
        IProjectAppService projectService,
        IEnvironmentAppService environmentService)
    {
        _repository = repository;
        _recordRepository = recordRepository;
        _settingsService = settingsService;
        _backgroundJobClient = backgroundJobClient;
        _projectService = projectService;
        _environmentService = environmentService;
    }

    public async Task<BaseOutput<long>> TriggerAsync(CiTriggerDto dto)
    {
        await ValidateTokenAsync(dto.Token);

        var project = (await _projectService.GetByIdAsync(dto.ProjectId)).Data;
        if (project == null)
        {
            throw new UserFriendlyException($"项目 {dto.ProjectId} 不存在");
        }

        var environments = (await _environmentService.GetByProjectIdAsync(dto.ProjectId)).Data ?? [];
        if (!environments.Any(e => e.Id == dto.EnvId))
        {
            throw new UserFriendlyException($"环境 {dto.EnvId} 不存在或不属于该项目");
        }

        var entity = new CiRunEntity
        {
            ProjectId = dto.ProjectId,
            EnvId = dto.EnvId,
            CaseIdsJson = dto.CaseIds is { Count: > 0 } ? JsonSerializer.Serialize(dto.CaseIds) : null,
            BrowsersJson = dto.Browsers is { Count: > 0 } ? JsonSerializer.Serialize(dto.Browsers) : null,
            Headless = dto.Headless,
            WebhookUrl = string.IsNullOrWhiteSpace(dto.WebhookUrl) ? null : dto.WebhookUrl.Trim(),
            Status = (int)ExecutionStatus.Pending
        };

        entity = await _repository.InsertAsync(entity, autoSave: true);

        // 入队后台执行，HTTP 调用立即返回运行ID
        _backgroundJobClient.Enqueue<ICiJobExecutor>(x => x.ExecuteAsync(entity.Id));

        return new(entity.Id);
    }

    public async Task<BaseOutput<CiRunDto?>> GetRunAsync(long runId, string token)
    {
        await ValidateTokenAsync(token);

        var entity = await _repository.FindAsync(runId);
        if (entity == null)
        {
            return new(null);
        }

        var dto = new CiRunDto
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            EnvId = entity.EnvId,
            Status = (ExecutionStatus)entity.Status,
            TotalCases = entity.TotalCases,
            SuccessCases = entity.SuccessCases,
            FailedCases = entity.FailedCases,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            ErrorMessage = entity.ErrorMessage
        };

        // 已结束时附带各用例执行摘要（按运行时间窗口关联执行记录）
        if (entity.Status != (int)ExecutionStatus.Pending && entity.Status != (int)ExecutionStatus.Running
            && entity.StartTime.HasValue)
        {
            var recordQuery = await _recordRepository.GetQueryableAsync();
            var records = await AsyncExecuter.ToListAsync(
                recordQuery.Where(r => r.ProjectId == entity.ProjectId
                                       && r.EnvId == entity.EnvId
                                       && r.StartTime >= entity.StartTime.Value
                                       && (entity.EndTime == null || r.StartTime <= entity.EndTime.Value.AddMinutes(1)))
                           .OrderBy(r => r.StartTime));

            dto.CaseResults = records.Select(r => new CiRunCaseResult
            {
                CaseId = r.CaseId,
                CaseName = r.CaseName ?? string.Empty,
                Status = (ExecutionStatus)r.Status,
                ErrorMessage = r.ErrorMessage,
                DataTag = r.DataTag
            }).ToList();
        }

        return new(dto);
    }

    private async Task ValidateTokenAsync(string? token)
    {
        var expected = (await _settingsService.GetCiTokenAsync()).Data;

        if (string.IsNullOrWhiteSpace(token) || !string.Equals(token, expected, StringComparison.Ordinal))
        {
            throw new UserFriendlyException("CI 令牌无效");
        }
    }
}
