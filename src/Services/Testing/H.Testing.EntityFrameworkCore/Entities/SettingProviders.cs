namespace H.Testing.EntityFrameworkCore;

/// <summary>
/// 设置提供者名称（参考 ABP SettingManagement 的 Provider：全局 / 租户 / 用户）
/// </summary>
public static class SettingProviders
{
    /// <summary>全局</summary>
    public const string Global = "G";

    /// <summary>租户</summary>
    public const string Tenant = "T";

    /// <summary>用户</summary>
    public const string User = "U";
}
