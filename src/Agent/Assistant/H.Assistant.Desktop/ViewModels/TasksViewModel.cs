using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using H.Assistant.Application.Contracts;
using H.Assistant.Desktop.Services;

namespace H.Assistant.Desktop.ViewModels;

/// <summary>
/// 定时任务页 ViewModel（对应 Web 端 Tasks.razor）
/// </summary>
public partial class TasksViewModel : ObservableObject
{
    private readonly ITaskAppService _taskAppService;
    private readonly ITaskLogAppService _taskLogAppService;
    private readonly IChatMessageAppService _chatMessageAppService;
    private readonly ToastService _toast;

    private bool _initialized;

    public ObservableCollection<TaskCardItem> Tasks { get; } = [];
    public ObservableCollection<TaskLogItem> TaskLogs { get; } = [];
    public ObservableCollection<AgentConfigDto> AvailableAgents { get; } = [];

    public List<ScheduleTypeOption> ScheduleTypeOptions { get; } =
    [
        new("Once", "仅一次"),
        new("Daily", "每天"),
        new("Weekly", "每周"),
        new("Monthly", "每月"),
        new("Cron", "Cron 表达式")
    ];

    public List<ScheduleTypeOption> DayOfWeekOptions { get; } =
    [
        new("1", "周一"), new("2", "周二"), new("3", "周三"), new("4", "周四"),
        new("5", "周五"), new("6", "周六"), new("0", "周日")
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTasksTab))]
    [NotifyPropertyChangedFor(nameof(IsLogsTab))]
    private string activeTab = "tasks";

    public bool IsTasksTab => ActiveTab == "tasks";
    public bool IsLogsTab => ActiveTab == "logs";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTasks))]
    private bool loadingTasks;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasLogs))]
    private bool loadingTaskLogs;

    public bool HasTasks => !LoadingTasks && Tasks.Count > 0;
    public bool HasLogs => !LoadingTaskLogs && TaskLogs.Count > 0;

    [ObservableProperty]
    private bool showTaskDialog;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DialogTitle))]
    [NotifyPropertyChangedFor(nameof(DialogSaveText))]
    private bool isEditingTask;

    public string DialogTitle => IsEditingTask ? "编辑定时任务" : "创建定时任务";
    public string DialogSaveText => IsEditingTask ? "保存" : "创建";

    private Guid? _editingTaskId;

    public TaskEditModel EditTaskModel { get; } = new();

    /// <summary>对话框中选中的 Agent（与 EditTaskModel.AgentType 同步）</summary>
    [ObservableProperty]
    private AgentConfigDto? dialogSelectedAgent;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectAllText))]
    private bool isLogSelectionMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedLog))]
    [NotifyPropertyChangedFor(nameof(HasSelectedLog))]
    private TaskLogItem? selectedLogItem;

    public TaskLogItem? SelectedLog => SelectedLogItem;
    public bool HasSelectedLog => SelectedLogItem != null;

    [ObservableProperty]
    private bool showBatchDeleteConfirm;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BatchDeleteMessage))]
    [NotifyPropertyChangedFor(nameof(CanBatchDelete))]
    private int selectedLogCount;

    public string BatchDeleteMessage => $"确定要删除选中的 {SelectedLogCount} 条执行日志吗？此操作不可撤销。";
    public bool CanBatchDelete => SelectedLogCount > 0;

    public string SelectAllText => TaskLogs.Count > 0 && TaskLogs.All(l => l.IsChecked) ? "取消全选" : "全选";

    public TasksViewModel(ITaskAppService taskAppService, ITaskLogAppService taskLogAppService,
        IChatMessageAppService chatMessageAppService, ToastService toast)
    {
        _taskAppService = taskAppService;
        _taskLogAppService = taskLogAppService;
        _chatMessageAppService = chatMessageAppService;
        _toast = toast;
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }
        _initialized = true;
        await LoadAvailableAgentsAsync();
        await LoadTasksAsync();
        await LoadTaskLogsAsync();
    }

    [RelayCommand]
    private async Task SwitchTabAsync(string tab)
    {
        ActiveTab = tab;
        CloseTaskMenus();
        if (tab == "logs")
        {
            await LoadTaskLogsAsync();
        }
        else
        {
            await LoadTasksAsync();
        }
    }

    private async Task LoadAvailableAgentsAsync()
    {
        try
        {
            var agents = await _chatMessageAppService.GetAvailableAgentsAsync();
            AvailableAgents.Clear();
            foreach (var agent in agents)
            {
                AvailableAgents.Add(agent);
            }
        }
        catch (Exception ex)
        {
            _toast.Show($"加载 Agent 列表失败: {ex.Message}", "error");
        }
    }

    private async Task LoadTasksAsync()
    {
        LoadingTasks = true;
        try
        {
            var result = await _taskAppService.GetListAsync(new TaskQueryDto { MaxResultCount = 50 });
            Tasks.Clear();
            foreach (var task in result.Items)
            {
                Tasks.Add(new TaskCardItem(task, this));
            }
        }
        catch (Exception ex)
        {
            _toast.Show($"加载任务失败: {ex.Message}", "error");
        }
        finally
        {
            LoadingTasks = false;
            OnPropertyChanged(nameof(HasTasks));
        }
    }

    private async Task LoadTaskLogsAsync()
    {
        LoadingTaskLogs = true;
        try
        {
            var result = await _taskLogAppService.GetListAsync(new TaskLogQueryDto { MaxResultCount = 50 });
            TaskLogs.Clear();
            foreach (var log in result.Items)
            {
                var item = new TaskLogItem(log);
                item.CheckedChanged += OnLogCheckedChanged;
                TaskLogs.Add(item);
            }
            SelectedLogItem = TaskLogs.FirstOrDefault();
            SelectedLogCount = 0;
        }
        catch (Exception ex)
        {
            _toast.Show($"加载执行记录失败: {ex.Message}", "error");
        }
        finally
        {
            LoadingTaskLogs = false;
            OnPropertyChanged(nameof(HasLogs));
            OnPropertyChanged(nameof(SelectAllText));
        }
    }

    private void OnLogCheckedChanged()
    {
        SelectedLogCount = TaskLogs.Count(l => l.IsChecked);
        OnPropertyChanged(nameof(SelectAllText));
    }

    [RelayCommand]
    private void SelectLog(TaskLogItem log)
    {
        SelectedLogItem = log;
        foreach (var item in TaskLogs)
        {
            item.IsSelected = item == log;
        }
    }

    #region 任务 CRUD

    [RelayCommand]
    private void CreateNewTask()
    {
        _editingTaskId = null;
        IsEditingTask = false;
        EditTaskModel.Reset(AvailableAgents.FirstOrDefault()?.AgentType ?? "");
        DialogSelectedAgent = AvailableAgents.FirstOrDefault();
        ShowTaskDialog = true;
    }

    [RelayCommand]
    private void EditTask(TaskCardItem item)
    {
        CloseTaskMenus();
        _editingTaskId = item.Dto.Id;
        IsEditingTask = true;
        EditTaskModel.LoadFrom(item.Dto);
        DialogSelectedAgent = AvailableAgents.FirstOrDefault(a => a.AgentType == item.Dto.AgentType)
            ?? AvailableAgents.FirstOrDefault();
        ShowTaskDialog = true;
    }

    [RelayCommand]
    private void CloseTaskDialog()
    {
        ShowTaskDialog = false;
        _editingTaskId = null;
    }

    [RelayCommand]
    private async Task SaveScheduledTaskAsync()
    {
        if (string.IsNullOrWhiteSpace(EditTaskModel.TaskName) || string.IsNullOrWhiteSpace(EditTaskModel.PromptContent))
        {
            return;
        }

        EditTaskModel.AgentType = DialogSelectedAgent?.AgentType ?? "";

        try
        {
            if (_editingTaskId.HasValue)
            {
                await _taskAppService.UpdateAsync(_editingTaskId.Value, EditTaskModel.ToUpdateDto());
                _toast.Show("任务已更新", "success");
            }
            else
            {
                await _taskAppService.CreateAsync(EditTaskModel.ToCreateDto());
                _toast.Show("任务已创建", "success");
            }
            CloseTaskDialog();
            await LoadTasksAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"保存失败: {ex.Message}", "error");
        }
    }

    [RelayCommand]
    private async Task ToggleTaskEnableAsync(TaskCardItem item)
    {
        try
        {
            await _taskAppService.ToggleEnableAsync(item.Dto.Id);
            await LoadTasksAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"操作失败: {ex.Message}", "error");
        }
    }

    [RelayCommand]
    private async Task ExecuteTaskNowAsync(TaskCardItem item)
    {
        CloseTaskMenus();
        try
        {
            await _taskAppService.ExecuteNowAsync(item.Dto.Id);
            _toast.Show("任务已触发执行", "success");
            await LoadTasksAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"执行失败: {ex.Message}", "error");
        }
    }

    [RelayCommand]
    private async Task DeleteScheduledTaskAsync(TaskCardItem item)
    {
        CloseTaskMenus();
        try
        {
            await _taskAppService.DeleteAsync(item.Dto.Id);
            _toast.Show("任务已删除", "success");
            await LoadTasksAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"删除失败: {ex.Message}", "error");
        }
    }

    [RelayCommand]
    private void ToggleTaskMenu(TaskCardItem item)
    {
        var newValue = !item.IsMenuOpen;
        CloseTaskMenus();
        item.IsMenuOpen = newValue;
    }

    private void CloseTaskMenus()
    {
        foreach (var task in Tasks)
        {
            task.IsMenuOpen = false;
        }
    }

    #endregion

    #region 执行日志多选删除

    [RelayCommand]
    private void ToggleLogSelectionMode()
    {
        IsLogSelectionMode = !IsLogSelectionMode;
        if (!IsLogSelectionMode)
        {
            ClearLogSelection();
        }
    }

    [RelayCommand]
    private void ExitLogSelectionMode()
    {
        IsLogSelectionMode = false;
        ClearLogSelection();
    }

    [RelayCommand]
    private void ToggleLogSelectAll()
    {
        var allSelected = TaskLogs.Count > 0 && TaskLogs.All(l => l.IsChecked);
        foreach (var log in TaskLogs)
        {
            log.IsChecked = !allSelected;
        }
        OnLogCheckedChanged();
    }

    [RelayCommand]
    private void ConfirmBatchDeleteLogs()
    {
        if (SelectedLogCount > 0)
        {
            ShowBatchDeleteConfirm = true;
        }
    }

    [RelayCommand]
    private void CancelBatchDelete()
    {
        ShowBatchDeleteConfirm = false;
    }

    [RelayCommand]
    private async Task BatchDeleteLogsAsync()
    {
        foreach (var log in TaskLogs.Where(l => l.IsChecked).ToList())
        {
            await _taskLogAppService.DeleteAsync(log.Dto.Id);
        }

        ShowBatchDeleteConfirm = false;
        IsLogSelectionMode = false;
        await LoadTaskLogsAsync();
    }

    private void ClearLogSelection()
    {
        foreach (var log in TaskLogs)
        {
            log.IsChecked = false;
        }
        SelectedLogCount = 0;
    }

    #endregion
}

public record ScheduleTypeOption(string Value, string Label);

/// <summary>
/// 任务卡片项
/// </summary>
public partial class TaskCardItem : ObservableObject
{
    private readonly TasksViewModel _owner;

    public TaskCardItem(TaskDto dto, TasksViewModel owner)
    {
        Dto = dto;
        _owner = owner;
    }

    public TaskDto Dto { get; }

    public string TaskName => Dto.TaskName;
    public string TaskDescription => Dto.TaskDescription;
    public bool HasDescription => !string.IsNullOrEmpty(Dto.TaskDescription);
    public bool IsEnabled => Dto.IsEnabled;
    public string ScheduleDisplayText => Dto.ScheduleDisplayText;
    public bool ShowNextExecution => Dto.IsEnabled && Dto.NextExecutionTime.HasValue;
    public string NextExecutionText => Dto.NextExecutionTime.HasValue ? $"下次执行: {Dto.NextExecutionTime.Value:MM-dd HH:mm}" : "";
    public double CardOpacity => Dto.IsEnabled ? 1.0 : 0.55;

    [ObservableProperty]
    private bool isMenuOpen;

    [RelayCommand]
    private void EditTask() => _owner.EditTaskCommand.Execute(this);

    [RelayCommand]
    private Task ExecuteNowAsync() => _owner.ExecuteTaskNowCommand.ExecuteAsync(this);

    [RelayCommand]
    private Task DeleteAsync() => _owner.DeleteScheduledTaskCommand.ExecuteAsync(this);

    [RelayCommand]
    private Task ToggleEnableAsync() => _owner.ToggleTaskEnableCommand.ExecuteAsync(this);
}

/// <summary>
/// 执行日志项
/// </summary>
public partial class TaskLogItem : ObservableObject
{
    public TaskLogItem(TaskLogDto dto)
    {
        Dto = dto;
    }

    public event Action? CheckedChanged;

    public TaskLogDto Dto { get; }

    public string TaskName => Dto.TaskName;
    public string StartTimeText => Dto.StartTime.ToString("MM-dd HH:mm");
    public string FullStartTimeText => Dto.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
    public bool HasDuration => Dto.DurationSeconds.HasValue;
    public string DurationText => Dto.DurationSeconds.HasValue ? $"{Dto.DurationSeconds.Value}秒" : "";
    public string? Result => Dto.Result;
    public bool HasResult => !string.IsNullOrEmpty(Dto.Result);
    public string? ErrorMessage => Dto.ErrorMessage;
    public bool HasError => !string.IsNullOrEmpty(Dto.ErrorMessage);

    public string StatusText => Dto.Status switch
    {
        "Success" => "成功",
        "Failed" => "失败",
        "Running" => "运行中",
        _ => Dto.Status
    };

    public string StatusColor => Dto.Status switch
    {
        "Success" => "#52c41a",
        "Failed" => "#ff4d4f",
        "Running" => "#1890ff",
        _ => "#8c8c8c"
    };

    public string StatusBackground => Dto.Status switch
    {
        "Success" => "#f6ffed",
        "Failed" => "#fff1f0",
        "Running" => "#e6f7ff",
        _ => "#fafafa"
    };

    [ObservableProperty]
    private bool isSelected;

    private bool _isChecked;

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (SetProperty(ref _isChecked, value))
            {
                CheckedChanged?.Invoke();
            }
        }
    }
}

/// <summary>
/// 任务创建/编辑表单模型
/// </summary>
public partial class TaskEditModel : ObservableObject
{
    [ObservableProperty]
    private string taskName = string.Empty;

    [ObservableProperty]
    private string taskDescription = string.Empty;

    [ObservableProperty]
    private string promptContent = string.Empty;

    [ObservableProperty]
    private string agentType = string.Empty;

    public Guid? ModelConfigId { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowTimeConfig))]
    [NotifyPropertyChangedFor(nameof(ShowCronConfig))]
    [NotifyPropertyChangedFor(nameof(ShowWeeklyConfig))]
    [NotifyPropertyChangedFor(nameof(ShowMonthlyConfig))]
    private ScheduleTypeOption? scheduleType;

    [ObservableProperty]
    private string? cronExpression;

    [ObservableProperty]
    private decimal? hour;

    [ObservableProperty]
    private decimal? minute;

    [ObservableProperty]
    private ScheduleTypeOption? dayOfWeek;

    [ObservableProperty]
    private decimal? dayOfMonth;

    public bool IsEnabled { get; set; } = true;

    public bool ShowTimeConfig => ScheduleType?.Value is "Daily" or "Weekly" or "Monthly";
    public bool ShowCronConfig => ScheduleType?.Value == "Cron";
    public bool ShowWeeklyConfig => ScheduleType?.Value == "Weekly";
    public bool ShowMonthlyConfig => ScheduleType?.Value == "Monthly";

    public void Reset(string defaultAgentType)
    {
        TaskName = string.Empty;
        TaskDescription = string.Empty;
        PromptContent = string.Empty;
        AgentType = defaultAgentType;
        ModelConfigId = null;
        ScheduleType = new ScheduleTypeOption("Daily", "每天");
        CronExpression = null;
        Hour = 9;
        Minute = 0;
        DayOfWeek = null;
        DayOfMonth = 1;
        IsEnabled = true;
    }

    public void LoadFrom(TaskDto task)
    {
        TaskName = task.TaskName;
        TaskDescription = task.TaskDescription;
        PromptContent = task.PromptContent;
        AgentType = task.AgentType;
        ModelConfigId = task.ModelConfigId;
        ScheduleType = new ScheduleTypeOption(task.ScheduleType,
            task.ScheduleType switch { "Once" => "仅一次", "Daily" => "每天", "Weekly" => "每周", "Monthly" => "每月", "Cron" => "Cron 表达式", _ => task.ScheduleType });
        CronExpression = task.CronExpression;
        Hour = task.Hour;
        Minute = task.Minute;
        DayOfWeek = task.DayOfWeek.HasValue
            ? new ScheduleTypeOption(task.DayOfWeek.Value.ToString(), DayOfWeekLabel(task.DayOfWeek.Value))
            : null;
        DayOfMonth = task.DayOfMonth;
        IsEnabled = task.IsEnabled;
    }

    private static string DayOfWeekLabel(int day) => day switch
    {
        0 => "周日", 1 => "周一", 2 => "周二", 3 => "周三", 4 => "周四", 5 => "周五", 6 => "周六", _ => ""
    };

    public CreateTaskDto ToCreateDto() => new()
    {
        TaskName = TaskName,
        TaskDescription = TaskDescription,
        PromptContent = PromptContent,
        AgentType = AgentType,
        ModelConfigId = ModelConfigId,
        ScheduleType = ScheduleType?.Value ?? "Daily",
        CronExpression = CronExpression,
        Hour = (int?)Hour,
        Minute = (int?)Minute,
        DayOfWeek = DayOfWeek != null ? int.Parse(DayOfWeek.Value) : null,
        DayOfMonth = (int?)DayOfMonth,
        IsEnabled = IsEnabled
    };

    public UpdateTaskDto ToUpdateDto() => new()
    {
        TaskName = TaskName,
        TaskDescription = TaskDescription,
        PromptContent = PromptContent,
        AgentType = AgentType,
        ModelConfigId = ModelConfigId,
        ScheduleType = ScheduleType?.Value ?? "Daily",
        CronExpression = CronExpression,
        Hour = (int?)Hour,
        Minute = (int?)Minute,
        DayOfWeek = DayOfWeek != null ? int.Parse(DayOfWeek.Value) : null,
        DayOfMonth = (int?)DayOfMonth,
        IsEnabled = IsEnabled
    };
}
