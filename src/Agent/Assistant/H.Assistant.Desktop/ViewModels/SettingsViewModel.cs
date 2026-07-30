using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using H.Assistant.Application.Contracts;
using H.Assistant.Desktop.Services;

namespace H.Assistant.Desktop.ViewModels;

/// <summary>
/// 设置页 ViewModel（对应 Web 端 Settings 系列页面：左侧菜单 + 右侧内容）
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    /// <summary>返回聊天页</summary>
    public event Action? BackRequested;

    public LlmSettingsViewModel Llm { get; }
    public AgentSettingsViewModel Agents { get; }
    public McpSettingsViewModel Mcp { get; }
    public SkillSettingsViewModel Skills { get; }

    public List<SettingsMenuItem> MenuItems { get; } =
    [
        new("general", "通用"),
        new("agent", "智能体"),
        new("skill", "技能"),
        new("model", "模型"),
        new("mcp", "MCP")
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGeneral))]
    [NotifyPropertyChangedFor(nameof(IsAgent))]
    [NotifyPropertyChangedFor(nameof(IsSkill))]
    [NotifyPropertyChangedFor(nameof(IsModel))]
    [NotifyPropertyChangedFor(nameof(IsMcp))]
    private string activeMenu = "general";

    public bool IsGeneral => ActiveMenu == "general";
    public bool IsAgent => ActiveMenu == "agent";
    public bool IsSkill => ActiveMenu == "skill";
    public bool IsModel => ActiveMenu == "model";
    public bool IsMcp => ActiveMenu == "mcp";

    // 通用设置（与 Web 端一致：仅本地状态，不做持久化）
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLightTheme))]
    [NotifyPropertyChangedFor(nameof(IsDarkTheme))]
    private string themeMode = "light";

    public bool IsLightTheme => ThemeMode == "light";
    public bool IsDarkTheme => ThemeMode == "dark";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsChineseLanguage))]
    [NotifyPropertyChangedFor(nameof(IsEnglishLanguage))]
    private string language = "zh";

    public bool IsChineseLanguage => Language == "zh";
    public bool IsEnglishLanguage => Language == "en";

    public SettingsViewModel(ILLMAppService llmAppService, IAgentAppService agentAppService,
        ISkillAppService skillAppService, IMcpServerAppService mcpServerAppService, ToastService toast)
    {
        Llm = new LlmSettingsViewModel(llmAppService, toast);
        Agents = new AgentSettingsViewModel(agentAppService, skillAppService, llmAppService, toast);
        Mcp = new McpSettingsViewModel(mcpServerAppService, toast);
        Skills = new SkillSettingsViewModel(skillAppService, toast);
    }

    public void SelectMenu(string key)
    {
        ActiveMenu = key;
        _ = LoadActiveMenuAsync(key);
    }

    private async Task LoadActiveMenuAsync(string key)
    {
        switch (key)
        {
            case "model":
                await Llm.LoadAsync();
                break;
            case "agent":
                await Agents.LoadAsync();
                break;
            case "mcp":
                await Mcp.LoadAsync();
                break;
            case "skill":
                await Skills.LoadAsync();
                break;
        }
    }

    [RelayCommand]
    private void Navigate(SettingsMenuItem item)
    {
        SelectMenu(item.Key);
    }

    [RelayCommand]
    private void GoBack()
    {
        BackRequested?.Invoke();
    }

    [RelayCommand]
    private void SetThemeMode(string mode)
    {
        ThemeMode = mode;
    }

    [RelayCommand]
    private void SetLanguage(string lang)
    {
        Language = lang;
    }
}

public record SettingsMenuItem(string Key, string Label);
