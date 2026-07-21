using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试用例服务接口
/// </summary>
public interface IProjectCaseAppService : IApplicationService
{
    /// <summary>
    /// 获取所有测试用例（所有项目）
    /// </summary>
    Task<List<ProjectCaseDto>> GetAllAsync();
    
    /// <summary>
    /// 根据项目 ID 获取测试用例
    /// </summary>
    Task<List<ProjectCaseDto>> GetByProjectIdAsync(long projectId);
    
    /// <summary>
    /// 根据 ID 获取测试用例
    /// </summary>
    Task<ProjectCaseDto?> GetByIdAsync(long id);
    
    /// <summary>
    /// 创建新的测试用例
    /// </summary>
    Task<long> CreateAsync(ProjectCaseDto projectCase);
    
    /// <summary>
    /// 更新测试用例
    /// </summary>
    Task<bool> UpdateAsync(long id, ProjectCaseDto projectCase);
    
    /// <summary>
    /// 删除测试用例
    /// </summary>
    Task<bool> DeleteAsync(long id);
    
    /// <summary>
    /// 获取激活状态的测试用例
    /// </summary>
    Task<List<ProjectCaseDto>> GetActiveProjectCasesAsync();
    
    /// <summary>
    /// 根据标签获取测试用例
    /// </summary>
    Task<List<ProjectCaseDto>> GetByTagsAsync(string tag);
    
    /// <summary>
    /// 根据级别获取测试用例
    /// </summary>
    Task<List<ProjectCaseDto>> GetByLevelAsync(string level);
    
    /// <summary>
    /// 搜索测试用例
    /// </summary>
    Task<List<ProjectCaseDto>> SearchAsync(string keyword);
    
    /// <summary>
    /// 复制测试用例
    /// </summary>
    Task<long> CopyAsync(long id);
}
