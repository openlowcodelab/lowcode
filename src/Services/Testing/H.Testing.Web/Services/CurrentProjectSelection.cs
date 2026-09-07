using H.Testing.Application.Contracts;
using Microsoft.JSInterop;

namespace H.Testing.Web.Services;

/// <summary>
/// Testing 模块「当前选中项目」共享状态：所有页面通过顶栏单一选择器读写，
/// 以 localStorage 作为缓存、后端用户级设置作为持久化来源。
/// </summary>
public class CurrentProjectSelection
{
    /// <summary>localStorage 缓存键（与既有写入保持一致）</summary>
    private const string StorageKey = "lastSelectedProjectId";

    private readonly IJSRuntime _js;
    private readonly ITestingSettingsAppService _settings;

    public CurrentProjectSelection(IJSRuntime js, ITestingSettingsAppService settings)
    {
        _js = js;
        _settings = settings;
    }

    /// <summary>当前选中的项目 Id；0 表示尚未确定</summary>
    public long ProjectId { get; private set; }

    /// <summary>是否已完成初始化（读缓存/回源后端）</summary>
    public bool Ready { get; private set; }

    /// <summary>选中项目变化时触发，供各页面重载数据</summary>
    public event Action? Changed;

    /// <summary>
    /// 初始化：命中 localStorage 直接使用（不打后端），仅缓存缺失时回源一次用户级设置。
    /// </summary>
    public async Task InitializeAsync()
    {
        if (Ready)
        {
            return;
        }

        if (await ReadFromStorageAsync() is { } stored && stored != 0)
        {
            ProjectId = stored;
            Ready = true;
            return;
        }

        try
        {
            var output = await _settings.GetUserSettingAsync(TestingSettingNames.LastProjectId);
            if (long.TryParse(output?.Data, out var remote) && remote != 0)
            {
                ProjectId = remote;
                await WriteToStorageAsync(remote);
            }
        }
        catch
        {
            // 回源失败时保持静默，ProjectId 交由调用方按项目列表回落
        }

        Ready = true;
    }

    /// <summary>
    /// 选择项目：更新内存状态、写 localStorage、通知订阅方，并持久化到后端用户级设置。
    /// </summary>
    public async Task SelectProjectAsync(long projectId)
    {
        if (projectId == 0 || (Ready && projectId == ProjectId))
        {
            return;
        }

        ProjectId = projectId;
        Ready = true;
        await WriteToStorageAsync(projectId);
        Changed?.Invoke();

        try
        {
            await _settings.SetUserSettingAsync(new SettingUserValueInput
            {
                Name = TestingSettingNames.LastProjectId,
                Value = projectId.ToString()
            });
        }
        catch
        {
            // 后端持久化失败不影响本地缓存与内存状态
        }
    }

    /// <summary>项目列表加载后，若当前无有效选择则回落到第一个项目。</summary>
    public async Task FallbackToAsync(long projectId)
    {
        if (projectId != 0)
        {
            await SelectProjectAsync(projectId);
        }
    }

    private async Task<long?> ReadFromStorageAsync()
    {
        try
        {
            var raw = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            return long.TryParse(raw, out var value) ? value : null;
        }
        catch
        {
            return null;
        }
    }

    private async Task WriteToStorageAsync(long projectId)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, projectId.ToString());
        }
        catch
        {
            // 预渲染阶段 JS 不可用，忽略
        }
    }
}
