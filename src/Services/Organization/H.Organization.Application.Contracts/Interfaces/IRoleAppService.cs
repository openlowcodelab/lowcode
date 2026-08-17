using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Organization.Application.Contracts;

/// <summary>
/// 角色服务接口
/// </summary>
public interface IRoleAppService : IAppService
{
    /// <summary>
    /// 获取角色列表
    /// </summary>
    Task<BaseOutput<PagedResult<RoleDto>>> GetListAsync(RoleQueryParams queryParams);

    /// <summary>
    /// 获取角色详情
    /// </summary>
    Task<BaseOutput<RoleDto>> GetByIdAsync(Guid id);

    /// <summary>
    /// 创建角色
    /// </summary>
    Task<BaseOutput<RoleDto>> CreateAsync(CreateRoleDto input);

    /// <summary>
    /// 更新角色
    /// </summary>
    Task<BaseOutput<RoleDto>> UpdateAsync(Guid id, UpdateRoleDto input);

    /// <summary>
    /// 删除角色
    /// </summary>
    Task<BaseOutput> DeleteAsync(Guid id);

    /// <summary>
    /// 批量删除角色
    /// </summary>
    Task<BaseOutput> BatchDeleteAsync(List<Guid> ids);

    /// <summary>
    /// 获取所有启用的角色
    /// </summary>
    Task<BaseOutput<List<RoleDto>>> GetAllEnabledAsync();

    /// <summary>
    /// 获取角色已分配成员
    /// </summary>
    Task<BaseOutput<List<RoleMemberDto>>> GetRoleMembersAsync(Guid roleId);
}
