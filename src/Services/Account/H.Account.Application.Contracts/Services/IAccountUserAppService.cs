using H.Abp.Application.Contracts;
using H.Util.Base;

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
    Task<BaseOutput<UserDto?>> GetUserDtoByIdAsync(Guid userId);

    /// <summary>
    /// 分页查询用户列表
    /// </summary>
    Task<BaseOutput<PagedResult<UserDto>>> GetPagedUsersAsync(UserQueryParams queryParams);

    /// <summary>
    /// 重置密码
    /// </summary>
    Task<BaseOutput> ResetPasswordAsync(Guid userId, ResetPasswordDto dto);
}
