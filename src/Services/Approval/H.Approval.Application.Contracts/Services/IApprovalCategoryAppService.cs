using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Approval.Application.Contracts;

/// <summary>
/// 审批分类(分组)服务接口
/// </summary>
public interface IApprovalCategoryAppService : IAppService
{
    /// <summary>
    /// 获取所有分类
    /// </summary>
    Task<BaseOutput<List<ApprovalCategoryDto>>> GetAllAsync();

    /// <summary>
    /// 创建分类
    /// </summary>
    Task<BaseOutput<ApprovalCategoryDto>> CreateAsync(CreateApprovalCategoryDto input);

    /// <summary>
    /// 重命名分类(同步更新引用该分类的审批定义)
    /// </summary>
    Task<BaseOutput<ApprovalCategoryDto>> RenameAsync(RenameApprovalCategoryDto input);

    /// <summary>
    /// 删除分类(将引用该分类的审批定义归入未分类)
    /// </summary>
    Task<BaseOutput> DeleteAsync(string id);
}
