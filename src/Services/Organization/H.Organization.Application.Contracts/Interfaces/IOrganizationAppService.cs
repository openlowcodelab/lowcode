using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Organization.Application.Contracts;

/// <summary>
/// 部门服务接口
/// </summary>
public interface IOrganizationAppService : IAppService
{
    /// <summary>
    /// 获取所有部门（树形结构）
    /// </summary>
    Task<BaseOutput<List<OrganizationTreeDto>>> GetAllAsTreeAsync();

    /// <summary>
    /// 获取部门列表
    /// </summary>
    Task<BaseOutput<PagedResult<OrganizationDto>>> GetListAsync(OrganizationQueryParams queryParams);

    /// <summary>
    /// 获取部门详情
    /// </summary>
    Task<BaseOutput<OrganizationDto>> GetByIdAsync(Guid id);

    /// <summary>
    /// 创建部门
    /// </summary>
    Task<BaseOutput<OrganizationDto>> CreateAsync(CreateOrganizationDto input);

    /// <summary>
    /// 更新部门
    /// </summary>
    Task<BaseOutput<OrganizationDto>> UpdateAsync(Guid id, UpdateOrganizationDto input);

    /// <summary>
    /// 删除部门
    /// </summary>
    Task<BaseOutput> DeleteAsync(Guid id);

    /// <summary>
    /// 批量删除部门
    /// </summary>
    Task<BaseOutput> BatchDeleteAsync(List<Guid> ids);

    /// <summary>
    /// 获取部门用户（包含子部门用户）
    /// </summary>
    /// <param name="organizationId">部门ID</param>
    /// <param name="includeChildren">是否包含子部门用户</param>
    /// <returns>用户ID列表</returns>
    Task<BaseOutput<List<Guid>>> GetOrganizationUserIdsAsync(Guid organizationId, bool includeChildren = true);
}
