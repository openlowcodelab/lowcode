using H.SystemPortal.Application.Contracts;
using Volo.Abp.Application.Services;

namespace H.Account.Application.Contracts;

/// <summary>
/// 外部登录应用服务接口
/// </summary>
public interface IExternalLoginAppService : IApplicationService
{
    /// <summary>
    /// 处理外部登录（查找绑定 → 自动注册/直接登录 → 写 Cookie）
    /// </summary>
    Task<ExternalLoginResultDto> ExternalLoginAsync(ExternalLoginRequestDto request);

    /// <summary>
    /// 将外部账号绑定到已登录用户
    /// </summary>
    Task<ExternalLoginResultDto> BindExternalAccountAsync(Guid userId, ExternalLoginRequestDto request);

    /// <summary>
    /// 解绑外部账号
    /// </summary>
    Task<bool> UnbindExternalAccountAsync(Guid userId, string provider);

    /// <summary>
    /// 获取用户已绑定的外部账号列表
    /// </summary>
    Task<List<ExternalAccountDto>> GetExternalAccountsAsync(Guid userId);
}
