using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Web;

namespace H.Abp.HttpClientProxy;

/// <summary>
/// 基于 DispatchProxy 的 HTTP 客户端代理，拦截接口方法调用并转换为 HTTP 请求
/// </summary>
public class HttpClientProxyInterceptor<TService> : DispatchProxy where TService : class
{
    private HttpClient _httpClient = null!;
    private string _baseUrl = string.Empty;
    private string _controllerName = string.Empty;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    internal void Initialize(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl.TrimEnd('/');
        _controllerName = AbpUrlConvention.GetControllerName(typeof(TService));
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod == null)
            throw new InvalidOperationException("Target method is null.");

        var (httpMethod, actionPath) = AbpUrlConvention.GetActionInfo(targetMethod.Name);
        var parameters = targetMethod.GetParameters();

        // 构建 URL
        var url = BuildUrl(httpMethod, actionPath, parameters, args);

        // 创建请求
        var request = new HttpRequestMessage(httpMethod, url);

        // 如果是 POST/PUT 且有复杂类型参数，放到 body
        if ((httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put) && args != null && args.Length > 0)
        {
            var bodyParam = FindBodyParameter(parameters, args);
            if (bodyParam != null)
            {
                request.Content = JsonContent.Create(bodyParam, bodyParam.GetType(), options: JsonOptions);
            }
        }

        // 执行请求并返回 Task<T>
        var returnType = targetMethod.ReturnType;
        if (returnType == typeof(Task))
        {
            return SendAsync(request);
        }
        else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var resultType = returnType.GetGenericArguments()[0];
            var method = typeof(HttpClientProxyInterceptor<TService>)
                .GetMethod(nameof(SendWithResultAsync), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(resultType);
            return method.Invoke(this, [request]);
        }

        throw new NotSupportedException($"Return type {returnType} is not supported. Only Task and Task<T> are supported.");
    }

    private string BuildUrl(HttpMethod httpMethod, string actionPath, ParameterInfo[] parameters, object?[]? args)
    {
        var path = $"{_baseUrl}/api/app/{_controllerName}";
        var usedInPath = new HashSet<int>();

        // ABP 约定: 名为 "id" 的参数作为路径段，位于 action 之前
        int idIndex = Array.FindIndex(parameters, p => string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase) && IsSimpleType(p.ParameterType));
        if (idIndex >= 0 && args?[idIndex] != null)
        {
            path += $"/{Uri.EscapeDataString(args[idIndex]!.ToString()!)}";
            usedInPath.Add(idIndex);
        }

        if (!string.IsNullOrEmpty(actionPath))
        {
            path += $"/{actionPath}";

            // ABP 约定: 恰好一个以 "Id" 结尾的参数（非 "id" 本身）作为路径段，位于 action 之后
            var secondaryIdIndexes = new List<int>();
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i == idIndex) continue;
                if (parameters[i].Name!.EndsWith("Id", StringComparison.Ordinal) && IsSimpleType(parameters[i].ParameterType))
                    secondaryIdIndexes.Add(i);
            }
            if (secondaryIdIndexes.Count == 1 && args?[secondaryIdIndexes[0]] != null)
            {
                path += $"/{Uri.EscapeDataString(args[secondaryIdIndexes[0]]!.ToString()!)}";
                usedInPath.Add(secondaryIdIndexes[0]);
            }
        }

        // 剩余简单参数进查询字符串；GET/DELETE 的复杂参数展开属性到查询参数
        if (args == null || args.Length == 0)
            return path;

        var queryParams = new List<string>();
        for (int i = 0; i < parameters.Length; i++)
        {
            if (usedInPath.Contains(i)) continue;

            var param = parameters[i];
            var value = args[i];
            if (value == null) continue;

            if (IsSimpleType(param.ParameterType))
            {
                queryParams.Add($"{ToCamelCase(param.Name!)}={HttpUtility.UrlEncode(FormatValue(value))}");
            }
            else if (httpMethod == HttpMethod.Get || httpMethod == HttpMethod.Delete)
            {
                AppendComplexTypeAsQuery(queryParams, value, string.Empty);
            }
        }

        if (queryParams.Count > 0)
            path += "?" + string.Join("&", queryParams);

        return path;
    }

    private static string FormatValue(object value)
    {
        return value switch
        {
            DateTime dt => dt.ToString("O"),
            DateTimeOffset dto => dto.ToString("O"),
            bool b => b ? "true" : "false",
            _ => value.ToString() ?? string.Empty
        };
    }

    private static void AppendComplexTypeAsQuery(List<string> queryParams, object value, string prefix)
    {
        var properties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            var propValue = prop.GetValue(value);
            if (propValue == null) continue;

            var key = string.IsNullOrEmpty(prefix) ? ToCamelCase(prop.Name) : $"{prefix}.{ToCamelCase(prop.Name)}";

            if (IsSimpleType(prop.PropertyType))
            {
                queryParams.Add($"{key}={HttpUtility.UrlEncode(propValue.ToString())}");
            }
        }
    }

    private static object? FindBodyParameter(ParameterInfo[] parameters, object?[] args)
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            if (!IsSimpleType(parameters[i].ParameterType) && args[i] != null)
                return args[i];
        }
        return null;
    }

    private static bool IsSimpleType(Type type)
    {
        var t = Nullable.GetUnderlyingType(type) ?? type;
        return t.IsPrimitive
            || t == typeof(string)
            || t == typeof(decimal)
            || t == typeof(DateTime)
            || t == typeof(DateTimeOffset)
            || t == typeof(TimeSpan)
            || t == typeof(Guid)
            || t.IsEnum;
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
            return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private async Task SendAsync(HttpRequestMessage request)
    {
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<T?> SendWithResultAsync<T>(HttpRequestMessage request)
    {
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return default;

        // 空响应体（如服务端返回空字符串）无法 JSON 反序列化，按类型返回默认值
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(content))
        {
            if (typeof(T) == typeof(string))
                return (T)(object)string.Empty;
            return default;
        }

        // 服务端对 string 返回值以纯文本输出（非 JSON），直接返回原文
        if (typeof(T) == typeof(string) &&
            response.Content.Headers.ContentType?.MediaType?.StartsWith("text/", StringComparison.OrdinalIgnoreCase) == true)
        {
            return (T)(object)content;
        }

        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    /// <summary>
    /// 创建代理实例的工厂方法
    /// </summary>
    public static TService Create(HttpClient httpClient, string baseUrl)
    {
        var proxy = Create<TService, HttpClientProxyInterceptor<TService>>();
        var interceptor = (HttpClientProxyInterceptor<TService>)(object)proxy;
        interceptor.Initialize(httpClient, baseUrl);
        return proxy;
    }
}
