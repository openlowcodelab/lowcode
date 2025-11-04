using System;

namespace H.LowCode.Application.Contracts;

/// <summary>
/// 应用上下文服务接口
/// 用于在请求生命周期中管理当前应用的 AppId
/// </summary>
public interface IAppContextService
{
    /// <summary>
    /// 获取当前请求的 AppId
    /// </summary>
    string? CurrentAppId { get; }

    /// <summary>
    /// 设置当前请求的 AppId
    /// </summary>
    /// <param name="appId">应用ID</param>
    void SetAppId(string? appId);

    /// <summary>
    /// 从 HTTP 上下文中自动解析并设置 AppId
    /// </summary>
    void ResolveAppIdFromContext();
}