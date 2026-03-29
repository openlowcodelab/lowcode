using System;

namespace H.Organization.Application.Contracts;

/// <summary>
/// 角色服务接口
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// 获取角色列表
    /// </summary>
    Task<PagedResult<RoleDto>> GetListAsync(RoleQueryParams queryParams);

    /// <summary>
    /// 获取角色详情
    /// </summary>
    Task<RoleDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// 创建角色
    /// </summary>
    Task<RoleDto> CreateAsync(CreateRoleDto input);

    /// <summary>
    /// 更新角色
    /// </summary>
    Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto input);

    /// <summary>
    /// 删除角色
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// 批量删除角色
    /// </summary>
    Task BatchDeleteAsync(List<Guid> ids);

    /// <summary>
    /// 获取所有启用的角色
    /// </summary>
    Task<List<RoleDto>> GetAllEnabledAsync();
}
