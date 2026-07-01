using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace H.AutoTest.Application.Contracts;

/// <summary>
/// 测试用例分类服务接口
/// </summary>
public interface IProjectCaseCategoryAppService : IApplicationService
{
    /// <summary>
    /// 根据项目 ID 获取分类列表
    /// </summary>
    Task<List<ProjectCaseCategory>> GetByProjectIdAsync(string projectId);
    
    /// <summary>
    /// 创建新的测试用例分类
    /// </summary>
    Task<ProjectCaseCategory> CreateAsync(ProjectCaseCategory category);
    
    /// <summary>
    /// 更新测试用例分类
    /// </summary>
    Task<bool> UpdateAsync(string id, ProjectCaseCategory category);
    
    /// <summary>
    /// 删除测试用例分类
    /// </summary>
    Task<bool> DeleteAsync(string projectId, string id);
    
    /// <summary>
    /// 获取树形结构的分类列表
    /// </summary>
    Task<List<ProjectCaseCategory>> GetTreeStructureAsync(string projectId);
}
