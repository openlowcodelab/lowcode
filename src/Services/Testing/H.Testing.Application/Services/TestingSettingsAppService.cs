using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 测试模块全局设置服务（Key-Value 存储）
/// </summary>
public class TestingSettingsAppService : ApplicationService, ITestingSettingsAppService
{
    /// <summary>浏览器可执行文件路径配置键</summary>
    public const string BrowserPathKey = "BrowserPath";

    /// <summary>CI 接入令牌配置键</summary>
    public const string CiTokenKey = "CiToken";

    private readonly IRepository<SettingsEntity, long> _repository;

    public TestingSettingsAppService(IRepository<SettingsEntity, long> repository)
    {
        _repository = repository;
    }

    public async Task<BaseOutput<TestingSettingsDto>> GetBrowserPathAsync()
    {
        return new(new TestingSettingsDto
        {
            BrowserPath = await GetValueAsync(BrowserPathKey)
        });
    }

    public async Task<BaseOutput<bool>> UpdateAsync(TestingSettingsDto settings)
    {
        await SetValueAsync(BrowserPathKey, settings.BrowserPath);
        return new(true);
    }

    public async Task<BaseOutput<List<DetectedBrowserDto>>> DetectBrowsersAsync()
    {
        return await Task.FromResult(new BaseOutput<List<DetectedBrowserDto>>(DetectInstalledBrowsers()));
    }

    public async Task<BaseOutput<string>> GetCiTokenAsync()
    {
        var token = await GetValueAsync(CiTokenKey);
        if (string.IsNullOrEmpty(token))
        {
            token = GenerateToken();
            await SetValueAsync(CiTokenKey, token);
        }

        return new(token);
    }

    public async Task<BaseOutput<string>> RegenerateCiTokenAsync()
    {
        var token = GenerateToken();
        await SetValueAsync(CiTokenKey, token);
        return new(token);
    }

    public async Task<BaseOutput<string?>> GetUserSettingAsync(string name)
    {
        var userKey = ResolveCurrentUserKey();
        return new(await GetValueAsync(name, SettingProviders.User, userKey));
    }

    public async Task<BaseOutput> SetUserSettingAsync(SettingUserValueInput input)
    {
        var userKey = ResolveCurrentUserKey();
        await SetValueAsync(input.Name, input.Value, SettingProviders.User, userKey);
        return new();
    }

    /// <summary>
    /// 解析当前登录用户标识作为用户级设置的 ProviderKey；未登录抛异常
    /// </summary>
    private string ResolveCurrentUserKey()
    {
        var userKey = CurrentUser?.Id?.ToString() ?? CurrentUser?.UserName;
        if (string.IsNullOrWhiteSpace(userKey))
        {
            throw new UserFriendlyException("用户未登录，无法读写用户级设置");
        }
        return userKey;
    }

    private static string GenerateToken() => Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

    /// <summary>
    /// 读取全局设置值，不存在时返回 null
    /// </summary>
    private Task<string?> GetValueAsync(string key)
        => GetValueAsync(key, SettingProviders.Global, string.Empty);

    /// <summary>
    /// 写入全局设置值（不存在则新增，空值存为 null）
    /// </summary>
    private Task SetValueAsync(string key, string? value)
        => SetValueAsync(key, value, SettingProviders.Global, string.Empty);

    /// <summary>
    /// 读取指定提供者作用域的设置值，不存在时返回 null
    /// </summary>
    private async Task<string?> GetValueAsync(string key, string providerName, string? providerKey)
    {
        var query = await _repository.GetQueryableAsync();
        var entity = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(e => e.Key == key && e.ProviderName == providerName && e.ProviderKey == providerKey));
        return entity?.Value;
    }

    /// <summary>
    /// 写入指定提供者作用域的设置值（不存在则新增，空值存为 null）
    /// </summary>
    private async Task SetValueAsync(string key, string? value, string providerName, string? providerKey)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        var query = await _repository.GetQueryableAsync();
        var entity = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(e => e.Key == key && e.ProviderName == providerName && e.ProviderKey == providerKey));
        if (entity == null)
        {
            entity = new SettingsEntity
            {
                Key = key,
                Value = normalized,
                ProviderName = providerName,
                ProviderKey = providerKey
            };
            await _repository.InsertAsync(entity, autoSave: true);
        }
        else
        {
            entity.Value = normalized;
            await _repository.UpdateAsync(entity, autoSave: true);
        }
    }

    /// <summary>
    /// 检测本机常见安装位置的 Chrome / Edge 浏览器
    /// </summary>
    private static List<DetectedBrowserDto> DetectInstalledBrowsers()
    {
        var candidates = new List<(string Name, string? Path)>
        {
            ("Chrome", Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Google\Chrome\Application\chrome.exe")),
            ("Chrome", Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Google\Chrome\Application\chrome.exe")),
            ("Chrome", Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Google\Chrome\Application\chrome.exe")),
            ("Edge", Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Microsoft\Edge\Application\msedge.exe")),
            ("Edge", Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Microsoft\Edge\Application\msedge.exe"))
        };

        var result = new List<DetectedBrowserDto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, path) in candidates)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path) || !seen.Add(path))
            {
                continue;
            }

            result.Add(new DetectedBrowserDto { Name = name, Path = path });
        }

        return result;
    }
}
