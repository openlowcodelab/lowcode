using H.Abstractions;

namespace H.Organization.Application.Contracts;

/// <summary>
/// 组织邀请服务接口
/// </summary>
public interface IOrgInviteAppService : IAppService
{
    /// <summary>
    /// 创建邀请（生成令牌；若填写手机号则发送短信邀请链接）
    /// </summary>
    Task<InviteDto> CreateInviteAsync(CreateInviteDto input);

    /// <summary>
    /// 获取邀请信息（用于确认加入页，校验令牌有效性）
    /// </summary>
    Task<InviteInfoDto> GetInviteInfoAsync(string token);

    /// <summary>
    /// 接受邀请（当前登录用户加入组织，消费令牌）
    /// </summary>
    Task AcceptInviteAsync(string token);
}
