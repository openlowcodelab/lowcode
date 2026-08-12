using H.Abp.Application.Contracts;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 项目服务配置管理服务接口
/// </summary>
public interface IProjectServiceConfigAppService : IAppService
{
    /// <summary>
    /// 获取项目的所有服务
    /// </summary>
    Task<List<ProjectServiceDto>> GetProjectServicesAsync(long projectId);

    /// <summary>
    /// 创建项目服务
    /// </summary>
    Task<ProjectServiceDto> CreateProjectServiceAsync(ProjectServiceDto service);

    /// <summary>
    /// 更新项目服务
    /// </summary>
    Task<ProjectServiceDto> UpdateProjectServiceAsync(long serviceId, ProjectServiceDto service);

    /// <summary>
    /// 删除项目服务
    /// </summary>
    Task DeleteProjectServiceAsync(long projectId, long serviceId);

    /// <summary>
    /// 获取环境的所有服务配置
    /// </summary>
    Task<List<ProjectEnvConfigDto>> GetEnvironmentServiceConfigsAsync(long environmentId);

    /// <summary>
    /// 获取单个环境服务配置
    /// </summary>
    Task<ProjectEnvConfigDto?> GetEnvironmentServiceConfigAsync(long configId);

    /// <summary>
    /// 更新环境服务配置
    /// </summary>
    Task<ProjectEnvConfigDto> UpdateEnvironmentServiceConfigAsync(ProjectEnvConfigDto config);

    /// <summary>
    /// 创建环境服务配置
    /// </summary>
    Task<ProjectEnvConfigDto> CreateEnvironmentServiceConfigAsync(ProjectEnvConfigDto config);

    /// <summary>
    /// 删除环境服务配置
    /// </summary>
    Task DeleteEnvironmentServiceConfigAsync(long configId);

    /// <summary>
    /// 获取服务配置视图（包含服务信息）
    /// </summary>
    Task<List<ServiceConfigView>> GetServiceConfigViewsAsync(long environmentId, long projectId);
}
