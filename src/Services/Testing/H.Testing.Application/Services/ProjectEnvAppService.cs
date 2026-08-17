using H.Testing.Application.Contracts;
using H.Testing.Application.Mapping;
using H.Testing.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 项目环境服务（数据库存储）
/// </summary>
public class ProjectEnvAppService : ApplicationService, IProjectEnvAppService
{
    private readonly IRepository<ProjectEnvEntity, long> _repository;

    public ProjectEnvAppService(IRepository<ProjectEnvEntity, long> repository)
    {
        _repository = repository;
    }

    public async Task<BaseOutput<List<ProjectEnvDto>>> GetAllAsync()
    {
        var query = await _repository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.OrderBy(e => e.Id));
        return BaseOutput<List<ProjectEnvDto>>.Ok(list.Select(e => e.ToDto()).ToList());
    }

    public async Task<BaseOutput<List<ProjectEnvDto>>> GetByProjectIdAsync(long projectId)
    {
        var query = await _repository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.Where(e => e.ProjectId == projectId).OrderBy(e => e.Id));
        return BaseOutput<List<ProjectEnvDto>>.Ok(list.Select(e => e.ToDto()).ToList());
    }

    public async Task<BaseOutput<ProjectEnvDto?>> GetByIdAsync(long id)
    {
        var entity = await _repository.FindAsync(id);
        return BaseOutput<ProjectEnvDto?>.Ok(entity?.ToDto());
    }

    public async Task<BaseOutput<long>> CreateAsync(ProjectEnvDto environment)
    {
        var entity = new ProjectEnvEntity();
        environment.Apply(entity);
        entity = await _repository.InsertAsync(entity, autoSave: true);
        return BaseOutput<long>.Ok(entity.Id);
    }

    public async Task<BaseOutput<bool>> UpdateAsync(long id, ProjectEnvDto environment)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return BaseOutput<bool>.Ok(false);
        }

        environment.Apply(entity);
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

        await _repository.DeleteAsync(entity, autoSave: true);
        return BaseOutput<bool>.Ok(true);
    }

    public async Task<BaseOutput<List<ProjectEnvDto>>> GetByTypeAsync(EnvironmentType type)
    {
        var query = await _repository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.Where(e => e.Type == (int)type));
        return BaseOutput<List<ProjectEnvDto>>.Ok(list.Select(e => e.ToDto()).ToList());
    }
}
