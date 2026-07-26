using System.Text.RegularExpressions;

namespace H.Abp.HttpClientProxy;

/// <summary>
/// ABP URL 约定工具类，将接口名和方法名转换为 ABP 风格的 API URL
/// </summary>
internal static partial class AbpUrlConvention
{
    /// <summary>
    /// 从接口类型获取控制器名称（kebab-case）
    /// 例如: IPageAppService → page, IAppApplicationService → app-application
    /// </summary>
    public static string GetControllerName(Type serviceInterface)
    {
        var name = serviceInterface.Name;

        // 去掉 "I" 前缀
        if (name.StartsWith('I') && name.Length > 1 && char.IsUpper(name[1]))
            name = name[1..];

        // 去掉 "AppService" 或 "ApplicationService" 后缀
        if (name.EndsWith("ApplicationService"))
            name = name[..^"ApplicationService".Length];
        else if (name.EndsWith("AppService"))
            name = name[..^"AppService".Length];

        return ToKebabCase(name);
    }

    /// <summary>
    /// 从方法名获取 HTTP 动词和 action 路径（与 ABP HttpMethodHelper.ConventionalPrefixes 保持一致）
    /// </summary>
    public static (HttpMethod HttpMethod, string ActionPath) GetActionInfo(string methodName)
    {
        // 去掉 "Async" 后缀
        if (methodName.EndsWith("Async"))
            methodName = methodName[..^"Async".Length];

        // ABP 约定前缀映射（顺序重要：长前缀优先）
        (string Prefix, HttpMethod Method)[] prefixMap =
        [
            ("GetList", HttpMethod.Get),
            ("GetAll", HttpMethod.Get),
            ("Get", HttpMethod.Get),
            ("Put", HttpMethod.Put),
            ("Update", HttpMethod.Put),
            ("Delete", HttpMethod.Delete),
            ("Remove", HttpMethod.Delete),
            ("Create", HttpMethod.Post),
            ("Add", HttpMethod.Post),
            ("Insert", HttpMethod.Post),
            ("Post", HttpMethod.Post),
            ("Patch", HttpMethod.Patch),
        ];

        foreach (var (prefix, method) in prefixMap)
        {
            if (methodName.StartsWith(prefix))
            {
                return (method, ToKebabCase(methodName[prefix.Length..]));
            }
        }

        // 无匹配前缀：默认 POST，完整方法名作为 action
        return (HttpMethod.Post, ToKebabCase(methodName));
    }

    /// <summary>
    /// PascalCase → kebab-case（与 ABP StringExtensions.ToKebabCase 保持一致：先转 camelCase 再拆分）
    /// 例如: GetById → get-by-id, LLM → l-lm（与 ABP 服务端路由一致）
    /// </summary>
    public static string ToKebabCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // 与 ABP 一致：先将首字母小写（camelCase），再在小写字母/数字与大写字母之间插入连字符
        var camel = char.ToLowerInvariant(input[0]) + input[1..];
        var result = KebabRegex().Replace(camel, "$1-$2");
        return result.ToLowerInvariant();
    }

    [GeneratedRegex(@"([a-z0-9])([A-Z])")]
    private static partial Regex KebabRegex();
}
