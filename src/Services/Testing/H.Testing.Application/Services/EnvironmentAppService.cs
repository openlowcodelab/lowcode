using H.Testing.Application.Contracts;
using H.Testing.Application.Mapping;
using H.Testing.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 测试环境服务（执行时视图）
/// 将环境（含服务配置）组合为执行引擎使用的 <see cref="EnvironmentDto"/>
/// </summary>
public class EnvironmentAppService : ApplicationService, IEnvironmentAppService
{
    private readonly IRepository<ProjectEnvEntity, long> _envRepository;

    public EnvironmentAppService(IRepository<ProjectEnvEntity, long> envRepository)
    {
        _envRepository = envRepository;
    }

    public async Task<BaseOutput<List<EnvironmentDto>>> GetByProjectIdAsync(long projectId)
    {
        var envQuery = await _envRepository.GetQueryableAsync();
        var envs = await AsyncExecuter.ToListAsync(
            envQuery.Where(e => e.ProjectId == projectId).OrderBy(e => e.Id));
        return BaseOutput<List<EnvironmentDto>>.Ok(envs.Select(e => e.ToEnvironmentDto()).ToList());
    }

    public async Task<BaseOutput<EnvironmentDto?>> GetByIdAsync(long projectId, long id)
    {
        var entity = await _envRepository.FindAsync(id);
        if (entity == null || entity.ProjectId != projectId)
        {
            return BaseOutput<EnvironmentDto?>.Ok(null);
        }

        return BaseOutput<EnvironmentDto?>.Ok(entity.ToEnvironmentDto());
    }

    public async Task<BaseOutput<EnvironmentDto>> CreateAsync(EnvironmentDto environment)
    {
        var entity = new ProjectEnvEntity
        {
            ProjectId = environment.ProjectId,
            Name = environment.Name,
            Description = environment.Description,
            VariablesJson = System.Text.Json.JsonSerializer.Serialize(
                environment.Config.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? string.Empty))
        };
        entity = await _envRepository.InsertAsync(entity, autoSave: true);
        environment.Id = entity.Id;
        return BaseOutput<EnvironmentDto>.Ok(environment);
    }

    public async Task<BaseOutput<bool>> UpdateAsync(long id, EnvironmentDto environment)
    {
        var entity = await _envRepository.FindAsync(id);
        if (entity == null)
        {
            return BaseOutput<bool>.Ok(false);
        }

        entity.Name = environment.Name;
        entity.Description = environment.Description;
        entity.VariablesJson = System.Text.Json.JsonSerializer.Serialize(
            environment.Config.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? string.Empty));
        await _envRepository.UpdateAsync(entity, autoSave: true);
        return BaseOutput<bool>.Ok(true);
    }

    public async Task<BaseOutput<bool>> DeleteAsync(long projectId, long id)
    {
        var entity = await _envRepository.FindAsync(id);
        if (entity == null || entity.ProjectId != projectId)
        {
            return BaseOutput<bool>.Ok(false);
        }

        await _envRepository.DeleteAsync(entity, autoSave: true);
        return BaseOutput<bool>.Ok(true);
    }
}
