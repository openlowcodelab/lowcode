using H.Testing.Application.Contracts;
using H.Testing.Application.Mapping;
using H.Testing.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 测试用例服务（数据库存储）
/// </summary>
public class CaseAppService : ApplicationService, ICaseAppService
{
    private readonly IRepository<CaseEntity, long> _repository;
    private readonly ICaseStepAppService _stepService;

    public CaseAppService(
        IRepository<CaseEntity, long> repository,
        ICaseStepAppService stepService)
    {
        _repository = repository;
        _stepService = stepService;
    }

    public async Task<BaseOutput<List<CaseDto>>> GetAllAsync()
    {
        var query = await _repository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.OrderBy(c => c.Order).ThenBy(c => c.Id));
        return BaseOutput<List<CaseDto>>.Ok(list.Select(e => e.ToDto()).ToList());
    }

    public async Task<BaseOutput<List<CaseDto>>> GetByProjectIdAsync(long projectId)
    {
        var query = await _repository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(
            query.Where(c => c.ProjectId == projectId).OrderBy(c => c.Order).ThenBy(c => c.Id));
        return BaseOutput<List<CaseDto>>.Ok(list.Select(e => e.ToDto()).ToList());
    }

    public async Task<BaseOutput<CaseDto?>> GetByIdAsync(long id)
    {
        var entity = await _repository.FindAsync(id);
        return BaseOutput<CaseDto?>.Ok(entity?.ToDto());
    }

    public async Task<BaseOutput<long>> CreateAsync(CaseDto projectCase)
    {
        var entity = new CaseEntity();
        projectCase.Apply(entity);
        entity = await _repository.InsertAsync(entity, autoSave: true);
        return BaseOutput<long>.Ok(entity.Id);
    }

    public async Task<BaseOutput<bool>> UpdateAsync(long id, CaseDto projectCase)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return BaseOutput<bool>.Ok(false);
        }

        projectCase.Apply(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return BaseOutput<bool>.Ok(true);
    }

    public async Task<BaseOutput<bool>> DeleteAsync(long id)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return BaseOutput<bool>.Ok(false);
        }

        await _stepService.DeleteByCaseIdAsync(id);
        await _repository.DeleteAsync(entity, autoSave: true);
        return BaseOutput<bool>.Ok(true);
    }

    public async Task<BaseOutput<List<CaseDto>>> GetActiveProjectCasesAsync()
    {
        var query = await _repository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.Where(c => c.Status == (int)CaseStatus.Active));
        return BaseOutput<List<CaseDto>>.Ok(list.Select(e => e.ToDto()).ToList());
    }

    public async Task<BaseOutput<List<CaseDto>>> GetByLevelAsync(CaseLevel level)
    {
        var query = await _repository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.Where(c => c.Level == (int)level));
        return BaseOutput<List<CaseDto>>.Ok(list.Select(e => e.ToDto()).ToList());
    }

    public async Task<BaseOutput<List<CaseDto>>> SearchAsync(string keyword)
    {
        var all = (await GetAllAsync()).Data ?? [];
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BaseOutput<List<CaseDto>>.Ok(all);
        }

        return BaseOutput<List<CaseDto>>.Ok(all.Where(tc =>
            tc.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            tc.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)
        ).ToList());
    }

    public async Task<BaseOutput<long>> CopyAsync(long id)
    {
        var originalCase = (await GetByIdAsync(id)).Data;
        if (originalCase == null)
        {
            throw new ArgumentException($"Test case with ID {id} not found");
        }

        var copiedCase = new CaseDto
        {
            Name = $"{originalCase.Name} - 副本",
            Description = originalCase.Description,
            ProjectId = originalCase.ProjectId,
            CategoryId = originalCase.CategoryId,
            Level = originalCase.Level,
            Order = originalCase.Order,
            Status = originalCase.Status
        };

        var newId = (await CreateAsync(copiedCase)).Data;

        // 复制原用例的步骤（步骤 Id 置空以生成新行）
        var originalSteps = (await _stepService.GetByCaseIdAsync(id)).Data ?? [];
        if (originalSteps.Count > 0)
        {
            await _stepService.SaveAsync(newId, CopySteps(originalSteps));
        }

        return BaseOutput<long>.Ok(newId);
    }

    public async Task<List<long>> CopyBatchAsync(List<long> ids)
    {
        var copiedIds = new List<long>();
        foreach (var id in ids)
        {
            try
            {
                copiedIds.Add((await CopyAsync(id)).Data);
            }
            catch (Exception)
            {
                continue;
            }
        }

        return copiedIds;
    }

    private static List<CaseStepDto> CopySteps(List<CaseStepDto> originalSteps)
    {
        var copiedSteps = new List<CaseStepDto>();
        foreach (var originalStep in originalSteps)
        {
            var copiedStep = new CaseStepDto
            {
                Id = string.Empty,
                Name = originalStep.Name,
                Type = originalStep.Type,
                Parameters = new Dictionary<string, object>(originalStep.Parameters),
                ExpectedResult = originalStep.ExpectedResult,
                Order = originalStep.Order,
                IsEnabled = originalStep.IsEnabled
            };

            if (originalStep.ApiConfig != null)
            {
                copiedStep.ApiConfig = new ApiStepConfig
                {
                    Method = originalStep.ApiConfig.Method,
                    ServiceId = originalStep.ApiConfig.ServiceId,
                    Url = originalStep.ApiConfig.Url,
                    Headers = new Dictionary<string, string>(originalStep.ApiConfig.Headers),
                    Params = new Dictionary<string, string>(originalStep.ApiConfig.Params),
                    Body = originalStep.ApiConfig.Body,
                    BodyType = originalStep.ApiConfig.BodyType,
                    Cookies = new Dictionary<string, string>(originalStep.ApiConfig.Cookies),
                    TimeoutMs = originalStep.ApiConfig.TimeoutMs,
                    FollowRedirects = originalStep.ApiConfig.FollowRedirects,
                    VerifySSL = originalStep.ApiConfig.VerifySSL,
                    VariableExtractions = new Dictionary<string, string>(originalStep.ApiConfig.VariableExtractions),
                    Assertions = CopyApiAssertions(originalStep.ApiConfig.Assertions),
                    Auth = CopyAuthConfig(originalStep.ApiConfig.Auth)
                };
            }

            if (originalStep.UiConfig != null)
            {
                copiedStep.UiConfig = new UiStepConfig
                {
                    Action = originalStep.UiConfig.Action,
                    Selector = originalStep.UiConfig.Selector,
                    SelectorType = originalStep.UiConfig.SelectorType,
                    Value = originalStep.UiConfig.Value,
                    TimeoutMs = originalStep.UiConfig.TimeoutMs,
                    TakeScreenshot = originalStep.UiConfig.TakeScreenshot,
                    Options = new Dictionary<string, string>(originalStep.UiConfig.Options)
                };
            }

            if (originalStep.ScriptConfig != null)
            {
                copiedStep.ScriptConfig = new ScriptStepConfig
                {
                    ScriptType = originalStep.ScriptConfig.ScriptType,
                    ScriptContent = originalStep.ScriptConfig.ScriptContent,
                    TimeoutMs = originalStep.ScriptConfig.TimeoutMs,
                    CaptureOutput = originalStep.ScriptConfig.CaptureOutput,
                    ExpectedReturnType = originalStep.ScriptConfig.ExpectedReturnType,
                    Parameters = new Dictionary<string, object>(originalStep.ScriptConfig.Parameters),
                    VariableExtractions = new Dictionary<string, string>(originalStep.ScriptConfig.VariableExtractions)
                };
            }

            copiedSteps.Add(copiedStep);
        }

        return copiedSteps;
    }

    private static List<ApiAssertion> CopyApiAssertions(List<ApiAssertion> originalAssertions)
    {
        return originalAssertions.Select(assertion => new ApiAssertion
        {
            Type = assertion.Type,
            Target = assertion.Target,
            Operator = assertion.Operator,
            ExpectedValue = assertion.ExpectedValue,
            Description = assertion.Description
        }).ToList();
    }

    private static AuthConfig CopyAuthConfig(AuthConfig originalAuth)
    {
        return new AuthConfig
        {
            Type = originalAuth.Type,
            Username = originalAuth.Username,
            Password = originalAuth.Password,
            Token = originalAuth.Token,
            ApiKeyName = originalAuth.ApiKeyName,
            ApiKeyValue = originalAuth.ApiKeyValue,
            ApiKeyLocation = originalAuth.ApiKeyLocation
        };
    }
}
