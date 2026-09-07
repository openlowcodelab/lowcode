using System.ComponentModel.DataAnnotations;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试模块全局设置模型
/// </summary>
public class TestingSettingsDto
{
    /// <summary>
    /// 浏览器可执行文件路径（如 Chrome/Edge 的安装路径，为空时使用项目自带或 Playwright 内置浏览器）
    /// </summary>
    [StringLength(500, ErrorMessage = "浏览器地址长度不能超过500个字符")]
    public string? BrowserPath { get; set; }
}

/// <summary>
/// 自动检测到的浏览器
/// </summary>
public class DetectedBrowserDto
{
    /// <summary>浏览器名称（如 Chrome、Edge）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>浏览器可执行文件路径</summary>
    public string Path { get; set; } = string.Empty;
}

/// <summary>
/// 用户级设置写入参数
/// </summary>
public class SettingUserValueInput
{
    /// <summary>设置名称（见 <see cref="TestingSettingNames"/>）</summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>设置值（为空表示清除）</summary>
    [StringLength(500)]
    public string? Value { get; set; }
}

/// <summary>
/// Testing 模块用户级设置名称常量（前后端共用）
/// </summary>
public static class TestingSettingNames
{
    /// <summary>当前用户上次选中的项目 Id（值为项目 Id 字符串）</summary>
    public const string LastProjectId = "Testing.LastProjectId";
}
