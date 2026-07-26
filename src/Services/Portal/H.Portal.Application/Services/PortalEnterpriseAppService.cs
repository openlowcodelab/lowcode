using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.Portal.Application;

/// <summary>
/// 门户企业网关服务：为门户前端提供创建企业、切换企业、我的企业列表等接口。
/// 通过 HTTP 调用系统级 Enterprise 服务（/api/app/enterprise/*）并转发认证 Cookie，
/// 与 System 层保持编译期零依赖（企业级应用与系统级应用代码逻辑完全隔离）。
/// 前端通过 HttpClient 调用 /api/app/portal-enterprise/*（ABP 约定路由）。
/// </summary>
[RemoteService]
[IgnoreAntiforgeryToken]
public class PortalEnterpriseAppService : ApplicationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public PortalEnterpriseAppService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 获取当前用户关联的所有企业列表
    /// GET /api/app/portal-enterprise/my-enterprises/{userId}
    /// </summary>
    public async Task<List<PortalEnterpriseDto>> GetMyEnterprisesAsync(Guid userId)
    {
        return await SendAsync<List<PortalEnterpriseDto>>(
            HttpMethod.Get, $"/api/app/enterprise/my-enterprises/{userId}") ?? [];
    }

    /// <summary>
    /// 创建企业（用户自行注册，待管理员激活）
    /// POST /api/app/portal-enterprise
    /// </summary>
    public async Task<PortalEnterpriseDto?> CreateAsync(PortalCreateEnterpriseDto input)
    {
        return await SendAsync<PortalEnterpriseDto>(
            HttpMethod.Post, "/api/app/enterprise", input);
    }

    /// <summary>
    /// 切换（选择）企业：Enterprise 服务会重新签发携带企业信息的认证 Cookie，
    /// 本网关将下游 Set-Cookie 回传给浏览器
    /// POST /api/app/portal-enterprise/select-enterprise/{enterpriseId}
    /// </summary>
    public async Task SelectEnterpriseAsync(Guid enterpriseId)
    {
        await SendAsync<object>(
            HttpMethod.Post, $"/api/app/enterprise/select-enterprise/{enterpriseId}");
    }

    /// <summary>
    /// 调用同宿主的 Enterprise API：转发请求 Cookie，回传响应 Set-Cookie
    /// </summary>
    private async Task<T?> SendAsync<T>(HttpMethod method, string path, object? body = null)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new UserFriendlyException("无法获取 HTTP 上下文");
        var request = httpContext.Request;

        using var message = new HttpRequestMessage(method, $"{request.Scheme}://{request.Host}{path}");

        // 转发认证 Cookie，使下游以当前用户身份执行
        if (request.Headers.TryGetValue("Cookie", out var cookieValues))
        {
            message.Headers.TryAddWithoutValidation("Cookie", cookieValues.ToArray());
        }

        if (body != null)
        {
            message.Content = JsonContent.Create(body, body.GetType(), options: JsonOptions);
        }

        var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(message);

        // 回传下游签发的 Cookie（切换企业时会重新签发认证 Cookie）
        if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
        {
            foreach (var value in setCookies)
            {
                httpContext.Response.Headers.Append("Set-Cookie", value);
            }
        }

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new UserFriendlyException(
                ExtractErrorMessage(content) ?? $"企业服务调用失败（HTTP {(int)response.StatusCode}）");
        }

        if (typeof(T) == typeof(object) || string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    /// <summary>
    /// 从 ABP 错误响应（{"error":{"message":...}}）或普通响应中提取错误消息
    /// </summary>
    private static string? ExtractErrorMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (doc.RootElement.TryGetProperty("error", out var error)
                && error.ValueKind == JsonValueKind.Object
                && error.TryGetProperty("message", out var errorMessage))
            {
                return errorMessage.GetString();
            }

            if (doc.RootElement.TryGetProperty("message", out var message))
            {
                return message.GetString();
            }
        }
        catch (JsonException)
        {
            // 非 JSON 响应，忽略
        }

        return null;
    }
}
