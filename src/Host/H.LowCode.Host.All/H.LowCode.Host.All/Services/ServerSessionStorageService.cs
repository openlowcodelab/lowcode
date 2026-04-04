using H.LowCode.ComponentBase;
using Microsoft.AspNetCore.Http;

namespace H.LowCode.Host.All.Services;

/// <summary>
/// 服务端 Session 存储服务实现
/// </summary>
public class ServerSessionStorageService : ISessionStorageService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServerSessionStorageService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task SetAsync(string key, string value)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Session != null)
        {
            httpContext.Session.SetString(key, value);
        }
    }

    public async Task<string?> GetAsync(string key)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Session != null)
        {
            var value = httpContext.Session.GetString(key);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
        }
        return null;
    }

    public async Task RemoveAsync(string key)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Session != null)
        {
            httpContext.Session.Remove(key);
        }
    }

    public async Task ClearAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Session != null)
        {
            httpContext.Session.Clear();
        }
    }
}
