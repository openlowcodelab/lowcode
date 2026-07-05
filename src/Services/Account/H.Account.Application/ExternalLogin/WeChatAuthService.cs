using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace H.Account.Application;

/// <summary>
/// 微信开放平台扫码登录 API 封装
/// </summary>
public class WeChatAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ProviderOptions _options;

    public WeChatAuthService(HttpClient httpClient, IOptions<ExternalLoginOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value.WeChat;
    }

    /// <summary>
    /// 是否已启用
    /// </summary>
    public bool IsEnabled => _options.Enabled;

    /// <summary>
    /// 构建微信开放平台扫码授权 URL
    /// 注意：URL 末尾必须以 #wechat_redirect 结尾
    /// </summary>
    public string BuildAuthorizationUrl(string state, string callbackUrl)
    {
        var encodedCallback = Uri.EscapeDataString(callbackUrl);
        return $"https://open.weixin.qq.com/connect/qrconnect" +
               $"?appid={_options.ClientId}" +
               $"&redirect_uri={encodedCallback}" +
               $"&response_type=code" +
               $"&scope=snsapi_login" +
               $"&state={state}" +
               $"#wechat_redirect";
    }

    /// <summary>
    /// 根据授权 code 换取 access_token 和 openid
    /// </summary>
    public async Task<WeChatTokenResponse?> GetAccessTokenAsync(string code)
    {
        var url = $"https://api.weixin.qq.com/sns/oauth2/access_token" +
                  $"?appid={_options.ClientId}" +
                  $"&secret={_options.ClientSecret}" +
                  $"&code={code}" +
                  $"&grant_type=authorization_code";

        var response = await _httpClient.GetStringAsync(url);
        return JsonSerializer.Deserialize<WeChatTokenResponse>(response, JsonOptions);
    }

    /// <summary>
    /// 根据 access_token 和 openid 获取用户信息
    /// </summary>
    public async Task<WeChatUserInfo?> GetUserInfoAsync(string accessToken, string openId)
    {
        var url = $"https://api.weixin.qq.com/sns/userinfo" +
                  $"?access_token={accessToken}" +
                  $"&openid={openId}" +
                  $"&lang=zh_CN";

        var response = await _httpClient.GetStringAsync(url);
        return JsonSerializer.Deserialize<WeChatUserInfo>(response, JsonOptions);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}

/// <summary>
/// 微信 access_token 响应
/// </summary>
public class WeChatTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("openid")]
    public string? OpenId { get; set; }

    [JsonPropertyName("unionid")]
    public string? UnionId { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("errcode")]
    public int? ErrCode { get; set; }

    [JsonPropertyName("errmsg")]
    public string? ErrMsg { get; set; }
}

/// <summary>
/// 微信用户信息
/// </summary>
public class WeChatUserInfo
{
    [JsonPropertyName("openid")]
    public string? OpenId { get; set; }

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("headimgurl")]
    public string? HeadImgUrl { get; set; }

    [JsonPropertyName("unionid")]
    public string? UnionId { get; set; }
}
