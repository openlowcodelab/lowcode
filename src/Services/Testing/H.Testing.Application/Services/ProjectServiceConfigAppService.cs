using H.Testing.Application.Contracts;
using H.Testing.Application.Mapping;
using H.Testing.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 项目服务配置管理服务（数据库存储，环境服务配置存于环境的 ServiceConfigsJson 字段）
/// </summary>
public class ProjectServiceConfigAppService : ApplicationService, IProjectServiceConfigAppService
{
    private readonly IRepository<ProjectServiceEntity, long> _serviceRepository;
    private readonly IRepository<ProjectEnvEntity, long> _envRepository;

    public ProjectServiceConfigAppService(
        IRepository<ProjectServiceEntity, long> serviceRepository,
        IRepository<ProjectEnvEntity, long> envRepository)
    {
        _serviceRepository = serviceRepository;
        _envRepository = envRepository;
    }

    public async Task<BaseOutput<List<ProjectServiceDto>>> GetProjectServicesAsync(long projectId)
    {
        var query = await _serviceRepository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.Where(s => s.ProjectId == projectId).OrderBy(s => s.Id));
        return new(list.Select(e => e.ToDto()).ToList());
    }

    public async Task<BaseOutput<ProjectServiceDto>> CreateProjectServiceAsync(ProjectServiceDto service)
    {
        var entity = new ProjectServiceEntity();
        service.Apply(entity);
        entity = await _serviceRepository.InsertAsync(entity, autoSave: true);
        return new(entity.ToDto());
    }

    public async Task<BaseOutput<ProjectServiceDto>> UpdateProjectServiceAsync(long serviceId, ProjectServiceDto service)
    {
        var entity = await _serviceRepository.FindAsync(serviceId);
        if (entity == null)
        {
            throw new ArgumentException($"Service with ID {serviceId} not found");
        }

        entity.Name = service.Name;
        entity.Description = service.Description;
        entity = await _serviceRepository.UpdateAsync(entity, autoSave: true);
        return new(entity.ToDto());
    }

    public async Task<BaseOutput> DeleteProjectServiceAsync(long projectId, long serviceId)
    {
        await _serviceRepository.DeleteAsync(s => s.Id == serviceId, autoSave: true);
        // 移除项目下各环境中对该服务的基础URL配置
        var query = await _envRepository.GetQueryableAsync();
        var envs = await AsyncExecuter.ToListAsync(query.Where(e => e.ProjectId == projectId));
        var changed = new List<ProjectEnvEntity>();
        foreach (var env in envs)
        {
            var configs = TestingMappers.DeserializeServiceConfigs(env.ServiceConfigsJson);
            if (configs.Remove(serviceId))
            {
                env.ServiceConfigsJson = TestingMappers.SerializeServiceConfigs(configs);
                changed.Add(env);
            }
        }
        if (changed.Count > 0) await _envRepository.UpdateManyAsync(changed, autoSave: true);
        return new();
    }

    public async Task<BaseOutput<List<ProjectEnvConfigDto>>> GetEnvironmentServiceConfigsAsync(long environmentId)
    {
        var entity = await _envRepository.FindAsync(environmentId);
        return new(entity?.ToConfigDtos() ?? new List<ProjectEnvConfigDto>());
    }

    public async Task<BaseOutput<ProjectEnvConfigDto>> UpdateEnvironmentServiceConfigAsync(ProjectEnvConfigDto config)
        => new(await UpsertEnvironmentServiceConfigAsync(config));

    public async Task<BaseOutput<ProjectEnvConfigDto>> CreateEnvironmentServiceConfigAsync(ProjectEnvConfigDto config)
        => new(await UpsertEnvironmentServiceConfigAsync(config));

    /// <summary>
    /// 新增或更新环境服务配置（以 环境Id + 项目服务Id 为键写入环境的 ServiceConfigsJson）
    /// </summary>
    private async Task<ProjectEnvConfigDto> UpsertEnvironmentServiceConfigAsync(ProjectEnvConfigDto config)
    {
        var entity = await _envRepository.FindAsync(config.EnvironmentId)
            ?? throw new ArgumentException($"Environment with ID {config.EnvironmentId} not found");

        var configs = TestingMappers.DeserializeServiceConfigs(entity.ServiceConfigsJson);
        configs[config.ProjectServiceId] = config.BaseUrl ?? string.Empty;
        entity.ServiceConfigsJson = TestingMappers.SerializeServiceConfigs(configs);
        entity = await _envRepository.UpdateAsync(entity, autoSave: true);

        return new ProjectEnvConfigDto
        {
            EnvironmentId = entity.Id,
            ProjectServiceId = config.ProjectServiceId,
            BaseUrl = config.BaseUrl ?? string.Empty
        };
    }

    public async Task<BaseOutput> DeleteEnvironmentServiceConfigAsync(long environmentId, long projectServiceId)
    {
        var entity = await _envRepository.FindAsync(environmentId);
        if (entity == null)
        {
            return new();
        }

        var configs = TestingMappers.DeserializeServiceConfigs(entity.ServiceConfigsJson);
        if (configs.Remove(projectServiceId))
        {
            entity.ServiceConfigsJson = TestingMappers.SerializeServiceConfigs(configs);
            await _envRepository.UpdateAsync(entity, autoSave: true);
        }
        return new();
    }

    public async Task<BaseOutput<List<ServiceConfigView>>> GetServiceConfigViewsAsync(long environmentId, long projectId)
    {
        var services = (await GetProjectServicesAsync(projectId)).Data ?? [];
        var configs = (await GetEnvironmentServiceConfigsAsync(environmentId)).Data ?? [];

        var views = new List<ServiceConfigView>();
        foreach (var service in services)
        {
            var config = configs.FirstOrDefault(c => c.ProjectServiceId == service.Id);
            views.Add(new ServiceConfigView
            {
                ProjectService = service,
                EnvironmentConfig = config,
                IsConfigured = config != null,
                StatusText = config != null ? "已配置" : "未配置",
                StatusClass = config != null ? "text-success" : "text-muted",
                EnvironmentServiceConfigId = config != null ? service.Id : null
            });
        }

        return new(views);
    }
}
