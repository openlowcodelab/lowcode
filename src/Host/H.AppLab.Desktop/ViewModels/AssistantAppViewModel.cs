using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using H.Assistant.Application.Contracts;
using H.AppLab.Desktop.Services;

namespace H.AppLab.Desktop.ViewModels;

/// <summary>
/// 助手应用根 ViewModel（对应 Web 端 ChatLayout：会话侧栏 + 页面导航）
/// </summary>
public partial class AssistantAppViewModel : ObservableObject
{
    private readonly IChatMessageAppService _chatMessageAppService;

    public ToastService Toast { get; }
    public ChatViewModel Chat { get; }
    public KnowledgeViewModel Knowledge { get; }

    public ObservableCollection<SessionItem> Sessions { get; } = [];

    [ObservableProperty]
    private object? currentPage;

    [ObservableProperty]
    private bool isKnowledgePage;

    [ObservableProperty]
    private bool showUserMenu;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSessions))]
    private bool sessionsLoaded;

    public bool HasSessions => Sessions.Count > 0;

    /// <summary>会话标题是否可点击返回聊天</summary>
    public bool SessionTitleClickable => IsKnowledgePage;

    public AssistantAppViewModel(IChatMessageAppService chatMessageAppService, ToastService toast,
        ChatViewModel chat, KnowledgeViewModel knowledge)
    {
        _chatMessageAppService = chatMessageAppService;
        Toast = toast;
        Chat = chat;
        Knowledge = knowledge;

        Chat.SessionCreated += OnSessionCreated;
        Chat.SessionsChanged += () => _ = LoadSessionsAsync();

        CurrentPage = Chat;
        _ = InitializeAsync();
    }

    partial void OnIsKnowledgePageChanged(bool value) => OnPropertyChanged(nameof(SessionTitleClickable));

    private async Task InitializeAsync()
    {
        await Chat.InitializeAsync();
        await LoadSessionsAsync();
    }

    private async Task LoadSessionsAsync()
    {
        try
        {
            var result = await _chatMessageAppService.GetSessionsAsync(new SessionQueryDto { MaxResultCount = 50, SkipCount = 0 });
            var selectedId = Chat.SessionId;
            Sessions.Clear();
            foreach (var chat in result.Items)
            {
                Sessions.Add(new SessionItem(chat, this) { IsSelected = chat.Id == selectedId });
            }
        }
        catch (Exception ex)
        {
            Toast.Show($"加载会话失败: {ex.Message}", "error");
        }
        finally
        {
            SessionsLoaded = true;
            OnPropertyChanged(nameof(HasSessions));
        }
    }

    private void OnSessionCreated(ChatDto chat)
    {
        foreach (var session in Sessions)
        {
            session.IsSelected = false;
        }
        Sessions.Insert(0, new SessionItem(chat, this) { IsSelected = true });
        OnPropertyChanged(nameof(HasSessions));
    }

    [RelayCommand]
    private async Task NewChatAsync()
    {
        CloseMenus();
        foreach (var session in Sessions)
        {
            session.IsSelected = false;
        }
        IsKnowledgePage = false;
        CurrentPage = Chat;
        await Chat.StartNewChatAsync();
    }

    [RelayCommand]
    private async Task SelectSessionAsync(SessionItem session)
    {
        CloseMenus();
        foreach (var item in Sessions)
        {
            item.IsSelected = item == session;
        }
        IsKnowledgePage = false;
        CurrentPage = Chat;
        await Chat.OpenSessionAsync(session.Dto.Id);
    }

    [RelayCommand]
    private async Task GoToChatAsync()
    {
        CloseMenus();
        if (IsKnowledgePage || CurrentPage != Chat)
        {
            IsKnowledgePage = false;
            CurrentPage = Chat;
            if (!Chat.HasSession)
            {
                await Chat.StartNewChatAsync();
            }
        }
    }

    [RelayCommand]
    private async Task GoToKnowledgeAsync()
    {
        CloseMenus();
        if (!IsKnowledgePage)
        {
            IsKnowledgePage = true;
            CurrentPage = Knowledge;
            await Knowledge.InitializeAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteSessionAsync(SessionItem session)
    {
        var wasSelected = session.IsSelected;
        try
        {
            await _chatMessageAppService.DeleteSessionAsync(session.Dto.Id);
            CloseMenus();
            await LoadSessionsAsync();
            Toast.Show("会话已删除", "success");
        }
        catch (Exception ex)
        {
            Toast.Show($"删除失败: {ex.Message}", "error");
        }

        if (wasSelected)
        {
            await NewChatAsync();
        }
    }

    [RelayCommand]
    private void ToggleSessionMenu(SessionItem session)
    {
        var newValue = !session.IsMenuOpen;
        foreach (var item in Sessions)
        {
            item.IsMenuOpen = false;
        }
        session.IsMenuOpen = newValue;
    }

    [RelayCommand]
    private void ToggleUserMenu()
    {
        ShowUserMenu = !ShowUserMenu;
    }

    public void CloseMenus()
    {
        ShowUserMenu = false;
        foreach (var item in Sessions)
        {
            item.IsMenuOpen = false;
        }
    }
}

/// <summary>
/// 侧栏会话项
/// </summary>
public partial class SessionItem : ObservableObject
{
    private readonly AssistantAppViewModel _owner;

    public SessionItem(ChatDto dto, AssistantAppViewModel owner)
    {
        Dto = dto;
        _owner = owner;
    }

    public ChatDto Dto { get; }

    public string Title => Dto.Title;

    public string MessageCountText => $"{Dto.MessageCount} msgs";

    public string TimeText => Dto.LastMessageTime.ToString("MM-dd HH:mm");

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private bool isMenuOpen;

    [RelayCommand]
    private Task SelectAsync() => _owner.SelectSessionCommand.ExecuteAsync(this);

    [RelayCommand]
    private Task DeleteAsync() => _owner.DeleteSessionCommand.ExecuteAsync(this);
}
