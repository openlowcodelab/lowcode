using H.Order.Application.Contracts;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace H.Order.Application.Services;

/// <summary>
/// 基于标准 HTTP 协议的供应商对接实现。
/// 支持的认证方式通过 <see cref="AuthTypeEnum"/> 选择，对应 AuthConfig 的 JSON 结构：
///  - ApiKey : { "keyName": "X-Api-Key", "value": "abc", "in": "header|query" }
///  - Header : { "headers": { "X-Foo": "bar" } }
///  - Basic  : { "username": "u", "password": "p" }
///  - Bearer : { "token": "xxx" }
/// 新增通信协议（MQ、gRPC 等）只需新增 <see cref="ISupplierClient"/> 实现。
/// </summary>
public class HttpSupplierClient : ISupplierClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    public SupplierProtocolEnum Protocol => SupplierProtocolEnum.Http;

    public HttpSupplierClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<SupplierResponse> SendAsync(SupplierContext context, CancellationToken cancellationToken = default)
    {
        var supplier = context.Supplier;
        var payload = context.Payload;

        if (string.IsNullOrWhiteSpace(supplier.ApiUrl))
        {
            return SupplierResponse.Fail(null, null, "供应商未配置 ApiUrl");
        }

        var client = _httpClientFactory.CreateClient("H.Order.HttpSupplier");
        using var request = new HttpRequestMessage(HttpMethod.Post, supplier.ApiUrl) { Content = JsonContent.Create(payload) };

        ApplyAuth(request, supplier);
        var requestPayload = JsonSerializer.Serialize(payload);

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            var responseTime = DateTime.UtcNow;
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return SupplierResponse.Ok((int)response.StatusCode, body);
            }

            return SupplierResponse.Fail((int)response.StatusCode, body, $"HTTP {(int)response.StatusCode}");
        }
        catch (Exception ex)
        {
            return SupplierResponse.Fail(null, null, ex.Message);
        }
        finally
        {
            request.Dispose();
        }
    }

    private static void ApplyAuth(HttpRequestMessage request, SupplierInfo supplier)
    {
        if (supplier.AuthType == AuthTypeEnum.None || string.IsNullOrWhiteSpace(supplier.AuthConfig))
        {
            return;
        }

        var config = JsonDocument.Parse(supplier.AuthConfig).RootElement;

        switch (supplier.AuthType)
        {
            case AuthTypeEnum.ApiKey:
                {
                    var keyName = config.TryGetProperty("keyName", out var kn) ? kn.GetString() : "X-Api-Key";
                    var val = config.TryGetProperty("value", out var v) ? v.GetString() : string.Empty;
                    var place = config.TryGetProperty("in", out var p) ? p.GetString() : "header";
                    if (string.IsNullOrEmpty(val)) break;
                    if (place == "query" && request.RequestUri is not null)
                    {
                        var ub = new UriBuilder(request.RequestUri);
                        var query = string.IsNullOrEmpty(ub.Query) ? "" : ub.Query.TrimStart('?') + "&";
                        query += $"{keyName}={Uri.EscapeDataString(val)}";
                        ub.Query = query;
                        request.RequestUri = ub.Uri;
                    }
                    else
                    {
                        request.Headers.TryAddWithoutValidation(keyName, val);
                    }
                    break;
                }
            case AuthTypeEnum.Header:
                {
                    if (config.TryGetProperty("headers", out var headersEl) && headersEl.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in headersEl.EnumerateObject())
                        {
                            request.Headers.TryAddWithoutValidation(prop.Name, prop.Value.GetString());
                        }
                    }
                    break;
                }
            case AuthTypeEnum.Basic:
                {
                    var user = config.TryGetProperty("username", out var u) ? u.GetString() : null;
                    var pass = config.TryGetProperty("password", out var pw) ? pw.GetString() : null;
                    if (!string.IsNullOrEmpty(user))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue(
                            "Basic", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{user}:{pass}")));
                    }
                    break;
                }
            case AuthTypeEnum.Bearer:
                {
                    var token = config.TryGetProperty("token", out var t) ? t.GetString() : null;
                    if (!string.IsNullOrEmpty(token))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                    break;
                }
        }
    }
}

/// <summary>
/// 模拟供应商对接：不实际调用外部接口，直接返回成功响应，便于开发/演示环境。
/// </summary>
public class MockSupplierClient : ISupplierClient
{
    public SupplierProtocolEnum Protocol => SupplierProtocolEnum.Mock;

    public Task<SupplierResponse> SendAsync(SupplierContext context, CancellationToken cancellationToken = default)
    {
        var mock = new
        {
            status = "mocked-success",
            receivedOrderNo = context.Payload.OrderNo,
            receivedAmount = context.Payload.TotalAmount,
            timestamp = DateTime.UtcNow
        };
        return Task.FromResult(SupplierResponse.Ok(200, JsonSerializer.Serialize(mock)));
    }
}