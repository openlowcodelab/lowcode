using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using H.Assistant.Application.Contracts;
using H.AppLab.Desktop.Services;

namespace H.AppLab.Desktop.ViewModels;

/// <summary>
/// 智能体管理 ViewModel（对应 Web 端 SettingsAgents.razor）
/// </summary>
public partial class AgentSettingsViewModel : ObservableObject
{
    private readonly IAgentAppService _agentAppService;
    private readonly ISkillAppService _skillAppService;
    private readonly ILLMAppService _llmAppService;
    private readonly ToastService _toast;

    public ObservableCollection<AgentCardItem> Agents { get; } = [];
    public ObservableCollection<LlmOption> LlmOptions { get; } = [];
    public ObservableCollection<SkillCheckItem> FilteredSkills { get; } = [];

    private List<SkillDto> _availableSkills = [];
    private readonly HashSet<Guid> _selectedSkillIds = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAgents))]
    private bool loading;

    public bool HasAgents => !Loading && Agents.Count > 0;

    [ObservableProperty]
    private bool showDialog;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DialogTitle))]
    [NotifyPropertyChangedFor(nameof(DialogSaveText))]
    [NotifyPropertyChangedFor(nameof(AgentTypeEditable))]
    private bool isEditing;

    public string DialogTitle => IsEditing ? "编辑智能体" : "新建智能体";
    public string DialogSaveText => IsEditing ? "保存" : "创建";
    public bool AgentTypeEditable => !IsEditing;

    [ObservableProperty]
    private bool saving;

    private Guid? _editingId;

    [ObservableProperty]
    private string editAgentType = string.Empty;

    [ObservableProperty]
    private string editDisplayName = string.Empty;

    [ObservableProperty]
    private string editDescription = string.Empty;

    [ObservableProperty]
    private string editSystemPrompt = string.Empty;

    [ObservableProperty]
    private decimal editTemperature = 0.7m;

    [ObservableProperty]
    private decimal editMaxTokens = 2000;

    [ObservableProperty]
    private bool editSupportsStreaming = true;

    [ObservableProperty]
    private bool editIsEnabled = true;

    [ObservableProperty]
    private LlmOption? editDefaultModel;

    [ObservableProperty]
    private string skillSearchText = string.Empty;

    public AgentSettingsViewModel(IAgentAppService agentAppService, ISkillAppService skillAppService,
        ILLMAppService llmAppService, ToastService toast)
    {
        _agentAppService = agentAppService;
        _skillAppService = skillAppService;
        _llmAppService = llmAppService;
        _toast = toast;
    }

    partial void OnSkillSearchTextChanged(string value) => RefreshFilteredSkills();

    public async Task LoadAsync()
    {
        await LoadLlmConfigsAsync();
        await LoadAgentsAsync();
    }

    private async Task LoadLlmConfigsAsync()
    {
        try
        {
            var configs = await _llmAppService.GetAllAsync();
            LlmOptions.Clear();
            LlmOptions.Add(new LlmOption(null, "无"));
            foreach (var config in configs)
            {
                LlmOptions.Add(new LlmOption(config.Id, $"{config.ProviderDisplayName} - {config.Model}"));
            }
        }
        catch (Exception ex)
        {
            _toast.Show($"加载模型配置失败: {ex.Message}", "error");
        }
    }

    private async Task LoadAgentsAsync()
    {
        Loading = true;
        try
        {
            var result = await _agentAppService.GetListAsync(new AgentQueryDto { MaxResultCount = 100 });
            Agents.Clear();
            foreach (var agent in result.Items)
            {
                var item = new AgentCardItem(agent);
                try
                {
                    var skills = await _agentAppService.GetAgentSkillsAsync(agent.Id);
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

    private async Task LoadAvailableSkillsAsync()
    {
        try
        {
            _availableSkills = await _skillAppService.GetEnabledSkillsAsync();
        }
        catch (Exception ex)
        {
            _availableSkills = [];
            _toast.Show($"加载可用技能失败: {ex.Message}", "error");
        }
        RefreshFilteredSkills();
    }

    private void RefreshFilteredSkills()
    {
        var filtered = string.IsNullOrWhiteSpace(SkillSearchText)
            ? _availableSkills
            : _availableSkills.Where(s =>
                s.DisplayName.Contains(SkillSearchText, StringComparison.OrdinalIgnoreCase) ||
                s.SkillName.Contains(SkillSearchText, StringComparison.OrdinalIgnoreCase)).ToList();

        FilteredSkills.Clear();
        foreach (var group in filtered.GroupBy(s => s.SkillType))
        {
            FilteredSkills.Add(new SkillCheckItem(group.Key, isGroupHeader: true));
            foreach (var skill in group)
            {
                var item = new SkillCheckItem(skill.DisplayName, isGroupHeader: false)
                {
                    SkillId = skill.Id,
                    IsChecked = _selectedSkillIds.Contains(skill.Id)
                };
                item.CheckedChanged += checkedValue =>
                {
                    if (checkedValue)
                    {
                        _selectedSkillIds.Add(skill.Id);
                    }
                    else
                    {
                        _selectedSkillIds.Remove(skill.Id);
                    }
                };
                FilteredSkills.Add(item);
            }
        }
    }

    [RelayCommand]
    private async Task ShowAddAsync()
    {
        IsEditing = false;
        _editingId = null;
        EditAgentType = string.Empty;
        EditDisplayName = string.Empty;
        EditDescription = string.Empty;
        EditSystemPrompt = string.Empty;
        EditIsEnabled = true;
        EditSupportsStreaming = true;
        EditTemperature = 0.7m;
        EditMaxTokens = 2000;
        EditDefaultModel = LlmOptions.FirstOrDefault();
        SkillSearchText = string.Empty;
        _selectedSkillIds.Clear();
        await LoadAvailableSkillsAsync();
        ShowDialog = true;
    }

    [RelayCommand]
    private async Task EditAsync(AgentCardItem item)
    {
        var agent = item.Dto;
        IsEditing = true;
        _editingId = agent.Id;
        EditAgentType = agent.AgentType;
        EditDisplayName = agent.DisplayName;
        EditDescription = agent.Description;
        EditSystemPrompt = agent.SystemPrompt;
        EditIsEnabled = agent.IsEnabled;
        EditSupportsStreaming = agent.SupportsStreaming;
        EditTemperature = (decimal)agent.Temperature;
        EditMaxTokens = agent.MaxTokens;
        EditDefaultModel = LlmOptions.FirstOrDefault(o => o.Id == agent.DefaultModelConfigId) ?? LlmOptions.FirstOrDefault();
        SkillSearchText = string.Empty;
        _selectedSkillIds.Clear();

        await LoadAvailableSkillsAsync();
        try
        {
            var currentSkills = await _agentAppService.GetAgentSkillsAsync(agent.Id);
            foreach (var skill in currentSkills)
            {
                _selectedSkillIds.Add(skill.Id);
            }
        }
        catch
        {
            // 忽略
        }
        RefreshFilteredSkills();
        ShowDialog = true;
    }

    [RelayCommand]
    private void CloseDialog()
    {
        ShowDialog = false;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditAgentType)) { _toast.Show("请输入类型标识", "warning"); return; }
        if (string.IsNullOrWhiteSpace(EditDisplayName)) { _toast.Show("请输入显示名称", "warning"); return; }
        if (string.IsNullOrWhiteSpace(EditDescription)) { _toast.Show("请输入描述", "warning"); return; }
        if (string.IsNullOrWhiteSpace(EditSystemPrompt)) { _toast.Show("请输入系统提示词", "warning"); return; }

        Saving = true;
        try
        {
            if (IsEditing && _editingId.HasValue)
            {
                await _agentAppService.UpdateAsync(_editingId.Value, new UpdateAgentDto
                {
                    DisplayName = EditDisplayName,
                    Description = EditDescription,
                    SystemPrompt = EditSystemPrompt,
                    IsEnabled = EditIsEnabled,
                    SupportsStreaming = EditSupportsStreaming,
                    Temperature = (float)EditTemperature,
                    MaxTokens = (int)EditMaxTokens,
                    DefaultModelConfigId = EditDefaultModel?.Id,
                    SkillIds = _selectedSkillIds.ToList()
                });
                _toast.Show("智能体已更新", "success");
            }
            else
            {
                await _agentAppService.CreateAsync(new CreateAgentDto
                {
                    AgentType = EditAgentType.Trim().ToLower(),
                    DisplayName = EditDisplayName,
                    Description = EditDescription,
                    SystemPrompt = EditSystemPrompt,
                    IsEnabled = EditIsEnabled,
                    SupportsStreaming = EditSupportsStreaming,
                    Temperature = (float)EditTemperature,
                    MaxTokens = (int)EditMaxTokens,
                    DefaultModelConfigId = EditDefaultModel?.Id,
                    SkillIds = _selectedSkillIds.ToList()
                });
                _toast.Show("智能体已创建", "success");
            }
            ShowDialog = false;
            await LoadAgentsAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"保存失败: {ex.Message}", "error");
        }
        finally
        {
            Saving = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(AgentCardItem item)
    {
        try
        {
            await _agentAppService.DeleteAsync(item.Dto.Id);
            _toast.Show("已删除", "success");
            await LoadAgentsAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"删除失败: {ex.Message}", "error");
        }
    }

    [RelayCommand]
    private async Task ToggleAsync(AgentCardItem item)
    {
        try
        {
            await _agentAppService.ToggleEnabledAsync(item.Dto.Id, !item.Dto.IsEnabled);
            _toast.Show(item.Dto.IsEnabled ? "智能体已禁用" : "智能体已启用", "success");
            await LoadAgentsAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"操作失败: {ex.Message}", "error");
        }
    }
}

public record LlmOption(Guid? Id, string Label)
{
    public override string ToString() => Label;
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
    public string ToggleText => Dto.IsEnabled ? "禁用" : "启用";
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

/// <summary>
/// 技能选择列表项（分组标题或复选项）
/// </summary>
public partial class SkillCheckItem : ObservableObject
{
    public SkillCheckItem(string label, bool isGroupHeader)
    {
        Label = label;
        IsGroupHeader = isGroupHeader;
    }

    public event Action<bool>? CheckedChanged;

    public string Label { get; }
    public bool IsGroupHeader { get; }
    public bool IsCheckable => !IsGroupHeader;
    public Guid SkillId { get; init; }

    private bool _isChecked;

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (SetProperty(ref _isChecked, value))
            {
                CheckedChanged?.Invoke(value);
            }
        }
    }
}
