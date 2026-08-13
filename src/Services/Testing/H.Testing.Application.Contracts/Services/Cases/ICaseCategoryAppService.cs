using H.Abp.Application.Contracts;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试用例分类服务接口
/// </summary>
public interface ICaseCategoryAppService : IAppService
{
    /// <summary>
    /// 根据项目 ID 获取分类列表
    /// </summary>
    Task<List<CaseCategoryDto>> GetByProjectIdAsync(long projectId);

    /// <summary>
    /// 创建新的测试用例分类
    /// </summary>
    Task<CaseCategoryDto> CreateAsync(CaseCategoryDto category);

    /// <summary>
    /// 更新测试用例分类
    /// </summary>
    Task<bool> UpdateAsync(long id, CaseCategoryDto category);

    /// <summary>
    /// 删除测试用例分类
    /// </summary>
    Task<bool> DeleteAsync(long projectId, long id);

    /// <summary>
    /// 获取树形结构的分类列表
    /// </summary>
    Task<List<CaseCategoryDto>> GetTreeStructureAsync(long projectId);
}
