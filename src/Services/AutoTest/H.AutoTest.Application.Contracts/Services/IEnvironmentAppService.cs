using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace H.AutoTest.Application.Contracts;

/// <summary>
/// 测试环境服务接口
/// </summary>
public interface IEnvironmentAppService : IApplicationService
{
    /// <summary>
    /// 获取指定项目的所有环境
    /// </summary>
    Task<List<EnvironmentDto>> GetByProjectIdAsync(string projectId);
    
    /// <summary>
    /// 根据ID获取环境
    /// </summary>
    Task<EnvironmentDto?> GetByIdAsync(string projectId, string id);
    
    /// <summary>
    /// 创建新环境
    /// </summary>
    Task<EnvironmentDto> CreateAsync(EnvironmentDto environment);
    
    /// <summary>
    /// 更新环境
    /// </summary>
    Task<bool> UpdateAsync(string id, EnvironmentDto environment);
    
    /// <summary>
    /// 删除环境
    /// </summary>
    Task<bool> DeleteAsync(string projectId, string id);
}
