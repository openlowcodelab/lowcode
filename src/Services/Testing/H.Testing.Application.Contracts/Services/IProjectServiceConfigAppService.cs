using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 项目服务配置管理服务接口
/// </summary>
public interface IProjectServiceConfigAppService : IAppService
{
    /// <summary>
    /// 获取项目的所有服务
    /// </summary>
    Task<BaseOutput<List<ProjectServiceDto>>> GetProjectServicesAsync(long projectId);

    /// <summary>
    /// 创建项目服务
    /// </summary>
    Task<BaseOutput<ProjectServiceDto>> CreateProjectServiceAsync(ProjectServiceDto service);

    /// <summary>
    /// 更新项目服务
    /// </summary>
    Task<BaseOutput<ProjectServiceDto>> UpdateProjectServiceAsync(long serviceId, ProjectServiceDto service);

    /// <summary>
    /// 删除项目服务
    /// </summary>
    Task<BaseOutput> DeleteProjectServiceAsync(long projectId, long serviceId);

    /// <summary>
    /// 获取环境的所有服务配置
    /// </summary>
    Task<BaseOutput<List<ProjectEnvConfigDto>>> GetEnvironmentServiceConfigsAsync(long environmentId);

    /// <summary>
    /// 更新环境服务配置（同一 环境+服务 已存在时覆盖）
    /// </summary>
    Task<BaseOutput<ProjectEnvConfigDto>> UpdateEnvironmentServiceConfigAsync(ProjectEnvConfigDto config);

    /// <summary>
    /// 创建环境服务配置（同一 环境+服务 已存在时覆盖）
    /// </summary>
    Task<BaseOutput<ProjectEnvConfigDto>> CreateEnvironmentServiceConfigAsync(ProjectEnvConfigDto config);

    /// <summary>
    /// 删除环境服务配置
    /// </summary>
    Task<BaseOutput> DeleteEnvironmentServiceConfigAsync(long environmentId, long projectServiceId);

    /// <summary>
    /// 获取服务配置视图（包含服务信息）
    /// </summary>
    Task<BaseOutput<List<ServiceConfigView>>> GetServiceConfigViewsAsync(long environmentId, long projectId);
}
