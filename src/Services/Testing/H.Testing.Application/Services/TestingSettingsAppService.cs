using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;
using H.Util.Base;
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

    /// <summary>
    /// 读取指定键的设置值，不存在时返回 null
    /// </summary>
    private async Task<string?> GetValueAsync(string key)
    {
        var query = await _repository.GetQueryableAsync();
        var entity = await AsyncExecuter.FirstOrDefaultAsync(query.Where(e => e.Key == key));
        return entity?.Value;
    }

    /// <summary>
    /// 写入指定键的设置值（不存在则新增，空值存为 null）
    /// </summary>
    private async Task SetValueAsync(string key, string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        var query = await _repository.GetQueryableAsync();
        var entity = await AsyncExecuter.FirstOrDefaultAsync(query.Where(e => e.Key == key));
        if (entity == null)
        {
            entity = new SettingsEntity { Key = key, Value = normalized };
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
