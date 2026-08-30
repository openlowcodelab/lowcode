using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using H.AppLab.Desktop.Services;
using H.Assistant.Application.Contracts;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace H.AppLab.Desktop.ViewModels;

/// <summary>
/// 任务页 ViewModel（支持任务分类、提示词/工作流创建、手动/自动执行）
/// </summary>
public partial class TasksViewModel : ObservableObject
{
    private readonly ITaskAppService _taskAppService;
    private readonly ITaskLogAppService _taskLogAppService;
    private readonly IChatMessageAppService _chatMessageAppService;
    private readonly ToastService _toast;
    private readonly CategoryService _categoryService;
    private readonly IAiCompletionAppService _aiCompletionAppService;

    private bool _initialized;

    /// <summary>全量任务列表（后端返回）</summary>
    public ObservableCollection<TaskCardItem> Tasks { get; } = [];

    /// <summary>按分类分组后的任务列表（界面绑定）</summary>
    public ObservableCollection<TaskCategoryGroup> TaskGroups { get; } = [];

    /// <summary>分类筛选项（首项为"全部"）</summary>
    public ObservableCollection<CategoryChipItem> FilterCategories { get; } = [];

    public ObservableCollection<TaskLogItem> TaskLogs { get; } = [];
    public ObservableCollection<AgentConfigDto> AvailableAgents { get; } = [];

    /// <summary>任务分类可选项（创建/编辑对话框使用）</summary>
    public ObservableCollection<string> CategoryOptions { get; } = [];

    /// <summary>当前选中的分类（"全部"表示不过滤）</summary>
    [ObservableProperty]
    private string selectedCategory = "全部";

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

    /// <summary>是否处于某个任务的执行记录视图（false 为任务列表视图）</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTasksView))]
    private bool isLogsView;

    public bool IsTasksView => !IsLogsView;

    /// <summary>当前查看执行记录的任务名称</summary>
    [ObservableProperty]
    private string logsTaskName = string.Empty;

    /// <summary>当前查看执行记录的任务 Id</summary>
    private Guid? _logsTaskId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTasks))]
    private bool loadingTasks;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasLogs))]
    private bool loadingTaskLogs;

    public bool HasTasks => !LoadingTasks && Tasks.Count > 0;
    public bool HasTaskGroups => !LoadingTasks && TaskGroups.Count > 0;
    public bool HasLogs => !LoadingTaskLogs && TaskLogs.Count > 0;

    [ObservableProperty]
    private bool showTaskDialog;

    /// <summary>是否显示 AI 生成任务对话框</summary>
    [ObservableProperty]
    private bool showAiDialog;

    /// <summary>AI 生成：用户输入的口语化描述</summary>
    [ObservableProperty]
    private string aiDescription = string.Empty;

    /// <summary>AI 生成：是否正在调用 AI 解析</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AiGenerateButtonText))]
    private bool isAiGenerating;

    public string AiGenerateButtonText => IsAiGenerating ? "生成中..." : "生成";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DialogTitle))]
    [NotifyPropertyChangedFor(nameof(DialogSaveText))]
    private bool isEditingTask;

    public string DialogTitle => IsEditingTask ? "编辑任务" : "创建任务";
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
        IChatMessageAppService chatMessageAppService, ToastService toast, CategoryService categoryService,
        IAiCompletionAppService aiCompletionAppService)
    {
        _taskAppService = taskAppService;
        _taskLogAppService = taskLogAppService;
        _chatMessageAppService = chatMessageAppService;
        _toast = toast;
        _categoryService = categoryService;
        _aiCompletionAppService = aiCompletionAppService;
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }
        _initialized = true;
        try
        {
            await _categoryService.LoadAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"加载分类失败: {ex.Message}", "error");
        }
        RefreshCategoryOptions();
        await LoadAvailableAgentsAsync();
        await LoadTasksAsync();
    }

    private void RefreshCategoryOptions()
    {
        CategoryOptions.Clear();
        foreach (var category in _categoryService.Categories)
        {
            CategoryOptions.Add(category.Name);
        }
    }

    /// <summary>查看某个任务的执行记录</summary>
    [RelayCommand]
    private async Task ViewTaskLogsAsync(TaskCardItem item)
    {
        CloseTaskMenus();
        _logsTaskId = item.Dto.Id;
        LogsTaskName = item.Dto.TaskName;
        IsLogsView = true;
        await LoadTaskLogsAsync();
    }

    /// <summary>从执行记录视图返回任务列表</summary>
    [RelayCommand]
    private void BackToTasks()
    {
        IsLogsView = false;
        ExitLogSelectionMode();
    }

    private async Task LoadAvailableAgentsAsync()
    {
        try
        {
            var agents = (await _chatMessageAppService.GetAvailableAgentsAsync()).Data ?? [];
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
            var result = (await _taskAppService.GetListAsync(new TaskQueryDto { MaxResultCount = 50 })).Data!;
            Tasks.Clear();
            foreach (var task in result.Items)
            {
                Tasks.Add(new TaskCardItem(task, this));
            }
            RebuildFilterCategories();
            ApplyCategoryFilter();
        }
        catch (Exception ex)
        {
            _toast.Show($"加载任务失败: {ex.Message}", "error");
        }
        finally
        {
            LoadingTasks = false;
            OnPropertyChanged(nameof(HasTasks));
            OnPropertyChanged(nameof(HasTaskGroups));
        }
    }

    /// <summary>重建分类筛选项（全部 + 所有已配置的分类）</summary>
    private void RebuildFilterCategories()
    {
        var names = new List<string> { "全部" };
        names.AddRange(_categoryService.Categories.Select(c => c.Name));

        if (SelectedCategory != "全部" && !names.Contains(SelectedCategory))
        {
            SelectedCategory = "全部";
        }

        FilterCategories.Clear();
        foreach (var name in names)
        {
            FilterCategories.Add(new CategoryChipItem { Name = name, IsActive = name == SelectedCategory });
        }
    }

    /// <summary>按当前选中分类过滤任务列表，并按分类分组</summary>
    private void ApplyCategoryFilter()
    {
        var filtered = Tasks.Where(t =>
            SelectedCategory == "全部" || string.IsNullOrEmpty(SelectedCategory) || t.Category == SelectedCategory);

        // 按分类分组，有分类的在前，无分类的"其它"在最后
        var grouped = filtered
            .GroupBy(t => string.IsNullOrWhiteSpace(t.Category) ? "其它" : t.Category)
            .OrderBy(g => g.Key == "其它" ? 1 : 0)
            .ThenBy(g => g.Key);

        TaskGroups.Clear();
        foreach (var group in grouped)
        {
            TaskGroups.Add(new TaskCategoryGroup
            {
                CategoryName = group.Key,
                Tasks = new ObservableCollection<TaskCardItem>(group)
            });
        }
        OnPropertyChanged(nameof(HasTaskGroups));
    }

    [RelayCommand]
    private void SelectCategory(string category)
    {
        if (SelectedCategory == category)
        {
            return;
        }
        SelectedCategory = category;
        foreach (var chip in FilterCategories)
        {
            chip.IsActive = chip.Name == category;
        }
        ApplyCategoryFilter();
    }

    private async Task LoadTaskLogsAsync()
    {
        LoadingTaskLogs = true;
        try
        {
            var result = (await _taskLogAppService.GetListAsync(new TaskLogQueryDto { TaskId = _logsTaskId, MaxResultCount = 50 })).Data!;
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
        if (string.IsNullOrWhiteSpace(EditTaskModel.TaskName))
        {
            _toast.Show("请输入任务名称", "error");
            return;
        }
        if (EditTaskModel.IsPromptSource && string.IsNullOrWhiteSpace(EditTaskModel.PromptContent))
        {
            _toast.Show("请输入提示词内容", "error");
            return;
        }
        if (EditTaskModel.IsWorkflowSource && !EditTaskModel.HasValidWorkflowSteps)
        {
            _toast.Show("请至少配置一个有效的工作流步骤", "error");
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

    /// <summary>切换创建方式（提示词/工作流）</summary>
    [RelayCommand]
    private void SetSourceType(string sourceType)
    {
        EditTaskModel.SourceType = sourceType;
    }

    /// <summary>切换执行方式（手动/自动）</summary>
    [RelayCommand]
    private void SetExecutionMode(string executionMode)
    {
        EditTaskModel.ExecutionMode = executionMode;
    }

    /// <summary>添加工作流步骤</summary>
    [RelayCommand]
    private void AddWorkflowStep()
    {
        EditTaskModel.AddStep();
    }

    /// <summary>删除工作流步骤</summary>
    [RelayCommand]
    private void RemoveWorkflowStep(WorkflowStepEditModel step)
    {
        EditTaskModel.RemoveStep(step);
    }

    private void CloseTaskMenus()
    {
        foreach (var task in Tasks)
        {
            task.IsMenuOpen = false;
        }
    }

    #endregion

    #region AI 生成任务

    /// <summary>打开 AI 生成任务对话框</summary>
    [RelayCommand]
    private void OpenAiGenerate()
    {
        AiDescription = string.Empty;
        ShowAiDialog = true;
    }

    /// <summary>关闭 AI 生成任务对话框（生成中禁止关闭）</summary>
    [RelayCommand]
    private void CloseAiGenerate()
    {
        if (IsAiGenerating)
        {
            return;
        }
        ShowAiDialog = false;
    }

    /// <summary>调用 AI 解析口语描述并直接创建任务</summary>
    [RelayCommand]
    private async Task GenerateTaskFromAiAsync()
    {
        var description = AiDescription?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(description))
        {
            _toast.Show("请输入任务描述", "error");
            return;
        }
        if (IsAiGenerating)
        {
            return;
        }

        IsAiGenerating = true;
        try
        {
            // 已有分类列表随用户消息传入，约束 AI 只能从中选择分类
            var categoryHint = CategoryOptions.Count > 0
                ? string.Join("、", CategoryOptions)
                : "（暂无分类）";
            var userMessage =
                $"已有任务分类（category 只能从中选择，都不合适则输出空字符串）：{categoryHint}\n\n" +
                $"用户描述：\n{description}";

            var result = (await _aiCompletionAppService.CompleteAsync(new AiCompletionInputDto
            {
                SystemPrompt = AiGenerateTaskSystemPrompt,
                UserMessage = userMessage,
                Temperature = 0.3f,
                MaxTokens = 2048
            })).Data;

            var config = ParseAiTaskConfig(result?.Content ?? string.Empty);
            if (config == null || string.IsNullOrWhiteSpace(config.TaskName))
            {
                _toast.Show("AI 返回内容无法解析，请调整描述后重试", "error");
                return;
            }

            var dto = BuildCreateTaskDto(config, description);
            if (dto == null)
            {
                return;
            }

            await _taskAppService.CreateAsync(dto);
            _toast.Show("任务已创建", "success");
            ShowAiDialog = false;
            AiDescription = string.Empty;
            await LoadTasksAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"AI 生成失败: {ex.Message}", "error");
        }
        finally
        {
            IsAiGenerating = false;
        }
    }

    /// <summary>将 AI 解析结果归一化并组装为 CreateTaskDto，校验失败返回 null（已 toast）</summary>
    private CreateTaskDto? BuildCreateTaskDto(AiTaskConfig config, string fallbackPrompt)
    {
        // 仅明确识别为 Manual 才是手动任务，其余按自动处理
        var executionMode = string.Equals(config.ExecutionMode?.Trim(), "Manual", StringComparison.OrdinalIgnoreCase)
            ? "Manual"
            : "Auto";

        // 调度类型白名单归一化；手动任务不参与调度，用应用默认值 Daily 占位
        var scheduleType = (config.ScheduleType ?? string.Empty).Trim();
        if (executionMode == "Manual")
        {
            scheduleType = "Daily";
        }
        if (scheduleType is not ("Once" or "Daily" or "Weekly" or "Monthly" or "Cron"))
        {
            scheduleType = "Daily";
        }

        // 数值范围过滤，越界视为未提供
        int? hour = config.Hour is >= 0 and <= 23 ? config.Hour : null;
        int? minute = config.Minute is >= 0 and <= 59 ? config.Minute : null;
        int? dayOfWeek = config.DayOfWeek is >= 0 and <= 6 ? config.DayOfWeek : null;
        int? dayOfMonth = config.DayOfMonth is >= 1 and <= 31 ? config.DayOfMonth : null;

        // 自动任务按调度类型校验必填项
        if (executionMode == "Auto")
        {
            switch (scheduleType)
            {
                case "Cron" when string.IsNullOrWhiteSpace(config.CronExpression):
                    _toast.Show("未能从描述中识别出调度规则，请写得更具体（如：每 2 小时一次）", "error");
                    return null;
                case "Weekly" when dayOfWeek == null:
                    _toast.Show("未能识别星期几，请明确说明（如：每周一早上 9 点）", "error");
                    return null;
            }
        }

        // 时间兜底
        hour ??= 9;
        minute ??= 0;
        dayOfMonth ??= 1;

        // 分类必须命中已有分类（忽略大小写），否则置空落入"其它"分组
        var category = CategoryOptions.FirstOrDefault(c =>
            string.Equals(c, config.Category?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? string.Empty;

        return new CreateTaskDto
        {
            TaskName = Truncate(config.TaskName!.Trim(), 100),
            TaskDescription = Truncate(config.TaskDescription, 500),
            Category = category,
            SourceType = "Prompt",
            WorkflowContent = null,
            ExecutionMode = executionMode,
            PromptContent = string.IsNullOrWhiteSpace(config.PromptContent)
                ? fallbackPrompt
                : config.PromptContent.Trim(),
            AgentType = AvailableAgents.FirstOrDefault()?.AgentType ?? string.Empty,
            ModelConfigId = null,
            ScheduleType = scheduleType,
            CronExpression = scheduleType == "Cron" ? config.CronExpression?.Trim() : null,
            Hour = scheduleType == "Cron" ? null : hour,
            Minute = scheduleType == "Cron" ? null : minute,
            DayOfWeek = scheduleType == "Weekly" ? dayOfWeek : null,
            DayOfMonth = scheduleType == "Monthly" ? dayOfMonth : null,
            IsEnabled = true
        };
    }

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }
        value = value.Trim();
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static readonly JsonSerializerOptions AiJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        // 容忍 AI 把数字输出成字符串（如 "9"）
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
    };

    /// <summary>解析 AI 返回文本中的任务配置，失败返回 null</summary>
    private static AiTaskConfig? ParseAiTaskConfig(string content)
    {
        var json = ExtractJson(content);
        if (json.Length == 0)
        {
            return null;
        }
        try
        {
            return JsonSerializer.Deserialize<AiTaskConfig>(json, AiJsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>从 AI 返回文本中提取 JSON（去 markdown 代码块，取第一个 { 到最后一个 }）</summary>
    private static string ExtractJson(string content)
    {
        var text = (content ?? string.Empty).Trim();
        if (text.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewline = text.IndexOf('\n');
            if (firstNewline > 0)
            {
                text = text[(firstNewline + 1)..];
            }
            var lastFence = text.LastIndexOf("```", StringComparison.Ordinal);
            if (lastFence > 0)
            {
                text = text[..lastFence];
            }
        }
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        return start < 0 || end <= start ? string.Empty : text[start..(end + 1)];
    }

    private const string AiGenerateTaskSystemPrompt = """
        你是一个定时任务规划助手。请根据用户的口语化描述，生成一个定时任务的配置。
        输出要求：
        1. 只输出一个 JSON 对象，不要输出任何解释文字，也不要使用 markdown 代码块标记。
        2. JSON 结构如下：
        {
          "taskName": "任务名称（简洁概括，不超过20字）",
          "taskDescription": "任务描述（用户意图的一句话总结）",
          "category": "任务分类（从用户给出的已有分类列表中选择，没有合适的则输出空字符串）",
          "promptContent": "任务执行时给 Agent 的提示词（完整、明确、可直接执行的指令）",
          "executionMode": "Auto 或 Manual",
          "scheduleType": "Once、Daily、Weekly、Monthly 或 Cron",
          "cronExpression": "Cron 表达式（仅 scheduleType 为 Cron 时填写，否则为 null）",
          "hour": 9,
          "minute": 0,
          "dayOfWeek": 1,
          "dayOfMonth": 1
        }
        3. 规则：
        - executionMode：用户描述了定时或周期执行时用 Auto；仅当用户明确表示手动触发、或完全未提及执行时间时用 Manual。
        - scheduleType：只执行一次用 Once；每天执行用 Daily；每周执行用 Weekly；每月执行用 Monthly；更复杂的周期（如每 2 小时、仅工作日、每 3 天）用 Cron 并填写 cronExpression。
        - hour 为 0-23 的整数，minute 为 0-59 的整数。用户未说具体时间时按语义推断：早上=9:00，中午=12:00，下午=15:00，晚上=20:00；未提及分钟时 minute 为 0。
        - dayOfWeek 仅 Weekly 时填写，取值 0-6，其中 0=周日、1=周一、2=周二、3=周三、4=周四、5=周五、6=周六；非 Weekly 时为 null。
        - dayOfMonth 仅 Monthly 时填写，取值 1-31；非 Monthly 时为 null。
        - cronExpression 为 5 段格式"分 时 日 月 周"，例如每个工作日 9 点为 "0 9 * * 1-5"，每 2 小时为 "0 */2 * * *"。
        - category 必须严格从用户消息中给出的已有分类列表里选择最匹配的一个（原样输出分类名）；列表为空或都不合适时输出空字符串，严禁创造新分类。
        - promptContent 基于用户描述整理成一条完整的执行指令，聚焦"做什么"，不要包含时间或调度信息；保留用户提到的关键对象、范围与要求。
        - taskName、taskDescription、promptContent 使用中文。
        """;

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

    #region 分类管理

    [ObservableProperty]
    private string newCategoryName = string.Empty;

    [ObservableProperty]
    private string editingCategoryId = string.Empty;

    [ObservableProperty]
    private bool isAddingCategory;

    [RelayCommand]
    private void StartAddCategory()
    {
        NewCategoryName = string.Empty;
        IsAddingCategory = true;
        // 关闭所有分组的编辑状态
        foreach (var g in TaskGroups)
        {
            g.IsEditing = false;
        }
    }

    [RelayCommand]
    private void CancelAddCategory()
    {
        IsAddingCategory = false;
        NewCategoryName = string.Empty;
    }

    [RelayCommand]
    private async Task ConfirmAddCategoryAsync()
    {
        var name = NewCategoryName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            _toast.Show("分类名称不能为空", "error");
            return;
        }

        var result = await _categoryService.AddAsync(name);
        if (result != null)
        {
            RefreshCategoryOptions();
            RebuildFilterCategories();
            IsAddingCategory = false;
            NewCategoryName = string.Empty;
            _toast.Show("分类已添加", "success");
        }
        else
        {
            _toast.Show("分类名称已存在", "error");
        }
    }

    [RelayCommand]
    private void StartEditCategory(TaskCategoryGroup group)
    {
        if (group.CategoryName == "其它") return;
        var item = _categoryService.Categories.FirstOrDefault(c => c.Name == group.CategoryName);
        if (item == null) return;

        // 关闭其他分组的编辑状态
        foreach (var g in TaskGroups)
        {
            g.IsEditing = false;
        }

        EditingCategoryId = item.Id.ToString();
        group.IsEditing = true;
        group.EditName = item.Name;
    }

    [RelayCommand]
    private void CancelEditCategory(TaskCategoryGroup group)
    {
        group.IsEditing = false;
        EditingCategoryId = string.Empty;
    }

    [RelayCommand]
    private async Task ConfirmEditCategoryAsync(TaskCategoryGroup group)
    {
        var name = group.EditName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            _toast.Show("分类名称不能为空", "error");
            return;
        }

        if (!Guid.TryParse(EditingCategoryId, out var id)) return;

        var oldItem = _categoryService.Categories.FirstOrDefault(c => c.Id == id);
        var oldName = oldItem?.Name;

        var result = await _categoryService.UpdateAsync(id, name);
        if (result != null)
        {
            // 更新任务中的分类名
            if (oldName != null && oldName != name)
            {
                foreach (var task in Tasks.Where(t => t.Category == oldName))
                {
                    task.Dto.Category = name;
                }
            }
            RefreshCategoryOptions();
            RebuildFilterCategories();
            ApplyCategoryFilter();
            EditingCategoryId = string.Empty;
            _toast.Show("分类已重命名", "success");
        }
        else
        {
            _toast.Show("分类名称已存在", "error");
        }
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(TaskCategoryGroup group)
    {
        if (group.CategoryName == "其它") return;
        var item = _categoryService.Categories.FirstOrDefault(c => c.Name == group.CategoryName);
        if (item == null) return;

        if (await _categoryService.DeleteAsync(item.Id))
        {
            // 清空该分类下任务的分类字段
            foreach (var task in Tasks.Where(t => t.Category == group.CategoryName))
            {
                task.Dto.Category = string.Empty;
            }
            RefreshCategoryOptions();
            RebuildFilterCategories();
            ApplyCategoryFilter();
            _toast.Show("分类已删除", "success");
        }
    }

    #endregion
}

public record ScheduleTypeOption(string Value, string Label);

/// <summary>
/// 分类筛选 chip 项
/// </summary>
public partial class CategoryChipItem : ObservableObject
{
    public string Name { get; set; } = string.Empty;

    [ObservableProperty]
    private bool isActive;
}

/// <summary>
/// 任务分类分组
/// </summary>
public partial class TaskCategoryGroup : ObservableObject
{
    public string CategoryName { get; set; } = string.Empty;

    public ObservableCollection<TaskCardItem> Tasks { get; set; } = [];

    public bool HasTasks => Tasks.Count > 0;

    public bool IsOtherGroup => CategoryName == "其它";

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private string editName = string.Empty;
}

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
    public bool ShowNextExecution => Dto.IsEnabled && Dto.ExecutionMode != "Manual" && Dto.NextExecutionTime.HasValue;
    public string NextExecutionText => Dto.NextExecutionTime.HasValue ? $"下次执行: {Dto.NextExecutionTime.Value:MM-dd HH:mm}" : "";
    public double CardOpacity => Dto.IsEnabled ? 1.0 : 0.55;

    /// <summary>任务分类</summary>
    public string Category => Dto.Category;
    public bool HasCategory => !string.IsNullOrWhiteSpace(Dto.Category);

    /// <summary>创建方式图标（提示词/工作流）</summary>
    public string SourceTypeIcon => Dto.SourceType == "Workflow" ? "🔀" : "💬";
    public string SourceTypeDisplayText => Dto.SourceTypeDisplayText;

    /// <summary>执行方式（手动/自动）</summary>
    public string ExecutionModeDisplayText => Dto.ExecutionModeDisplayText;
    public bool IsManual => Dto.ExecutionMode == "Manual";

    [ObservableProperty]
    private bool isMenuOpen;

    [RelayCommand]
    private void EditTask() => _owner.EditTaskCommand.Execute(this);

    [RelayCommand]
    private Task ExecuteNowAsync() => _owner.ExecuteTaskNowCommand.ExecuteAsync(this);

    [RelayCommand]
    private Task ViewLogsAsync() => _owner.ViewTaskLogsCommand.ExecuteAsync(this);

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

    /// <summary>任务分类</summary>
    [ObservableProperty]
    private string category = string.Empty;

    /// <summary>创建方式：Prompt(提示词)/Workflow(工作流)</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPromptSource))]
    [NotifyPropertyChangedFor(nameof(IsWorkflowSource))]
    private string sourceType = "Prompt";

    /// <summary>执行方式：Manual(手动)/Auto(自动)</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsManualMode))]
    [NotifyPropertyChangedFor(nameof(IsAutoMode))]
    private string executionMode = "Auto";

    [ObservableProperty]
    private string promptContent = string.Empty;

    [ObservableProperty]
    private string agentType = string.Empty;

    public Guid? ModelConfigId { get; set; }

    /// <summary>工作流步骤列表</summary>
    public ObservableCollection<WorkflowStepEditModel> WorkflowSteps { get; } = [];

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

    public bool IsPromptSource => SourceType == "Prompt";
    public bool IsWorkflowSource => SourceType == "Workflow";
    public bool IsManualMode => ExecutionMode == "Manual";
    public bool IsAutoMode => ExecutionMode == "Auto";

    public bool ShowTimeConfig => ScheduleType?.Value is "Daily" or "Weekly" or "Monthly";
    public bool ShowCronConfig => ScheduleType?.Value == "Cron";
    public bool ShowWeeklyConfig => ScheduleType?.Value == "Weekly";
    public bool ShowMonthlyConfig => ScheduleType?.Value == "Monthly";

    /// <summary>是否存在有效的工作流步骤（名称或提示词不为空）</summary>
    public bool HasValidWorkflowSteps => WorkflowSteps.Any(s =>
        !string.IsNullOrWhiteSpace(s.Name) || !string.IsNullOrWhiteSpace(s.Prompt));

    public void AddStep()
    {
        WorkflowSteps.Add(new WorkflowStepEditModel { Name = $"步骤 {WorkflowSteps.Count + 1}" });
    }

    public void RemoveStep(WorkflowStepEditModel step)
    {
        WorkflowSteps.Remove(step);
    }

    public void Reset(string defaultAgentType)
    {
        TaskName = string.Empty;
        TaskDescription = string.Empty;
        Category = string.Empty;
        SourceType = "Prompt";
        ExecutionMode = "Auto";
        PromptContent = string.Empty;
        AgentType = defaultAgentType;
        ModelConfigId = null;
        WorkflowSteps.Clear();
        AddStep();
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
        Category = task.Category;
        SourceType = string.IsNullOrWhiteSpace(task.SourceType) ? "Prompt" : task.SourceType;
        ExecutionMode = string.IsNullOrWhiteSpace(task.ExecutionMode) ? "Auto" : task.ExecutionMode;
        PromptContent = task.PromptContent;
        AgentType = task.AgentType;
        ModelConfigId = task.ModelConfigId;
        WorkflowSteps.Clear();
        if (!string.IsNullOrWhiteSpace(task.WorkflowContent))
        {
            try
            {
                var steps = JsonSerializer.Deserialize<List<WorkflowStepDto>>(task.WorkflowContent);
                if (steps != null)
                {
                    foreach (var step in steps)
                    {
                        WorkflowSteps.Add(new WorkflowStepEditModel { Name = step.Name, Prompt = step.Prompt });
                    }
                }
            }
            catch
            {
                // 忽略无效的工作流内容
            }
        }
        if (SourceType == "Workflow" && WorkflowSteps.Count == 0)
        {
            AddStep();
        }
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
        0 => "周日",
        1 => "周一",
        2 => "周二",
        3 => "周三",
        4 => "周四",
        5 => "周五",
        6 => "周六",
        _ => ""
    };

    /// <summary>序列化工作流步骤为 JSON（非工作流任务返回 null）</summary>
    private string? BuildWorkflowContent()
    {
        if (!IsWorkflowSource)
        {
            return null;
        }
        var steps = WorkflowSteps
            .Where(s => !string.IsNullOrWhiteSpace(s.Name) || !string.IsNullOrWhiteSpace(s.Prompt))
            .Select(s => new WorkflowStepDto { Name = s.Name, Prompt = s.Prompt })
            .ToList();
        return JsonSerializer.Serialize(steps);
    }

    public CreateTaskDto ToCreateDto() => new()
    {
        TaskName = TaskName,
        TaskDescription = TaskDescription,
        Category = Category,
        SourceType = SourceType,
        WorkflowContent = BuildWorkflowContent(),
        ExecutionMode = ExecutionMode,
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
        Category = Category,
        SourceType = SourceType,
        WorkflowContent = BuildWorkflowContent(),
        ExecutionMode = ExecutionMode,
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

/// <summary>
/// 工作流步骤编辑模型
/// </summary>
public partial class WorkflowStepEditModel : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string prompt = string.Empty;
}

/// <summary>
/// AI 生成的任务配置（字段名对应系统提示词中的 JSON，反序列化大小写不敏感）
/// </summary>
public class AiTaskConfig
{
    public string? TaskName { get; set; }
    public string? TaskDescription { get; set; }
    public string? Category { get; set; }
    public string? PromptContent { get; set; }
    public string? ExecutionMode { get; set; }
    public string? ScheduleType { get; set; }
    public string? CronExpression { get; set; }
    public int? Hour { get; set; }
    public int? Minute { get; set; }
    public int? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
}
