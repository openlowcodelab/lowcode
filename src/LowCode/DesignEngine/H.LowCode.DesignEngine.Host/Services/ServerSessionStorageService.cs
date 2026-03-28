using H.LowCode.ComponentBase;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Text;
using System.Text.Json;

namespace H.LowCode.DesignEngine.Host;

public class ServerSessionStorageService : ISessionStorageService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    //private readonly ProtectedSessionStorage _sessionStorage;

    public ServerSessionStorageService(
        //ProtectedSessionStorage sessionStorage,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        //_sessionStorage = sessionStorage;
    }

    public async Task SetAsync(string key, string value)
    {
        // 使用 ASP.NET Core 原生 Session
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Session != null)
        {
            httpContext.Session.SetString(key, value);
        }

        // 同时存储到 ProtectedSessionStorage（用于服务端持久化）
        //await _sessionStorage.SetAsync(key, value);
    }

    public async Task<string?> GetAsync(string key)
    {
        // 首先尝试从 ASP.NET Core Session 获取
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Session != null)
        {
            var value = httpContext.Session.GetString(key);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
        }

        // 如果 Session 中没有，尝试从 ProtectedSessionStorage 获取
        //try
        //{
        //    var result = await _sessionStorage.GetAsync<T>(key);
        //    return result.Success ? result.Value : default;
        //}
        //catch
        //{
        //    return default;
        //}
        return null;
    }

    public async Task RemoveAsync(string key)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Session != null)
        {
            httpContext.Session.Remove(key);
        }

        //await _sessionStorage.DeleteAsync(key);
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
