using Volo.Abp.Application.Services;

namespace H.Account.Application.Contracts;

/// <summary>
/// 系统角色管理服务接口
/// </summary>
public interface ISystemRoleAppService : IApplicationService
{
    /// <summary>
    /// 获取所有系统角色
    /// </summary>
    Task<List<SystemRoleDto>> GetRolesAsync();

    /// <summary>
    /// 创建自定义角色
    /// </summary>
    Task<SystemRoleDto> CreateRoleAsync(CreateSystemRoleDto dto);

    /// <summary>
    /// 删除自定义角色（内置角色不可删除）
    /// </summary>
    Task DeleteRoleAsync(Guid roleId);
}
