using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试环境服务接口
/// </summary>
public interface IEnvironmentAppService : IApplicationService
{
    /// <summary>
    /// 获取指定项目的所有环境
    /// </summary>
    Task<List<EnvironmentDto>> GetByProjectIdAsync(long projectId);
    
    /// <summary>
    /// 根据ID获取环境
    /// </summary>
    Task<EnvironmentDto?> GetByIdAsync(long projectId, long id);
    
    /// <summary>
    /// 创建新环境
    /// </summary>
    Task<EnvironmentDto> CreateAsync(EnvironmentDto environment);
    
    /// <summary>
    /// 更新环境
    /// </summary>
    Task<bool> UpdateAsync(long id, EnvironmentDto environment);
    
    /// <summary>
    /// 删除环境
    /// </summary>
    Task<bool> DeleteAsync(long projectId, long id);
}
