using H.Abp.Application.Contracts;

namespace H.Enterprise.Application.Contracts;

/// <summary>
/// 企业用户管理服务接口
/// </summary>
public interface IEnterpriseUserAppService : IAppService
{
    /// <summary>
    /// 获取企业的用户列表
    /// </summary>
    Task<List<EnterpriseUserDto>> GetEnterpriseUsersAsync(Guid enterpriseId);

    /// <summary>
    /// 添加用户到企业
    /// </summary>
    Task AddUserAsync(AddEnterpriseUserDto input);

    /// <summary>
    /// 从企业移除用户
    /// </summary>
    Task RemoveUserAsync(Guid enterpriseId, Guid userId);

    /// <summary>
    /// 设置默认企业（用户登录时自动选择）
    /// </summary>
    Task SetDefaultEnterpriseAsync(Guid enterpriseId);

    /// <summary>
    /// 更新用户在企业中的角色
    /// </summary>
    Task UpdateUserRoleAsync(Guid enterpriseId, Guid userId, string role);
}
