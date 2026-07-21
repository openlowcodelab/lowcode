using H.Testing.Application.Contracts;
using H.Testing.Application.Mapping;
using H.Testing.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 测试环境服务（数据库存储，基于项目环境 + 环境服务配置聚合）
/// </summary>
public class EnvironmentAppService : ApplicationService, IEnvironmentAppService
{
    private readonly IRepository<TestingProjectEnvironment, long> _repository;
    private readonly IProjectServiceConfigAppService _projectServiceConfigService;

    public EnvironmentAppService(
        IRepository<TestingProjectEnvironment, long> repository,
        IProjectServiceConfigAppService projectServiceConfigService)
    {
        _repository = repository;
        _projectServiceConfigService = projectServiceConfigService;
    }

    public async Task<List<EnvironmentDto>> GetByProjectIdAsync(long projectId)
    {
        var query = await _repository.GetQueryableAsync();
        var entities = await AsyncExecuter.ToListAsync(query.Where(e => e.ProjectId == projectId));

        var environments = new List<EnvironmentDto>();
        foreach (var entity in entities)
        {
            var projectEnv = entity.ToDto();
            var testEnv = new EnvironmentDto
            {
                Id = projectEnv.Id,
                Name = projectEnv.Name,
                Description = projectEnv.Description,
                ProjectId = projectEnv.ProjectId,
                Status = projectEnv.Status,
                CreatedAt = projectEnv.CreatedAt,
                UpdatedAt = projectEnv.UpdatedAt,
                CreatedBy = projectEnv.CreatedBy,
                UpdatedBy = projectEnv.UpdatedBy,
                Order = 0,
                Config = new Dictionary<string, object>(),
                ServiceEndpoints = new Dictionary<long, string>()
            };

            foreach (var variable in projectEnv.Variables)
            {
                testEnv.Config[variable.Key] = variable.Value;
            }

            var envServiceConfigs = await _projectServiceConfigService.GetEnvironmentServiceConfigsAsync(projectEnv.Id);
            foreach (var cfg in envServiceConfigs)
            {
                if (!string.IsNullOrWhiteSpace(cfg.BaseUrl))
                {
                    testEnv.ServiceEndpoints[cfg.ProjectServiceId] = cfg.BaseUrl;
                }
            }

            environments.Add(testEnv);
        }

        return environments.OrderBy(e => e.Order).ThenBy(e => e.CreatedAt).ToList();
    }

    public async Task<EnvironmentDto?> GetByIdAsync(long projectId, long id)
    {
        var environments = await GetByProjectIdAsync(projectId);
        return environments.FirstOrDefault(e => e.Id == id);
    }

    public async Task<EnvironmentDto> CreateAsync(EnvironmentDto environment)
    {
        var entity = new TestingProjectEnvironment
        {
            ProjectId = environment.ProjectId,
            Name = environment.Name,
            Description = environment.Description,
            Type = (int)EnvironmentType.Development,
            Status = (int)environment.Status,
            VariablesJson = SerializeConfig(environment.Config),
            CreatedBy = environment.CreatedBy,
            UpdatedBy = environment.UpdatedBy
        };

        entity = await _repository.InsertAsync(entity, autoSave: true);
        environment.Id = entity.Id;
        return environment;
    }

    public async Task<bool> UpdateAsync(long id, EnvironmentDto environment)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        entity.Name = environment.Name;
        entity.Description = environment.Description;
        entity.Status = (int)environment.Status;
        entity.VariablesJson = SerializeConfig(environment.Config);
        entity.UpdatedBy = environment.UpdatedBy;
        await _repository.UpdateAsync(entity, autoSave: true);
        return true;
    }

    public async Task<bool> DeleteAsync(long projectId, long id)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null || entity.ProjectId != projectId)
        {
            return false;
        }

        await _repository.DeleteAsync(entity, autoSave: true);
        return true;
    }

    private static string SerializeConfig(Dictionary<string, object> config)
    {
        var variables = config.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? string.Empty);
        return System.Text.Json.JsonSerializer.Serialize(variables, TestingMappers.JsonOptions);
    }
}
