using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using H.AppLab.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace H.AppLab.Desktop.ViewModels;

/// <summary>
/// 宿主外壳主窗口 ViewModel：左侧快捷菜单 + 内容区。
/// 插件应用视图按需创建并缓存，切换菜单时保留应用内状态。
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private const string TasksMenuId = "tasks";
    private const string KnowledgeMenuId = "knowledge";
    private const string SettingsMenuId = "settings";

    private readonly IServiceProvider _services;
    private readonly Dictionary<string, Control> _appViews = [];

    private Control? _tasksView;
    private Control? _knowledgeView;
    private Control? _settingsView;

    /// <summary>左侧快捷菜单项</summary>
    public ObservableCollection<NavItemViewModel> NavItems { get; } = [];

    /// <summary>底部知识中心入口（仅展示图标）</summary>
    public NavItemViewModel KnowledgeNav { get; }

    /// <summary>底部设置入口（仅展示图标）</summary>
    public NavItemViewModel SettingsNav { get; }

    /// <summary>已接入的全部插件应用</summary>
    public ObservableCollection<AppCardViewModel> Apps { get; } = [];

    [ObservableProperty]
    private object? currentContent;

    public MainWindowViewModel(IServiceProvider services, IEnumerable<IDesktopApp> apps)
    {
        _services = services;

        foreach (var app in apps)
        {
            Apps.Add(new AppCardViewModel(app));
        }

        // 快捷菜单：各插件应用 + 助手 + 任务
        foreach (var card in Apps)
        {
            NavItems.Add(new NavItemViewModel(card.App.Id, card.App.Name, card.App.Icon, this) { AppId = card.App.Id });
        }
        NavItems.Add(new NavItemViewModel(TasksMenuId, "任务", "🗓", this));

        // 底部知识中心 / 设置入口（仅展示图标）
        KnowledgeNav = new NavItemViewModel(KnowledgeMenuId, "知识中心", "📖", this);
        SettingsNav = new NavItemViewModel(SettingsMenuId, "设置", "⚙", this);

        Navigate(NavItems[0]);
    }

    [RelayCommand]
    private void Navigate(NavItemViewModel item)
    {
        foreach (var nav in NavItems)
        {
            nav.IsActive = nav == item;
        }
        KnowledgeNav.IsActive = item == KnowledgeNav;
        SettingsNav.IsActive = item == SettingsNav;

        if (item.Id == SettingsMenuId)
        {
            CurrentContent = GetSettingsView();
            return;
        }

        if (item.Id == KnowledgeMenuId)
        {
            CurrentContent = GetKnowledgeView();
            return;
        }

        CurrentContent = item.AppId is not null
            ? GetAppView(item.AppId)
            : GetTasksView();
    }

    /// <summary>
    /// 任务页（复用 H.Assistant.UI 的任务中心视图，按需创建并缓存）
    /// </summary>
    private Control GetTasksView()
    {
        if (_tasksView is null)
        {
            var vm = _services.GetRequiredService<TasksViewModel>();
            _tasksView = new TasksView { DataContext = vm };
            _ = vm.InitializeAsync();
        }
        return _tasksView;
    }

    /// <summary>
    /// 知识中心页（按需创建并缓存）
    /// </summary>
    private Control GetKnowledgeView()
    {
        if (_knowledgeView is null)
        {
            var vm = _services.GetRequiredService<KnowledgeViewModel>();
            _knowledgeView = new KnowledgeView { DataContext = vm };
            _ = vm.InitializeAsync();
        }
        return _knowledgeView;
    }

    /// <summary>
    /// 设置页（来自 H.Assistant.UI，按需创建并缓存）：每次进入重置到“通用”页。
    /// </summary>
    private Control GetSettingsView()
    {
        if (_settingsView is null)
        {
            var vm = _services.GetRequiredService<SettingsViewModel>();
            _settingsView = new SettingsPageView { DataContext = vm };
        }

        (_settingsView.DataContext as SettingsViewModel)?.SelectMenu("general");
        return _settingsView;
    }

    private Control GetAppView(string appId)
    {
        if (!_appViews.TryGetValue(appId, out var view))
        {
            var app = Apps.First(a => a.App.Id == appId).App;
            view = app.CreateView(_services);
            _appViews[appId] = view;
        }
        return view;
    }
}

/// <summary>
/// 左侧快捷菜单项
/// </summary>
public partial class NavItemViewModel : ObservableObject
{
    private readonly MainWindowViewModel _owner;

    public NavItemViewModel(string id, string title, string icon, MainWindowViewModel owner)
    {
        Id = id;
        Title = title;
        Icon = icon;
        _owner = owner;
    }

    public string Id { get; }

    public string Title { get; }

    public string Icon { get; }

    /// <summary>关联的插件应用 Id（内置页面为 null）</summary>
    public string? AppId { get; init; }

    [ObservableProperty]
    private bool isActive;

    [RelayCommand]
    private void Select() => _owner.NavigateCommand.Execute(this);
}

/// <summary>
/// 插件应用条目（用于构建快捷菜单）
/// </summary>
public partial class AppCardViewModel : ObservableObject
{
    public AppCardViewModel(IDesktopApp app)
    {
        App = app;
    }

    public IDesktopApp App { get; }

    public string Name => App.Name;

    public string Icon => App.Icon;

    public string Description => App.Description;
}
