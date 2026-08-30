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
