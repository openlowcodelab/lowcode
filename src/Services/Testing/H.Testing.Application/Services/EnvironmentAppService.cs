using H.Testing.Application.Contracts;
using H.Testing.Application.Mapping;
using H.Testing.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 测试环境服务（执行时视图）
/// 将环境与环境服务配置组合为执行引擎使用的 <see cref="EnvironmentDto"/>
/// </summary>
public class EnvironmentAppService : ApplicationService, IEnvironmentAppService
{
    private readonly IRepository<ProjectEnvEntity, long> _envRepository;
    private readonly IRepository<ProjectEnvConfigEntity, long> _configRepository;

    public EnvironmentAppService(
        IRepository<ProjectEnvEntity, long> envRepository,
        IRepository<ProjectEnvConfigEntity, long> configRepository)
    {
        _envRepository = envRepository;
        _configRepository = configRepository;
    }

    public async Task<List<EnvironmentDto>> GetByProjectIdAsync(long projectId)
    {
        var envQuery = await _envRepository.GetQueryableAsync();
        var envs = await AsyncExecuter.ToListAsync(
            envQuery.Where(e => e.ProjectId == projectId).OrderBy(e => e.Id));
        if (envs.Count == 0)
        {
            return new List<EnvironmentDto>();
        }

        var envIds = envs.Select(e => e.Id).ToList();
        var configQuery = await _configRepository.GetQueryableAsync();
        var configs = await AsyncExecuter.ToListAsync(configQuery.Where(c => envIds.Contains(c.EnvId)));
        return envs.Select(e => e.ToEnvironmentDto(configs)).ToList();
    }

    public async Task<EnvironmentDto?> GetByIdAsync(long projectId, long id)
    {
        var entity = await _envRepository.FindAsync(id);
        if (entity == null || entity.ProjectId != projectId)
        {
            return null;
        }

        var configQuery = await _configRepository.GetQueryableAsync();
        var configs = await AsyncExecuter.ToListAsync(configQuery.Where(c => c.EnvId == id));
        return entity.ToEnvironmentDto(configs);
    }

    public async Task<EnvironmentDto> CreateAsync(EnvironmentDto environment)
    {
        var entity = new ProjectEnvEntity
        {
            ProjectId = environment.ProjectId,
            Name = environment.Name,
            Description = environment.Description,
            Status = (int)environment.Status,
            VariablesJson = System.Text.Json.JsonSerializer.Serialize(
                environment.Config.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? string.Empty))
        };
        entity = await _envRepository.InsertAsync(entity, autoSave: true);
        environment.Id = entity.Id;
        return environment;
    }

    public async Task<bool> UpdateAsync(long id, EnvironmentDto environment)
    {
        var entity = await _envRepository.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        entity.Name = environment.Name;
        entity.Description = environment.Description;
        entity.Status = (int)environment.Status;
        entity.VariablesJson = System.Text.Json.JsonSerializer.Serialize(
            environment.Config.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? string.Empty));
        await _envRepository.UpdateAsync(entity, autoSave: true);
        return true;
    }

    public async Task<bool> DeleteAsync(long projectId, long id)
    {
        var entity = await _envRepository.FindAsync(id);
        if (entity == null || entity.ProjectId != projectId)
        {
            return false;
        }

        await _envRepository.DeleteAsync(entity, autoSave: true);
        await _configRepository.DeleteAsync(c => c.EnvId == id, autoSave: true);
        return true;
    }
}
