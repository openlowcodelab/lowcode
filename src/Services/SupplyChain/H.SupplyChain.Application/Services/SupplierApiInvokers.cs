using H.SupplyChain.Application.Contracts;
using System.Net.Http.Headers;
using System.Text.Json;

namespace H.SupplyChain.Application.Services;

/// <summary>
/// 基于 HTTP 协议的供应商接口调用实现。
/// 1. 按 RequestMappings 将标准 Input 映射为供应商请求体；
/// 2. 调用供应商接口（支持认证）；
/// 3. 按 ResponseMappings 从应答解析出标准字段。
/// 认证方式通过 <see cref="AuthTypeEnum"/> 选择，对应 AuthConfig 的 JSON 结构：
///  - ApiKey : { "keyName": "X-Api-Key", "value": "abc", "in": "header|query" }
///  - Header : { "headers": { "X-Foo": "bar" } }
///  - Basic  : { "username": "u", "password": "p" }
///  - Bearer : { "token": "xxx" }
/// 新增通信协议（MQ、gRPC 等）只需新增 <see cref="ISupplierApiInvoker"/> 实现。
/// </summary>
public class HttpSupplierApiInvoker : ISupplierApiInvoker
{
    private readonly IHttpClientFactory _httpClientFactory;
    public SupplierProtocolEnum Protocol => SupplierProtocolEnum.Http;

    public HttpSupplierApiInvoker(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<SupplierApiResponse> InvokeAsync(SupplierApiContext context, CancellationToken cancellationToken = default)
    {
        var supplier = context.Supplier;
        var mapping = context.Mapping;

        // 计算最终请求地址：映射覆盖优先，其次供应商默认 ApiUrl，再拼接接口路径
        var baseUrl = !string.IsNullOrWhiteSpace(mapping.SupplierApiUrl)
            ? mapping.SupplierApiUrl
            : supplier.ApiUrl;

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return SupplierApiResponse.Fail(null, null, "供应商未配置 ApiUrl");
        }

        // 按请求参数映射构造请求体
        var requestBody = BuildRequestPayload(context);
        var fullUrl = !string.IsNullOrWhiteSpace(context.Interface.Path)
            ? baseUrl.TrimEnd('/') + "/" + context.Interface.Path.TrimStart('/')
            : baseUrl;

        var client = _httpClientFactory.CreateClient("H.SupplyChain.HttpSupplier");
        var method = string.IsNullOrWhiteSpace(context.Interface.HttpMethod)
            ? "POST"
            : context.Interface.HttpMethod.ToUpperInvariant();
        var httpMethod = new HttpMethod(method);

        using var request = new HttpRequestMessage(httpMethod, fullUrl);

        var content = new StringContent(JsonSerializer.Serialize(requestBody));
        request.Content = content;

        ApplyAuth(request, supplier);
        var requestPayload = JsonSerializer.Serialize(requestBody);

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return SupplierApiResponse.Fail((int)response.StatusCode, body, $"HTTP {(int)response.StatusCode}");
            }

            // 按返回值字段映射解析应答
            var mapped = MapResponse(body, mapping.ResponseMappings);
            return SupplierApiResponse.Ok((int)response.StatusCode, body, mapped);
        }
        catch (Exception ex)
        {
            return SupplierApiResponse.Fail(null, null, ex.Message);
        }
        finally
        {
            request.Dispose();
        }
    }

    /// <summary>
    /// 按 RequestMappings 将标准 Input 映射为供应商请求体。
    /// SourceField 为标准字段，TargetField 为供应商侧字段。
    /// </summary>
    private static Dictionary<string, object?> BuildRequestPayload(SupplierApiContext context)
    {
        var payload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var mappings = context.Mapping.RequestMappings;

        foreach (var m in mappings)
        {
            var value = context.Input.TryGetValue(m.SourceField, out var v) && v is not null
                ? v
                : m.DefaultValue;

            if (value is null && m.IsRequired)
            {
                value = string.Empty;
            }

            var target = string.IsNullOrWhiteSpace(m.TargetField) ? m.SourceField : m.TargetField;
            payload[target] = value;
        }

        // 未配置映射的字段也透传，避免供应商接口缺失必要输入
        foreach (var kv in context.Input)
        {
            if (!payload.ContainsKey(kv.Key))
            {
                payload[kv.Key] = kv.Value;
            }
        }

        return payload;
    }

    /// <summary>
    /// 按 ResponseMappings 从供应商应答 JSON 中解析出标准字段。
    /// SourceField 为供应商侧字段（JSON 路径），TargetField 为标准字段。
    /// </summary>
    private static Dictionary<string, string?> MapResponse(string body, List<FieldMapping> responseMappings)
    {
        var result = new Dictionary<string, string?>();

        if (string.IsNullOrWhiteSpace(body) || responseMappings.Count == 0)
        {
            return result;
        }

        JsonElement root;
        try
        {
            root = JsonDocument.Parse(body).RootElement;
        }
        catch
        {
            return result;
        }

        foreach (var m in responseMappings)
        {
            var target = string.IsNullOrWhiteSpace(m.TargetField) ? m.SourceField : m.TargetField;
            var sourcePath = m.SourceField;

            var element = TryGetByPath(root, sourcePath);
            string? value = element.HasValue ? element.Value.ToString() : null;
            if (value is null && m.DefaultValue is not null)
            {
                value = m.DefaultValue;
            }

            result[target] = value;
        }

        return result;
    }

    /// <summary>支持简单 JSON 路径取值，如 "data.orderNo" 或 "orderNo"</summary>
    private static JsonElement? TryGetByPath(JsonElement element, string path)
    {
        var current = element;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(segment, out var child))
            {
                current = child;
            }
            else
            {
                return null;
            }
        }
        return current.ValueKind == JsonValueKind.Undefined ? null : current;
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
/// 模拟供应商接口调用：不实际调用外部接口，直接返回模拟应答，便于开发/演示环境。
/// </summary>
public class MockSupplierApiInvoker : ISupplierApiInvoker
{
    public SupplierProtocolEnum Protocol => SupplierProtocolEnum.Mock;

    public Task<SupplierApiResponse> InvokeAsync(SupplierApiContext context, CancellationToken cancellationToken = default)
    {
        var mockBody = JsonSerializer.Serialize(new
        {
            status = "mocked-success",
            receivedFields = context.Input,
            timestamp = DateTime.UtcNow
        });

        var mapped = new Dictionary<string, string?>();
        foreach (var m in context.Mapping.ResponseMappings)
        {
            var target = string.IsNullOrWhiteSpace(m.TargetField) ? m.SourceField : m.TargetField;
            mapped[target] = context.Input.TryGetValue(m.SourceField, out var v) ? v?.ToString() : m.DefaultValue;
        }

        // 下单场景补充供应商订单号
        mapped.TryAdd("supplierOrderNo", "MOCK-" + (context.Input.TryGetValue("externalOrderNo", out var eon) ? eon : "") + "-" + DateTime.UtcNow.Ticks);

        return Task.FromResult(SupplierApiResponse.Ok(200, mockBody, mapped));
    }
}