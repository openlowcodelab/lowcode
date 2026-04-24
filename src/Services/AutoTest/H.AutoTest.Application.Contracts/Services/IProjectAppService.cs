using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace H.AutoTest.Application.Contracts;

/// <summary>
/// 项目服务接口
/// </summary>
public interface IProjectAppService : IApplicationService
{
    /// <summary>
    /// 获取所有项目
    /// </summary>
    Task<List<ProjectDto>> GetAllAsync();
    
    /// <summary>
    /// 根据ID获取项目
    /// </summary>
    Task<ProjectDto?> GetByIdAsync(string id);
    
    /// <summary>
    /// 创建项目
    /// </summary>
    Task<string> CreateAsync(ProjectDto project);
    
    /// <summary>
    /// 创建项目(支持自定义ID或自动生成)
    /// </summary>
    Task<string> CreateProjectAsync(ProjectDto project);
    
    /// <summary>
    /// 更新项目
    /// </summary>
    Task<bool> UpdateAsync(string id, ProjectDto project);
    
    /// <summary>
    /// 删除项目及其相关的所有数据
    /// </summary>
    Task<bool> DeleteAsync(string id);
    
    /// <summary>
    /// 检查项目ID是否已存在
    /// </summary>
    Task<bool> IsIdExistsAsync(string id);
    
    /// <summary>
    /// 生成唯一的项目ID
    /// </summary>
    Task<string> GenerateUniqueIdAsync();
    
    /// <summary>
    /// 获取项目的环境列表
    /// </summary>
    Task<List<ProjectEnvironmentDto>> GetProjectEnvironmentsAsync(string projectId);
}
