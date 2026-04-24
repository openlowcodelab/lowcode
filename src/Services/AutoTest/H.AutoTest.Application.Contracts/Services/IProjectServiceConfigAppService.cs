using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace H.AutoTest.Application.Contracts;

/// <summary>
/// 项目服务配置管理服务接口
/// </summary>
public interface IProjectServiceConfigAppService : IApplicationService
{
    /// <summary>
    /// 获取项目的所有服务
    /// </summary>
    Task<List<ProjectServiceDto>> GetProjectServicesAsync(string projectId);
    
    /// <summary>
    /// 创建项目服务
    /// </summary>
    Task<ProjectServiceDto> CreateProjectServiceAsync(ProjectServiceDto service);
    
    /// <summary>
    /// 更新项目服务
    /// </summary>
    Task<ProjectServiceDto> UpdateProjectServiceAsync(string serviceId, ProjectServiceDto service);
    
    /// <summary>
    /// 删除项目服务
    /// </summary>
    Task DeleteProjectServiceAsync(string projectId, string serviceId);
    
    /// <summary>
    /// 获取环境的所有服务配置
    /// </summary>
    Task<List<EnvironmentServiceConfigDto>> GetEnvironmentServiceConfigsAsync(string environmentId);
    
    /// <summary>
    /// 获取单个环境服务配置
    /// </summary>
    Task<EnvironmentServiceConfigDto?> GetEnvironmentServiceConfigAsync(string configId);
    
    /// <summary>
    /// 更新环境服务配置
    /// </summary>
    Task<EnvironmentServiceConfigDto> UpdateEnvironmentServiceConfigAsync(EnvironmentServiceConfigDto config);
    
    /// <summary>
    /// 创建环境服务配置
    /// </summary>
    Task<EnvironmentServiceConfigDto> CreateEnvironmentServiceConfigAsync(EnvironmentServiceConfigDto config);
    
    /// <summary>
    /// 删除环境服务配置
    /// </summary>
    Task DeleteEnvironmentServiceConfigAsync(string configId);
    
    /// <summary>
    /// 获取服务配置视图（包含服务信息）
    /// </summary>
    Task<List<ServiceConfigView>> GetServiceConfigViewsAsync(string environmentId, string projectId);
}
