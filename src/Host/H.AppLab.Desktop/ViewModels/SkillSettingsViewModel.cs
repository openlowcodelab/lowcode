using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using H.Assistant.Application.Contracts;
using H.AppLab.Desktop.Services;

namespace H.AppLab.Desktop.ViewModels;

/// <summary>
/// 技能管理 ViewModel（只读列表，Skill/Tool 双 Tab，数据统一由 Assistant.Web 维护）
/// </summary>
public partial class SkillSettingsViewModel : ObservableObject
{
    private readonly ISkillAppService _skillAppService;
    private readonly ToastService _toast;

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
