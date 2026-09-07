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

    /// <summary>
    /// 获取 CI 接入令牌（首次访问自动生成）
    /// </summary>
    Task<BaseOutput<string>> GetCiTokenAsync();

    /// <summary>
    /// 重新生成 CI 接入令牌（旧令牌立即失效）
    /// </summary>
    Task<BaseOutput<string>> RegenerateCiTokenAsync();

    /// <summary>
    /// 读取当前用户的某项设置（不存在返回 null）
    /// </summary>
    Task<BaseOutput<string?>> GetUserSettingAsync(string name);

    /// <summary>
    /// 写入当前用户的某项设置（值为空表示清除）
    /// </summary>
    Task<BaseOutput> SetUserSettingAsync(SettingUserValueInput input);
}
