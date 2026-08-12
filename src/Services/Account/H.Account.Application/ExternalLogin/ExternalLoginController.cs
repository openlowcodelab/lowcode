using H.Account.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace H.Account.Application;

/// <summary>
/// 外部登录 Controller（处理微信/钉钉扫码登录的跳转和回调）
/// </summary>
[Route("api/external-login")]
[AllowAnonymous]
public class ExternalLoginController : Controller
{
    private readonly IExternalLoginAppService _externalLoginService;
    private readonly WeChatAuthService _wechatAuth;
    private readonly DingTalkAuthService _dingtalkAuth;
    private readonly ExternalLoginOptions _options;

    private const string StateCookieName = "ext_login_state";

    public ExternalLoginController(
        IExternalLoginAppService externalLoginService,
        WeChatAuthService wechatAuth,
        DingTalkAuthService dingtalkAuth,
        IOptions<ExternalLoginOptions> options)
    {
        _externalLoginService = externalLoginService;
        _wechatAuth = wechatAuth;
        _dingtalkAuth = dingtalkAuth;
        _options = options.Value;
    }

    /// <summary>
    /// 发起外部登录跳转
    /// 1. 生成 state + 写入临时 Cookie
    /// 2. 构建授权 URL 并 302 跳转到微信/钉钉授权页
    /// </summary>
    [HttpGet("challenge")]
    public IActionResult Challenge(string provider, string? returnUrl)
    {
        // 验证 provider 是否已启用
        var isEnabled = provider switch
        {
            "WeChat" => _options.WeChat.Enabled,
            "DingTalk" => _options.DingTalk.Enabled,
            _ => false
        };

        if (!isEnabled)
        {
            return Redirect($"/account/login?error={Uri.EscapeDataString($"{provider}登录未启用")}");
        }

        // 生成 state 并写入 Cookie
        var state = Guid.NewGuid().ToString("N");
        var stateData = new ExternalLoginState { State = state, Provider = provider, ReturnUrl = returnUrl };

        Response.Cookies.Append(StateCookieName, JsonSerializer.Serialize(stateData), new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(5)
        });

        // 构建回调 URL
        var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/external-login/callback";

        // 构建授权 URL
        var authUrl = provider switch
        {
            "WeChat" => _wechatAuth.BuildAuthorizationUrl(state, callbackUrl),
            "DingTalk" => _dingtalkAuth.BuildAuthorizationUrl(state, callbackUrl),
            _ => throw new ArgumentException($"不支持的登录提供者: {provider}")
        };

        return Redirect(authUrl);
    }

    /// <summary>
    /// 处理外部登录回调
    /// 1. 验证 state
    /// 2. 换取用户信息
    /// 3. 调用 ExternalLoginAppService 处理登录
    /// 4. 重定向回 Blazor 应用
    /// </summary>
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string? code, string? authCode, string? state)
    {
        // 从 Cookie 读取 state 数据
        var cookieValue = Request.Cookies[StateCookieName];
        Response.Cookies.Delete(StateCookieName);

        if (string.IsNullOrEmpty(cookieValue))
        {
            return Redirect("/account/login?error=" + Uri.EscapeDataString("登录状态已过期，请重试"));
        }

        ExternalLoginState? stateData;
        try
        {
            stateData = JsonSerializer.Deserialize<ExternalLoginState>(cookieValue);
        }
        catch
        {
            return Redirect("/account/login?error=" + Uri.EscapeDataString("登录状态数据无效"));
        }

        if (stateData == null || state != stateData.State)
        {
            return Redirect("/account/login?error=" + Uri.EscapeDataString("安全验证失败，请重试"));
        }

        // 微信使用 code 参数，钉钉新版使用 authCode 参数
        var authCodeValue = code ?? authCode;
        if (string.IsNullOrEmpty(authCodeValue))
        {
            return Redirect("/account/login?error=" + Uri.EscapeDataString("未收到授权码"));
        }

        try
        {
            // 根据 provider 获取用户信息
            ExternalLoginRequestDto loginRequest;

            switch (stateData.Provider)
            {
                case "WeChat":
                    var tokenResp = await _wechatAuth.GetAccessTokenAsync(authCodeValue);
                    if (tokenResp?.ErrCode is not null and not 0)
                    {
                        return Redirect($"/account/login?error={Uri.EscapeDataString($"微信授权失败: {tokenResp.ErrMsg}")}");
                    }
                    if (string.IsNullOrEmpty(tokenResp?.AccessToken) || string.IsNullOrEmpty(tokenResp?.OpenId))
                    {
                        return Redirect("/account/login?error=" + Uri.EscapeDataString("微信授权信息不完整"));
                    }

                    var wechatUser = await _wechatAuth.GetUserInfoAsync(tokenResp.AccessToken, tokenResp.OpenId);
                    loginRequest = new ExternalLoginRequestDto
                    {
                        Provider = "WeChat",
                        ProviderKey = tokenResp.OpenId,
                        DisplayName = wechatUser?.Nickname,
                        AvatarUrl = wechatUser?.HeadImgUrl,
                        UnionId = tokenResp.UnionId ?? wechatUser?.UnionId
                    };
                    break;

                case "DingTalk":
                    var dingtalkUser = await _dingtalkAuth.GetUserInfoByCodeAsync(authCodeValue);
                    if (dingtalkUser == null || string.IsNullOrEmpty(dingtalkUser.OpenId))
                    {
                        return Redirect("/account/login?error=" + Uri.EscapeDataString("钉钉授权失败，请重试"));
                    }

                    loginRequest = new ExternalLoginRequestDto
                    {
                        Provider = "DingTalk",
                        ProviderKey = dingtalkUser.OpenId,
                        DisplayName = dingtalkUser.Nick,
                        AvatarUrl = dingtalkUser.AvatarUrl,
                        UnionId = dingtalkUser.UnionId
                    };
                    break;

                default:
                    return Redirect($"/account/login?error={Uri.EscapeDataString($"不支持的登录提供者: {stateData.Provider}")}");
            }

            // 调用应用服务处理登录
            var result = await _externalLoginService.ExternalLoginAsync(loginRequest);

            if (result.Success)
            {
                var returnUrl = stateData.ReturnUrl ?? "/account/external-callback";
                return Redirect($"{returnUrl}?success=true&isNewUser={result.IsNewUser}");
            }
            else
            {
                return Redirect($"/account/login?error={Uri.EscapeDataString(result.Message)}");
            }
        }
        catch (Exception ex)
        {
            return Redirect($"/account/login?error={Uri.EscapeDataString($"登录处理失败: {ex.Message}")}");
        }
    }

    /// <summary>
    /// 存储在 Cookie 中的登录状态
    /// </summary>
    private class ExternalLoginState
    {
        public string State { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
    }
}
