using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 项目环境服务接口
/// </summary>
public interface IProjectEnvironmentAppService : IApplicationService
{
    /// <summary>
    /// 获取所有项目环境（所有项目）
    /// </summary>
    Task<List<ProjectEnvironmentDto>> GetAllAsync();
    
    /// <summary>
    /// 根据项目ID获取环境列表
    /// </summary>
    Task<List<ProjectEnvironmentDto>> GetByProjectIdAsync(long projectId);
    
    /// <summary>
    /// 根据ID获取项目环境
    /// </summary>
    Task<ProjectEnvironmentDto?> GetByIdAsync(long id);
    
    /// <summary>
    /// 创建新的项目环境
    /// </summary>
    Task<long> CreateAsync(ProjectEnvironmentDto environment);
    
    /// <summary>
    /// 更新项目环境
    /// </summary>
    Task<bool> UpdateAsync(long id, ProjectEnvironmentDto environment);
    
    /// <summary>
    /// 删除项目环境
    /// </summary>
    Task<bool> DeleteAsync(long id);
    
    /// <summary>
    /// 根据类型获取环境
    /// </summary>
    Task<List<ProjectEnvironmentDto>> GetByTypeAsync(EnvironmentType type);
    
    /// <summary>
    /// 获取激活状态的环境
    /// </summary>
    Task<List<ProjectEnvironmentDto>> GetActiveEnvironmentsAsync();
    
    /// <summary>
    /// 获取项目下的活跃环境
    /// </summary>
    Task<List<ProjectEnvironmentDto>> GetActiveEnvironmentsByProjectAsync(long projectId);
}
