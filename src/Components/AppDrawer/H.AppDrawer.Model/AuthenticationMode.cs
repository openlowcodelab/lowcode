namespace H.AppDrawer.Components;

/// <summary>
/// 认证模式枚举
/// </summary>
public enum AuthenticationMode
{
    /// <summary>
    /// 不检测认证状态
    /// </summary>
    None,

    /// <summary>
    /// 检测认证状态，允许匿名访问（未登录不跳转，仅展示登录状态）
    /// </summary>
    Optional,

    /// <summary>
    /// 强制认证，未登录自动跳转登录页
    /// </summary>
    Required,

    /// <summary>
    /// 仅系统级用户（LoginMode=System）可访问
    /// </summary>
    SystemRequired,

    /// <summary>
    /// 仅企业级用户（无 LoginMode=System）可访问
    /// </summary>
    EnterpriseRequired
}
