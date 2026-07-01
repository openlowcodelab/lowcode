namespace H.Account.Application;

/// <summary>
/// 外部登录配置选项（对应 appsettings.json 的 "ExternalLogin" 节）
/// </summary>
public class ExternalLoginOptions
{
    public ProviderOptions WeChat { get; set; } = new();
    public ProviderOptions DingTalk { get; set; } = new();
}

/// <summary>
/// 单个第三方登录提供者配置
/// </summary>
public class ProviderOptions
{
    public bool Enabled { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string CallbackPath { get; set; } = string.Empty;
}
