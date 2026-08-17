using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 任务分类管理服务接口
/// </summary>
public interface ICategoryAppService : IAppService
{
    /// <summary>
    /// 获取分类列表
    /// </summary>
    Task<BaseOutput<List<CategoryDto>>> GetListAsync();

    /// <summary>
    /// 创建分类
    /// </summary>
    Task<BaseOutput<CategoryDto>> CreateAsync(CreateCategoryDto input);

    /// <summary>
    /// 更新分类
    /// </summary>
    Task<BaseOutput<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto input);

    /// <summary>
    /// 删除分类
    /// </summary>
    Task<BaseOutput> DeleteAsync(Guid id);
}
