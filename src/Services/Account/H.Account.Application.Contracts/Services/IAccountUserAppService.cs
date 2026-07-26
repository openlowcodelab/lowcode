using H.Abp.Application.Contracts;

namespace H.Account.Application.Contracts;

/// <summary>
/// 企业级用户管理服务接口
/// 提供企业用户的查询、密码管理等功能
/// </summary>
public interface IAccountUserAppService : IAppService
{
    /// <summary>
    /// 根据用户ID获取用户信息
    /// </summary>
    Task<UserDto?> GetUserDtoByIdAsync(Guid userId);

    /// <summary>
    /// 分页查询用户列表
    /// </summary>
    Task<PagedResult<UserDto>> GetPagedUsersAsync(UserQueryParams queryParams);

    /// <summary>
    /// 重置密码
    /// </summary>
    Task ResetPasswordAsync(Guid userId, ResetPasswordDto dto);
}
