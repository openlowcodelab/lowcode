using H.Abp.Application.Contracts;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 任务分类管理服务接口
/// </summary>
public interface ICategoryAppService : IAppService
{
    /// <summary>
    /// 获取分类列表
    /// </summary>
    Task<List<CategoryDto>> GetListAsync();

    /// <summary>
    /// 创建分类
    /// </summary>
    Task<CategoryDto> CreateAsync(CreateCategoryDto input);

    /// <summary>
    /// 更新分类
    /// </summary>
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto input);

    /// <summary>
    /// 删除分类
    /// </summary>
    Task DeleteAsync(Guid id);
}
