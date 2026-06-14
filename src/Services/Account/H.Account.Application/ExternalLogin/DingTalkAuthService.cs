using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace H.Account.Application;

/// <summary>
/// 钉钉扫码登录 API 封装（新版 OAuth2）
/// </summary>
public class DingTalkAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ProviderOptions _options;

    public DingTalkAuthService(HttpClient httpClient, IOptions<ExternalLoginOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value.DingTalk;
    }

    /// <summary>
    /// 是否已启用
    /// </summary>
    public bool IsEnabled => _options.Enabled;

    /// <summary>
    /// 构建钉钉扫码授权 URL（新版 login.dingtalk.com，显示二维码扫描页面）
    /// </summary>
    public string BuildAuthorizationUrl(string state, string callbackUrl)
    {
        var encodedCallback = Uri.EscapeDataString(callbackUrl);
        return $"https://login.dingtalk.com/oauth2/auth" +
               $"?redirect_uri={encodedCallback}" +
               $"&response_type=code" +
               $"&client_id={_options.ClientId}" +
               $"&scope=openid" +
               $"&state={state}" +
               $"&prompt=consent";
    }

    /// <summary>
    /// 根据 authCode 获取用户信息（新版 OAuth2 流程）
    /// 1. 用 authCode 换取 accessToken
    /// 2. 用 accessToken 获取用户信息
    /// </summary>
    public async Task<DingTalkUserInfo?> GetUserInfoByCodeAsync(string authCode)
    {
        // 第一步：用 authCode 换取 accessToken
        var tokenUrl = "https://api.dingtalk.com/v1.0/oauth2/userAccessToken";
        var tokenBody = JsonSerializer.Serialize(new
        {
            clientId = _options.ClientId,
            clientSecret = _options.ClientSecret,
            code = authCode,
            grantType = "authorization_code"
        });

        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = new StringContent(tokenBody, Encoding.UTF8, "application/json")
        };

        var tokenResponse = await _httpClient.SendAsync(tokenRequest);
        var tokenText = await tokenResponse.Content.ReadAsStringAsync();

        if (!tokenResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var tokenResult = JsonSerializer.Deserialize<DingTalkTokenResponse>(tokenText, JsonOptions);
        if (string.IsNullOrEmpty(tokenResult?.AccessToken))
        {
            return null;
        }

        // 第二步：用 accessToken 获取用户信息
        var userInfoUrl = "https://api.dingtalk.com/v1.0/contact/users/me";
        var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, userInfoUrl);
        userInfoRequest.Headers.Add("x-acs-dingtalk-access-token", tokenResult.AccessToken);

        var userInfoResponse = await _httpClient.SendAsync(userInfoRequest);
        var userInfoText = await userInfoResponse.Content.ReadAsStringAsync();

        if (!userInfoResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var userInfo = JsonSerializer.Deserialize<DingTalkUserInfo>(userInfoText, JsonOptions);
        return userInfo;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}

/// <summary>
/// 钉钉 OAuth2 token 响应
/// </summary>
public class DingTalkTokenResponse
{
    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("refreshToken")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expireIn")]
    public int ExpireIn { get; set; }
}

/// <summary>
/// 钉钉用户信息（新版 API）
/// </summary>
public class DingTalkUserInfo
{
    [JsonPropertyName("nick")]
    public string? Nick { get; set; }

    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("mobile")]
    public string? Mobile { get; set; }

    [JsonPropertyName("openId")]
    public string? OpenId { get; set; }

    [JsonPropertyName("unionId")]
    public string? UnionId { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}
