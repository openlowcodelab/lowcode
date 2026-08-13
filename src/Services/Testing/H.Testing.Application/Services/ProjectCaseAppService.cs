using H.Testing.Application.Contracts;
using H.Testing.Application.Mapping;
using H.Testing.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 测试用例服务（数据库存储）
/// </summary>
public class ProjectCaseAppService : ApplicationService, IProjectCaseAppService
{
    private readonly IRepository<CaseEntity, long> _repository;
    private readonly IRepository<CaseStepEntity, long> _stepRepository;

    public ProjectCaseAppService(
        IRepository<CaseEntity, long> repository,
        IRepository<CaseStepEntity, long> stepRepository)
    {
        _repository = repository;
        _stepRepository = stepRepository;
    }

    public async Task<List<CaseDto>> GetAllAsync()
    {
        var query = await _repository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.OrderBy(c => c.Order).ThenBy(c => c.Id));
        return await ToDtoListAsync(list);
    }

    public async Task<List<CaseDto>> GetByProjectIdAsync(long projectId)
    {
        var query = await _repository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(
            query.Where(c => c.ProjectId == projectId).OrderBy(c => c.Order).ThenBy(c => c.Id));
        return await ToDtoListAsync(list);
    }

    public async Task<CaseDto?> GetByIdAsync(long id)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return null;
        }

        var steps = await GetStepsAsync(id);
        return entity.ToDto(steps);
    }

    public async Task<long> CreateAsync(CaseDto projectCase)
    {
        var entity = new CaseEntity();
        projectCase.Apply(entity);
        entity = await _repository.InsertAsync(entity, autoSave: true);
        await SaveStepsAsync(entity.Id, projectCase.Steps);
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(long id, CaseDto projectCase)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        projectCase.Apply(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        await _stepRepository.DeleteAsync(s => s.CaseId == id, autoSave: true);
        await SaveStepsAsync(id, projectCase.Steps);
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        await _stepRepository.DeleteAsync(s => s.CaseId == id, autoSave: true);
        await _repository.DeleteAsync(entity, autoSave: true);
        return true;
    }

    public async Task<List<CaseDto>> GetActiveProjectCasesAsync()
    {
        var query = await _repository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.Where(c => c.Status == (int)CaseStatus.Active));
        return await ToDtoListAsync(list);
    }

    public async Task<List<CaseDto>> GetByTagsAsync(string tag)
    {
        var all = await GetAllAsync();
        return all.Where(tc => tc.Tags.Contains(tag)).ToList();
    }

    public async Task<List<CaseDto>> GetByLevelAsync(string level)
    {
        var all = await GetAllAsync();
        return all.Where(tc => tc.Levels.Contains(level)).ToList();
    }

    public async Task<List<CaseDto>> SearchAsync(string keyword)
    {
        var all = await GetAllAsync();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return all;
        }

        return all.Where(tc =>
            tc.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            tc.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            tc.Tags.Any(t => t.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        ).ToList();
    }

    public async Task<long> CopyAsync(long id)
    {
        var originalCase = await GetByIdAsync(id);
        if (originalCase == null)
        {
            throw new ArgumentException($"Test case with ID {id} not found");
        }

        var copiedCase = new CaseDto
        {
            CaseNumber = GenerateNewCaseNumber(originalCase.CaseNumber),
            Name = $"{originalCase.Name} - 副本",
            Description = originalCase.Description,
            ProjectId = originalCase.ProjectId,
            CategoryId = originalCase.CategoryId,
            Levels = new List<string>(originalCase.Levels),
            Tags = new List<string>(originalCase.Tags),
            Order = originalCase.Order,
            Status = originalCase.Status,
            TestData = new Dictionary<string, object>(originalCase.TestData),
            Steps = CopySteps(originalCase.Steps)
        };

        return await CreateAsync(copiedCase);
    }

    public async Task<List<long>> CopyBatchAsync(List<long> ids)
    {
        var copiedIds = new List<long>();
        foreach (var id in ids)
        {
            try
            {
                copiedIds.Add(await CopyAsync(id));
            }
            catch (Exception)
            {
                continue;
            }
        }

        return copiedIds;
    }

    private static string GenerateNewCaseNumber(string originalCaseNumber)
    {
        if (string.IsNullOrEmpty(originalCaseNumber))
        {
            return string.Empty;
        }

        var match = System.Text.RegularExpressions.Regex.Match(originalCaseNumber, @"^(.*)(\d+)$");
        if (match.Success)
        {
            var prefix = match.Groups[1].Value;
            var number = int.Parse(match.Groups[2].Value);
            return $"{prefix}{number + 1}";
        }

        return $"{originalCaseNumber}_copy";
    }

    /// <summary>
    /// 批量加载用例步骤并按用例分组（避免逐个用例查询）
    /// </summary>
    private async Task<Dictionary<long, List<CaseStepDto>>> LoadStepsMapAsync(IEnumerable<long> caseIds)
    {
        var ids = caseIds.ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<long, List<CaseStepDto>>();
        }

        var query = await _stepRepository.GetQueryableAsync();
        var steps = await AsyncExecuter.ToListAsync(
            query.Where(s => ids.Contains(s.CaseId)).OrderBy(s => s.Order).ThenBy(s => s.Id));
        return steps.GroupBy(s => s.CaseId)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ToDto()).ToList());
    }

    private async Task<List<CaseStepDto>> GetStepsAsync(long caseId)
    {
        var query = await _stepRepository.GetQueryableAsync();
        var steps = await AsyncExecuter.ToListAsync(
            query.Where(s => s.CaseId == caseId).OrderBy(s => s.Order).ThenBy(s => s.Id));
        return steps.Select(e => e.ToDto()).ToList();
    }

    private async Task<List<CaseDto>> ToDtoListAsync(List<CaseEntity> entities)
    {
        var stepsMap = await LoadStepsMapAsync(entities.Select(e => e.Id));
        return entities.Select(e => e.ToDto(stepsMap.GetValueOrDefault(e.Id))).ToList();
    }

    private async Task SaveStepsAsync(long caseId, List<CaseStepDto> steps)
    {
        if (steps.Count == 0)
        {
            return;
        }

        var entities = steps.Select(s => s.ToEntity(caseId)).ToList();
        await _stepRepository.InsertManyAsync(entities, autoSave: true);
    }

    private static List<CaseStepDto> CopySteps(List<CaseStepDto> originalSteps)
    {
        var copiedSteps = new List<CaseStepDto>();
        foreach (var originalStep in originalSteps)
        {
            var copiedStep = new CaseStepDto
            {
                Id = Guid.NewGuid().ToString(),
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
