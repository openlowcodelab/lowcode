using CommunityToolkit.Mvvm.ComponentModel;
using H.AppLab.Desktop.Services;
using H.Assistant.Application.Contracts;
using System.Collections.ObjectModel;

namespace H.AppLab.Desktop.ViewModels;

/// <summary>
/// 智能体管理 ViewModel（只读列表，数据统一由 Assistant.Web 维护）
/// </summary>
public partial class AgentSettingsViewModel : ObservableObject
{
    private readonly IAgentAppService _agentAppService;
    private readonly ToastService _toast;

    public ObservableCollection<AgentCardItem> Agents { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAgents))]
    private bool loading;

    public bool HasAgents => !Loading && Agents.Count > 0;

    public AgentSettingsViewModel(IAgentAppService agentAppService, ToastService toast)
    {
        _agentAppService = agentAppService;
        _toast = toast;
    }

    public async Task LoadAsync()
    {
        Loading = true;
        try
        {
            var result = (await _agentAppService.GetListAsync(new AgentQueryDto { MaxResultCount = 100 })).Data!;
            Agents.Clear();
            foreach (var agent in result.Items)
            {
                var item = new AgentCardItem(agent);
                try
                {
                    var skills = (await _agentAppService.GetAgentSkillsAsync(agent.Id)).Data ?? [];
                    item.SetSkills(skills);
                }
                catch
                {
                    // 与 Web 端一致：技能加载失败忽略
                }
                Agents.Add(item);
            }
        }
        catch (Exception ex)
        {
            _toast.Show($"加载智能体列表失败: {ex.Message}", "error");
        }
        finally
        {
            Loading = false;
            OnPropertyChanged(nameof(HasAgents));
        }
    }
}

/// <summary>
/// 智能体卡片项
/// </summary>
public partial class AgentCardItem : ObservableObject
{
    public AgentCardItem(AgentDto dto)
    {
        Dto = dto;
    }

    public AgentDto Dto { get; }

    public string DisplayName => Dto.DisplayName;
    public string AgentType => Dto.AgentType;
    public bool IsEnabled => Dto.IsEnabled;
    public string Description => Dto.Description;
    public string TemperatureText => Dto.Temperature.ToString();
    public string MaxTokensText => Dto.MaxTokens.ToString();
    public string StreamingText => Dto.SupportsStreaming ? "支持" : "不支持";

    public ObservableCollection<string> SkillNames { get; } = [];

    [ObservableProperty]
    private bool hasSkills;

    public void SetSkills(List<SkillDto> skills)
    {
        SkillNames.Clear();
        foreach (var skill in skills)
        {
            SkillNames.Add(skill.DisplayName);
        }
        HasSkills = SkillNames.Count > 0;
    }
}
