using System.ComponentModel.DataAnnotations;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试用例步骤
/// </summary>
public class CaseStepDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required(ErrorMessage = "步骤名称不能为空")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "步骤类型不能为空")]
    public StepType Type { get; set; }

    public Dictionary<string, object> Parameters { get; set; } = new();

    public string ExpectedResult { get; set; } = string.Empty;

    public int Order { get; set; }

    public bool IsEnabled { get; set; } = true;

    // API 项目步骤特有属性
    public ApiStepConfig? ApiConfig { get; set; }

    // UI 项目步骤特有属性
    public UiStepConfig? UiConfig { get; set; }

    // 脚本步骤特有属性
    public ScriptStepConfig? ScriptConfig { get; set; }
}

/// <summary>
/// API项目步骤配置
/// </summary>
public class ApiStepConfig
{
    public string Method { get; set; } = "GET"; // GET, POST, PUT, DELETE, etc.

    /// <summary>
    /// 关联的项目服务ID，用于拼接完整URL
    /// </summary>
    public long ServiceId { get; set; }

    public string Url { get; set; } = string.Empty;

    public Dictionary<string, string> Headers { get; set; } = new();

    public Dictionary<string, string> Params { get; set; } = new();

    public string Body { get; set; } = string.Empty;

    public string BodyType { get; set; } = "json"; // json, form-data, x-www-form-urlencoded, raw, etc.

    public List<ApiAssertion> Assertions { get; set; } = new();

    public Dictionary<string, string> VariableExtractions { get; set; } = new(); // 从响应中提取变量

    /// <summary>
    /// Cookie配置
    /// </summary>
    public Dictionary<string, string> Cookies { get; set; } = new();

    /// <summary>
    /// 认证配置
    /// </summary>
    public AuthConfig Auth { get; set; } = new();

    /// <summary>
    /// 超时时间（毫秒）
    /// </summary>
    public int TimeoutMs { get; set; } = 30000;

    /// <summary>
    /// 是否跟随重定向
    /// </summary>
    public bool FollowRedirects { get; set; } = true;

    /// <summary>
    /// 是否验证SSL证书
    /// </summary>
    public bool VerifySSL { get; set; } = true;
}

/// <summary>
/// 认证配置
/// </summary>
public class AuthConfig
{
    /// <summary>
    /// 认证类型：None, Basic, Bearer, ApiKey, OAuth2
    /// </summary>
    public string Type { get; set; } = "None";

    /// <summary>
    /// 用户名（Basic认证）
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码（Basic认证）
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Token（Bearer认证）
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// API Key名称
    /// </summary>
    public string ApiKeyName { get; set; } = string.Empty;

    /// <summary>
    /// API Key值
    /// </summary>
    public string ApiKeyValue { get; set; } = string.Empty;

    /// <summary>
    /// API Key位置：Header, Query
    /// </summary>
    public string ApiKeyLocation { get; set; } = "Header";
}

/// <summary>
/// API断言配置
/// </summary>
public class ApiAssertion
{
    public string Type { get; set; } = string.Empty; // status_code, response_time, json_path, header, etc.

    public string Target { get; set; } = string.Empty; // 断言目标，如json路径

    public string Operator { get; set; } = string.Empty; // equals, contains, gt, lt, etc.

    public string ExpectedValue { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// UI项目步骤配置
/// </summary>
public class UiStepConfig
{
    public string Action { get; set; } = string.Empty; // navigate, click, type, wait, assert, etc.

    public string Selector { get; set; } = string.Empty; // CSS选择器或XPath

    public string SelectorType { get; set; } = "css"; // css, xpath, id, name, etc.

    public string Value { get; set; } = string.Empty; // 输入值或期望值

    public int TimeoutMs { get; set; } = 30000;

    public bool TakeScreenshot { get; set; } = false;

    public Dictionary<string, string> Options { get; set; } = new(); // 额外选项
}

/// <summary>
/// 脚本步骤配置
/// </summary>
public class ScriptStepConfig
{
    /// <summary>
    /// 脚本类型：Javascript, CSharp
    /// </summary>
    public string ScriptType { get; set; } = "Javascript";

    /// <summary>
    /// 脚本内容
    /// </summary>
    public string ScriptContent { get; set; } = string.Empty;

    /// <summary>
    /// 脚本参数
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// 超时时间（毫秒）
    /// </summary>
    public int TimeoutMs { get; set; } = 30000;

    /// <summary>
    /// 是否捕获输出
    /// </summary>
    public bool CaptureOutput { get; set; } = true;

    /// <summary>
    /// 变量提取配置
    /// </summary>
    public Dictionary<string, string> VariableExtractions { get; set; } = new();

    /// <summary>
    /// 预期返回值类型：string, number, boolean, object
    /// </summary>
    public string ExpectedReturnType { get; set; } = "string";
}

/// <summary>
/// 步骤类型
/// </summary>
public enum StepType
{
    // API项目步骤
    HttpRequest = 1,
    ApiAssertion = 2,
    VariableExtraction = 3,

    // UI项目步骤
    Navigate = 10,
    Click = 11,
    Input = 12,
    Select = 13,
    Wait = 14,
    Assert = 15,
    Screenshot = 16,
    Scroll = 17,
    Hover = 18,
    KeyPress = 19,

    // 通用步骤
    Script = 20,
    Delay = 21,
    SetVariable = 22,
    Condition = 23,

    // 脚本步骤
    JavascriptScript = 30,
    CSharpScript = 31
}