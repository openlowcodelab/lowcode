using H.Testing.Application.Contracts;
using H.Testing.Application.Mapping;
using H.Testing.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 项目服务配置管理服务（数据库存储）
/// </summary>
public class ProjectServiceConfigAppService : ApplicationService, IProjectServiceConfigAppService
{
    private readonly IRepository<ProjectServiceEntity, long> _serviceRepository;
    private readonly IRepository<ProjectEnvConfigEntity, long> _configRepository;

    public ProjectServiceConfigAppService(
        IRepository<ProjectServiceEntity, long> serviceRepository,
        IRepository<ProjectEnvConfigEntity, long> configRepository)
    {
        _serviceRepository = serviceRepository;
        _configRepository = configRepository;
    }

    public async Task<List<ProjectServiceDto>> GetProjectServicesAsync(long projectId)
    {
        var query = await _serviceRepository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.Where(s => s.ProjectId == projectId).OrderBy(s => s.Id));
        return list.Select(e => e.ToDto()).ToList();
    }

    public async Task<ProjectServiceDto> CreateProjectServiceAsync(ProjectServiceDto service)
    {
        var entity = new ProjectServiceEntity();
        service.Apply(entity);
        entity = await _serviceRepository.InsertAsync(entity, autoSave: true);
        return entity.ToDto();
    }

    public async Task<ProjectServiceDto> UpdateProjectServiceAsync(long serviceId, ProjectServiceDto service)
    {
        var entity = await _serviceRepository.FindAsync(serviceId);
        if (entity == null)
        {
            throw new ArgumentException($"Service with ID {serviceId} not found");
        }

        entity.Name = service.Name;
        entity.Description = service.Description;
        await _serviceRepository.UpdateAsync(entity, autoSave: true);
        return entity.ToDto();
    }

    public async Task DeleteProjectServiceAsync(long projectId, long serviceId)
    {
        await _serviceRepository.DeleteAsync(s => s.Id == serviceId, autoSave: true);
        // 删除相关的环境服务配置
        await _configRepository.DeleteAsync(c => c.ProjectServiceId == serviceId, autoSave: true);
    }

    public async Task<List<ProjectEnvConfigDto>> GetEnvironmentServiceConfigsAsync(long environmentId)
    {
        var query = await _configRepository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.Where(c => c.EnvId == environmentId));
        return list.Select(e => e.ToDto()).ToList();
    }

    public async Task<ProjectEnvConfigDto?> GetEnvironmentServiceConfigAsync(long configId)
    {
        var entity = await _configRepository.FindAsync(configId);
        return entity?.ToDto();
    }

    public async Task<ProjectEnvConfigDto> UpdateEnvironmentServiceConfigAsync(ProjectEnvConfigDto config)
    {
        var entity = config.Id > 0 ? await _configRepository.FindAsync(config.Id) : null;
        if (entity != null)
        {
            config.Apply(entity);
            await _configRepository.UpdateAsync(entity, autoSave: true);
            return entity.ToDto();
        }

        entity = new ProjectEnvConfigEntity();
        config.Apply(entity);
        entity = await _configRepository.InsertAsync(entity, autoSave: true);
        return entity.ToDto();
    }

    public async Task<ProjectEnvConfigDto> CreateEnvironmentServiceConfigAsync(ProjectEnvConfigDto config)
    {
        var entity = new ProjectEnvConfigEntity();
        config.Apply(entity);
        entity = await _configRepository.InsertAsync(entity, autoSave: true);
        return entity.ToDto();
    }

    public async Task DeleteEnvironmentServiceConfigAsync(long configId)
    {
        await _configRepository.DeleteAsync(c => c.Id == configId, autoSave: true);
    }

    public async Task<List<ServiceConfigView>> GetServiceConfigViewsAsync(long environmentId, long projectId)
    {
        var services = await GetProjectServicesAsync(projectId);
        var configs = await GetEnvironmentServiceConfigsAsync(environmentId);

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
                EnvironmentServiceConfigId = config?.Id
            });
        }

        return views;
    }
}
