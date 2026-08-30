using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试模块全局设置服务接口
/// </summary>
public interface ITestingSettingsAppService : IAppService
{
    /// <summary>
    /// 获取浏览器设置
    /// </summary>
    Task<BaseOutput<TestingSettingsDto>> GetBrowserPathAsync();

    /// <summary>
    /// 保存设置
    /// </summary>
    Task<BaseOutput<bool>> UpdateAsync(TestingSettingsDto settings);

    /// <summary>
    /// 自动检测本机已安装的浏览器（Chrome/Edge）
    /// </summary>
    Task<BaseOutput<List<DetectedBrowserDto>>> DetectBrowsersAsync();
}
