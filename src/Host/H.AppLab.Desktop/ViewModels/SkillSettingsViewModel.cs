using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using H.Assistant.Application.Contracts;
using H.AppLab.Desktop.Services;

namespace H.AppLab.Desktop.ViewModels;

/// <summary>
/// 技能管理 ViewModel（对应 Web 端 SettingsSkills.razor，Skill/Tool 双 Tab）
/// </summary>
public partial class SkillSettingsViewModel : ObservableObject
{
    private readonly ISkillAppService _skillAppService;
    private readonly ToastService _toast;

    public List<TransportOption> SkillTypeOptions { get; } =
    [
        new("Function", "Function (函数调用)"),
        new("Plugin", "Plugin (插件)"),
        new("Workflow", "Workflow (工作流)")
    ];

    public ObservableCollection<SkillCardItem> Skills { get; } = [];
    public ObservableCollection<ToolCardItem> Tools { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSkillTab))]
    [NotifyPropertyChangedFor(nameof(IsToolTab))]
    private string activeTab = "skill";

    public bool IsSkillTab => ActiveTab == "skill";
    public bool IsToolTab => ActiveTab == "tool";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSkills))]
    [NotifyPropertyChangedFor(nameof(HasTools))]
    private bool loading;

    public bool HasSkills => !Loading && Skills.Count > 0;
    public bool HasTools => !Loading && Tools.Count > 0;

    [ObservableProperty]
    private bool showDialog;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DialogTitle))]
    [NotifyPropertyChangedFor(nameof(DialogSaveText))]
    [NotifyPropertyChangedFor(nameof(ShowSkillNameField))]
    private bool isEditing;

    public string DialogTitle => IsEditing ? "编辑技能" : "添加技能";
    public string DialogSaveText => IsEditing ? "保存" : "添加";
    public bool ShowSkillNameField => !IsEditing;

    [ObservableProperty]
    private bool saving;

    private Guid? _editingId;

    [ObservableProperty]
    private string editSkillName = string.Empty;

    [ObservableProperty]
    private string editDisplayName = string.Empty;

    [ObservableProperty]
    private string editDescription = string.Empty;

    [ObservableProperty]
    private TransportOption? editSkillType;

    [ObservableProperty]
    private string editImplementationClass = string.Empty;

    [ObservableProperty]
    private string editConfig = string.Empty;

    [ObservableProperty]
    private string editParameterSchema = string.Empty;

    [ObservableProperty]
    private bool editIsEnabled = true;

    [ObservableProperty]
    private bool editRequiresApproval;

    public SkillSettingsViewModel(ISkillAppService skillAppService, ToastService toast)
    {
        _skillAppService = skillAppService;
        _toast = toast;
    }

    [RelayCommand]
    private void SwitchTab(string tab)
    {
        ActiveTab = tab;
    }

    public async Task LoadAsync()
    {
        Loading = true;
        try
        {
            var result = await _skillAppService.GetListAsync(new SkillDefinitionQueryDto { MaxResultCount = 100 });
            Skills.Clear();
            foreach (var skill in result.Items)
            {
                Skills.Add(new SkillCardItem(skill));
            }

            // Tool 列表由技能的实现类分组推导（与 Web 端一致）
            Tools.Clear();
            var toolGroups = result.Items
                .Where(s => !string.IsNullOrWhiteSpace(s.ImplementationClass))
                .GroupBy(s => s.ImplementationClass!);
            foreach (var group in toolGroups)
            {
                var first = group.First();
                Tools.Add(new ToolCardItem
                {
                    ClassName = first.ImplementationClass!.Split('.').Last(),
                    FullName = first.ImplementationClass!,
                    Description = first.Description,
                    SkillName = string.Join(", ", group.Select(s => s.DisplayName)),
                    IsEnabled = group.Any(s => s.IsEnabled)
                });
            }
        }
        catch (Exception ex)
        {
            _toast.Show($"加载技能列表失败: {ex.Message}", "error");
        }
        finally
        {
            Loading = false;
            OnPropertyChanged(nameof(HasSkills));
            OnPropertyChanged(nameof(HasTools));
        }
    }

    [RelayCommand]
    private void ShowAdd()
    {
        IsEditing = false;
        _editingId = null;
        EditSkillName = string.Empty;
        EditDisplayName = string.Empty;
        EditDescription = string.Empty;
        EditSkillType = SkillTypeOptions[0];
        EditImplementationClass = string.Empty;
        EditConfig = string.Empty;
        EditParameterSchema = string.Empty;
        EditIsEnabled = true;
        EditRequiresApproval = false;
        ShowDialog = true;
    }

    [RelayCommand]
    private void Edit(SkillCardItem item)
    {
        var skill = item.Dto;
        IsEditing = true;
        _editingId = skill.Id;
        EditSkillName = skill.SkillName;
        EditDisplayName = skill.DisplayName;
        EditDescription = skill.Description;
        EditSkillType = SkillTypeOptions.FirstOrDefault(t => t.Value == skill.SkillType) ?? SkillTypeOptions[0];
        EditImplementationClass = skill.ImplementationClass ?? string.Empty;
        EditConfig = skill.Config ?? string.Empty;
        EditParameterSchema = skill.ParameterSchema ?? string.Empty;
        EditIsEnabled = skill.IsEnabled;
        EditRequiresApproval = skill.RequiresApproval;
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
        if (string.IsNullOrWhiteSpace(EditSkillName)) { _toast.Show("请输入技能名称", "warning"); return; }
        if (string.IsNullOrWhiteSpace(EditDisplayName)) { _toast.Show("请输入显示名称", "warning"); return; }
        if (string.IsNullOrWhiteSpace(EditDescription)) { _toast.Show("请输入描述", "warning"); return; }

        Saving = true;
        try
        {
            if (IsEditing && _editingId.HasValue)
            {
                await _skillAppService.UpdateAsync(_editingId.Value, new UpdateSkillDefinitionDto
                {
                    DisplayName = EditDisplayName,
                    Description = EditDescription,
                    ImplementationClass = string.IsNullOrWhiteSpace(EditImplementationClass) ? null : EditImplementationClass,
                    Config = string.IsNullOrWhiteSpace(EditConfig) ? null : EditConfig,
                    ParameterSchema = string.IsNullOrWhiteSpace(EditParameterSchema) ? null : EditParameterSchema,
                    IsEnabled = EditIsEnabled,
                    RequiresApproval = EditRequiresApproval
                });
                _toast.Show("技能已更新", "success");
            }
            else
            {
                await _skillAppService.CreateAsync(new CreateSkillDefinitionDto
                {
                    SkillName = EditSkillName.Trim().ToLower(),
                    DisplayName = EditDisplayName,
                    Description = EditDescription,
                    SkillType = EditSkillType?.Value ?? "Function",
                    ImplementationClass = string.IsNullOrWhiteSpace(EditImplementationClass) ? null : EditImplementationClass,
                    Config = string.IsNullOrWhiteSpace(EditConfig) ? null : EditConfig,
                    ParameterSchema = string.IsNullOrWhiteSpace(EditParameterSchema) ? null : EditParameterSchema,
                    IsEnabled = EditIsEnabled,
                    RequiresApproval = EditRequiresApproval
                });
                _toast.Show("技能已添加", "success");
            }
            ShowDialog = false;
            await LoadAsync();
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
    private async Task ToggleAsync(SkillCardItem item)
    {
        try
        {
            await _skillAppService.ToggleEnabledAsync(item.Dto.Id, !item.Dto.IsEnabled);
            _toast.Show(item.Dto.IsEnabled ? "技能已禁用" : "技能已启用", "success");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"操作失败: {ex.Message}", "error");
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(SkillCardItem item)
    {
        try
        {
            await _skillAppService.DeleteAsync(item.Dto.Id);
            _toast.Show("已删除", "success");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"删除失败: {ex.Message}", "error");
        }
    }
}

/// <summary>
/// 技能卡片项
/// </summary>
public class SkillCardItem(SkillDto dto)
{
    public SkillDto Dto { get; } = dto;

    public string DisplayName => Dto.DisplayName;
    public string SkillName => Dto.SkillName;
    public bool IsEnabled => Dto.IsEnabled;
    public string ToggleText => Dto.IsEnabled ? "禁用" : "启用";
    public string SkillType => Dto.SkillType;
    public string Description => Dto.Description;
    public string? ImplementationClass => Dto.ImplementationClass;
    public bool HasImplementationClass => !string.IsNullOrEmpty(Dto.ImplementationClass);
    public string UsageCountText => Dto.UsageCount.ToString();
    public bool HasLastUsedTime => Dto.LastUsedTime.HasValue;
    public string LastUsedTimeText => Dto.LastUsedTime.HasValue ? Dto.LastUsedTime.Value.ToString("yyyy-MM-dd HH:mm") : "";
}

/// <summary>
/// Tool 展示项（由技能实现类推导）
/// </summary>
public class ToolCardItem
{
    public string ClassName { get; init; } = "";
    public string FullName { get; init; } = "";
    public string Description { get; init; } = "";
    public string SkillName { get; init; } = "";
    public bool IsEnabled { get; init; }
}
