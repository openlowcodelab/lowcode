using System.Collections.ObjectModel;
using System.Text.Encodings.Web;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using H.Assistant.Application.Contracts;
using H.Assistant.UI.Services;

namespace H.Assistant.UI.ViewModels;

/// <summary>
/// 聊天页 ViewModel（对应 Web 端 Chat.razor + ChatInput.razor）
/// </summary>
public partial class ChatViewModel : ObservableObject
{
    private readonly IChatMessageAppService _chatMessageAppService;
    private readonly ILLMAppService _llmAppService;
    private readonly ChatStreamClient _streamClient;
    private readonly ToastService _toast;

    private CancellationTokenSource? _streamCts;

    /// <summary>新会话创建成功（用于侧栏置顶插入）</summary>
    public event Action<ChatDto>? SessionCreated;

    /// <summary>会话列表需要刷新</summary>
    public event Action? SessionsChanged;

    /// <summary>消息区滚动到底部</summary>
    public event Action? ScrollToBottomRequested;

    public ObservableCollection<ChatMessageItem> Messages { get; } = [];
    public ObservableCollection<ReactStepItem> ReactSteps { get; } = [];
    public ObservableCollection<AgentConfigDto> AvailableAgents { get; } = [];
    public ObservableCollection<LLMDto> AvailableModels { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSession))]
    private Guid? sessionId;

    [ObservableProperty]
    private bool loadingMessages;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    [NotifyPropertyChangedFor(nameof(ShowThinkingIndicator))]
    private bool isSending;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private string inputMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStreamingResponse))]
    [NotifyPropertyChangedFor(nameof(ShowThinkingIndicator))]
    [NotifyPropertyChangedFor(nameof(ShowPlainStreaming))]
    private string streamingResponse = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReactAnswerContent))]
    [NotifyPropertyChangedFor(nameof(HasReactAnswer))]
    private string historicalAnswer = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowThinkingIndicator))]
    [NotifyPropertyChangedFor(nameof(ShowPlainStreaming))]
    private bool hasReactSteps;

    [ObservableProperty]
    private AgentConfigDto? selectedAgent;

    [ObservableProperty]
    private LLMDto? selectedModel;

    [ObservableProperty]
    private bool showAgentDropdown;

    [ObservableProperty]
    private bool showModelDropdown;

    public bool HasSession => SessionId.HasValue;

    public bool HasStreamingResponse => !string.IsNullOrEmpty(StreamingResponse);

    /// <summary>ReAct 区域的最终回答（流式中优先显示流式内容）</summary>
    public string ReactAnswerContent => !string.IsNullOrEmpty(StreamingResponse) ? StreamingResponse : HistoricalAnswer;

    public bool HasReactAnswer => !string.IsNullOrEmpty(ReactAnswerContent);

    /// <summary>纯流式回复（无 ReAct 步骤时）</summary>
    public bool ShowPlainStreaming => !HasReactSteps && HasStreamingResponse;

    /// <summary>等待响应的三点动画</summary>
    public bool ShowThinkingIndicator => IsSending && string.IsNullOrEmpty(StreamingResponse) && !HasReactSteps;

    public string SelectedAgentDisplayName => SelectedAgent?.DisplayName ?? "智能体";

    public string SelectedModelDisplay => SelectedModel == null ? "未配置模型" : $"{SelectedModel.ProviderDisplayName} ({SelectedModel.Model})";

    public ChatViewModel(IChatMessageAppService chatMessageAppService, ILLMAppService llmAppService,
        ChatStreamClient streamClient, ToastService toast)
    {
        _chatMessageAppService = chatMessageAppService;
        _llmAppService = llmAppService;
        _streamClient = streamClient;
        _toast = toast;
    }

    partial void OnStreamingResponseChanged(string value) => OnPropertyChanged(nameof(ReactAnswerContent));

    partial void OnSelectedAgentChanged(AgentConfigDto? value) => OnPropertyChanged(nameof(SelectedAgentDisplayName));

    partial void OnSelectedModelChanged(LLMDto? value) => OnPropertyChanged(nameof(SelectedModelDisplay));

    public async Task InitializeAsync()
    {
        await LoadAvailableAgentsAsync();
        await LoadAvailableModelsAsync();
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
            SelectedAgent ??= AvailableAgents.FirstOrDefault();
        }
        catch
        {
            // 与 Web 端一致：初始化失败静默
        }
    }

    private async Task LoadAvailableModelsAsync()
    {
        try
        {
            var allConfigs = await _llmAppService.GetAllAsync();
            var enabled = allConfigs.Where(c => c.IsEnabled).ToList();
            AvailableModels.Clear();
            foreach (var model in enabled)
            {
                AvailableModels.Add(model);
            }

            if (SelectedModel == null || AvailableModels.All(m => m.Id != SelectedModel.Id))
            {
                SelectedModel = AvailableModels.FirstOrDefault(m => m.IsDefault) ?? AvailableModels.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            AvailableModels.Clear();
            _toast.Show($"Load models failed: {ex.Message}", "error");
        }
    }

    /// <summary>
    /// 开始新会话（清空当前状态）
    /// </summary>
    public async Task StartNewChatAsync()
    {
        _streamCts?.Cancel();
        SessionId = null;
        Messages.Clear();
        ReactSteps.Clear();
        HasReactSteps = false;
        StreamingResponse = string.Empty;
        HistoricalAnswer = string.Empty;
        InputMessage = string.Empty;
        await LoadAvailableModelsAsync();
    }

    /// <summary>
    /// 打开指定会话
    /// </summary>
    public async Task OpenSessionAsync(Guid id)
    {
        if (SessionId == id)
        {
            return;
        }

        SessionId = id;
        InputMessage = string.Empty;
        StreamingResponse = string.Empty;
        await LoadMessagesAsync(id);
    }

    private async Task LoadMessagesAsync(Guid sessionId)
    {
        LoadingMessages = true;
        try
        {
            var messages = await _chatMessageAppService.GetMessagesAsync(sessionId);

            ReactSteps.Clear();
            HasReactSteps = false;
            HistoricalAnswer = string.Empty;

            // 检查最后一条助手消息是否包含嵌入的 ReAct 步骤数据（富内容格式）
            var lastAssistantMsg = messages.LastOrDefault(m => m.Role == "assistant");
            if (lastAssistantMsg != null && !string.IsNullOrEmpty(lastAssistantMsg.Content))
            {
                TryParseReactHistory(lastAssistantMsg);
            }

            Messages.Clear();
            var renderMessages = messages;
            if (HasReactSteps && messages.Count > 0 && messages[^1].Role == "assistant")
            {
                // 最后一条助手消息由 ReAct 区域替代显示
                renderMessages = messages.Take(messages.Count - 1).ToList();
            }

            foreach (var message in renderMessages)
            {
                Messages.Add(new ChatMessageItem(message.Role, message.Content, message.CreationTime));
            }

            ScrollToBottomRequested?.Invoke();
        }
        finally
        {
            LoadingMessages = false;
        }
    }

    private void TryParseReactHistory(ChatMessageDto lastAssistantMsg)
    {
        try
        {
            using var doc = JsonDocument.Parse(lastAssistantMsg.Content);
            var root = doc.RootElement;
            if (!root.TryGetProperty("answer", out var answerProp) ||
                !root.TryGetProperty("reactSteps", out var reactStepsProp))
            {
                return;
            }

            var answerContent = answerProp.GetString() ?? "";
            foreach (var stepElem in reactStepsProp.EnumerateArray())
            {
                var type = stepElem.TryGetProperty("type", out var t) ? t.GetString() ?? "" : "";
                var iteration = stepElem.TryGetProperty("iteration", out var i) ? i.GetInt32() : 0;
                var step = ReactSteps.LastOrDefault(s => s.Iteration == iteration);

                switch (type)
                {
                    case "thinking":
                        if (step == null) { step = new ReactStepItem { Iteration = iteration }; ReactSteps.Add(step); }
                        step.ThinkingContent = stepElem.TryGetProperty("content", out var tc) ? tc.GetString() ?? "" : "";
                        break;

                    case "tool_call":
                        if (step == null) { step = new ReactStepItem { Iteration = iteration }; ReactSteps.Add(step); }
                        step.ToolCalls.Add(new ToolCallItem
                        {
                            ToolName = stepElem.TryGetProperty("toolName", out var tn) ? tn.GetString() ?? "" : "",
                            Arguments = stepElem.TryGetProperty("arguments", out var args) ? args.GetString() ?? "" : "",
                            IsExecuting = false
                        });
                        break;

                    case "tool_result":
                        var rToolName = stepElem.TryGetProperty("toolName", out var rtn) ? rtn.GetString() ?? "" : "";
                        var rResult = stepElem.TryGetProperty("result", out var res) ? res.GetString() ?? "" : "";
                        var rIsError = stepElem.TryGetProperty("isError", out var ie) && ie.GetBoolean();
                        var toolInfo = ReactSteps.SelectMany(st => st.ToolCalls)
                            .LastOrDefault(tc2 => tc2.ToolName == rToolName && tc2.Result == null);
                        if (toolInfo != null)
                        {
                            toolInfo.Result = rResult;
                            toolInfo.IsError = rIsError;
                        }
                        break;
                }
            }

            HasReactSteps = ReactSteps.Any();
            HistoricalAnswer = answerContent;

            // 更新消息内容为纯文本答案（防止列表中渲染 JSON）
            lastAssistantMsg.Content = answerContent;

            // 兜底：有 ReAct 步骤但没有最终回答时降级为普通消息显示
            if (HasReactSteps && string.IsNullOrEmpty(HistoricalAnswer))
            {
                HasReactSteps = false;
            }
        }
        catch
        {
            ReactSteps.Clear();
            HasReactSteps = false;
        }
    }

    private bool CanSend() => !string.IsNullOrWhiteSpace(InputMessage) && !IsSending;

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        var message = InputMessage;
        InputMessage = string.Empty;
        IsSending = true;
        CloseDropdowns();

        var isNewSession = !SessionId.HasValue;
        var newSessionTitle = message.Length > 30 ? message[..30] + "..." : message;

        var pendingMessage = new ChatMessageItem("user", message, DateTime.Now);
        Messages.Add(pendingMessage);
        StreamingResponse = string.Empty;
        HistoricalAnswer = string.Empty;
        ReactSteps.Clear();
        HasReactSteps = false;
        ScrollToBottomRequested?.Invoke();

        _streamCts?.Cancel();
        _streamCts = new CancellationTokenSource();

        try
        {
            var input = new SendChatMessageInputDto
            {
                SessionId = SessionId,
                Message = message,
                AgentType = string.IsNullOrEmpty(SelectedAgent?.AgentType) ? null : SelectedAgent.AgentType,
                ProviderName = SelectedModel?.Id.ToString(),
                ModelConfigId = SelectedModel?.Id
            };

            await foreach (var data in _streamClient.StreamAsync(input, _streamCts.Token))
            {
                HandleStreamChunk(data, isNewSession ? newSessionTitle : null);
            }

            if (SessionId.HasValue)
            {
                StreamingResponse = string.Empty;
                await LoadMessagesAsync(SessionId.Value);
                SessionsChanged?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            // 用户主动停止
        }
        catch (Exception ex)
        {
            InputMessage = message;
            Messages.Remove(pendingMessage);
            StreamingResponse = string.Empty;
            _toast.Show($"Send failed: {ex.Message}", "error");
        }
        finally
        {
            IsSending = false;
        }
    }

    private void HandleStreamChunk(string data, string? newSessionTitle)
    {
        if (data.TrimStart().StartsWith('{'))
        {
            try
            {
                using var doc = JsonDocument.Parse(data);
                var root = doc.RootElement;

                // Session 事件
                if (root.TryGetProperty("session", out var s))
                {
                    var newSessionId = Guid.Parse(s.GetString()!);
                    SessionId = newSessionId;
                    if (!string.IsNullOrEmpty(newSessionTitle))
                    {
                        SessionCreated?.Invoke(new ChatDto
                        {
                            Id = newSessionId,
                            Title = newSessionTitle,
                            CreationTime = DateTime.Now,
                            LastMessageTime = DateTime.Now,
                            MessageCount = 1
                        });
                    }
                    return;
                }

                // ReAct 类型化事件
                if (root.TryGetProperty("type", out var typeProp))
                {
                    var eventType = typeProp.GetString();
                    var iteration = root.TryGetProperty("iteration", out var iterProp) ? iterProp.GetInt32() : 0;
                    var step = ReactSteps.LastOrDefault(st => st.Iteration == iteration);

                    switch (eventType)
                    {
                        case "thinking":
                            if (step == null) { step = new ReactStepItem { Iteration = iteration }; ReactSteps.Add(step); }
                            step.ThinkingContent += root.TryGetProperty("content", out var tc) ? tc.GetString() ?? "" : "";
                            HasReactSteps = true;
                            ScrollToBottomRequested?.Invoke();
                            return;

                        case "tool_call":
                            if (step == null) { step = new ReactStepItem { Iteration = iteration }; ReactSteps.Add(step); }
                            step.ToolCalls.Add(new ToolCallItem
                            {
                                ToolName = root.TryGetProperty("toolName", out var tn) ? tn.GetString() ?? "" : "",
                                Arguments = root.TryGetProperty("arguments", out var args) ? args.GetString() ?? "" : "",
                                IsExecuting = true
                            });
                            HasReactSteps = true;
                            ScrollToBottomRequested?.Invoke();
                            return;

                        case "tool_result":
                            var rToolName = root.TryGetProperty("toolName", out var rtn) ? rtn.GetString() ?? "" : "";
                            var rResult = root.TryGetProperty("result", out var res) ? res.GetString() ?? "" : "";
                            var rIsError = root.TryGetProperty("isError", out var ie) && ie.GetBoolean();
                            var toolInfo = ReactSteps.SelectMany(st => st.ToolCalls)
                                .LastOrDefault(tc2 => tc2.ToolName == rToolName && tc2.IsExecuting);
                            if (toolInfo != null)
                            {
                                toolInfo.Result = rResult;
                                toolInfo.IsError = rIsError;
                                toolInfo.IsExecuting = false;
                            }
                            ScrollToBottomRequested?.Invoke();
                            return;

                        case "answer":
                            var ansContent = root.TryGetProperty("content", out var ac) ? ac.GetString() ?? "" : "";
                            if (!string.IsNullOrEmpty(ansContent))
                            {
                                StreamingResponse += ansContent;
                                ScrollToBottomRequested?.Invoke();
                            }
                            return;

                        case "error":
                            var errMsg = root.TryGetProperty("message", out var em) ? em.GetString() ?? "Unknown error" : "Unknown error";
                            var isFatal = root.TryGetProperty("isFatal", out var fat) && fat.GetBoolean();
                            if (isFatal)
                            {
                                throw new InvalidOperationException(errMsg);
                            }
                            _toast.Show(errMsg, "warning");
                            return;
                    }
                }

                // 向后兼容：旧格式 error
                if (root.TryGetProperty("error", out var e))
                {
                    throw new InvalidOperationException(e.GetString() ?? "Unknown error");
                }
            }
            catch (JsonException)
            {
                // 非合法 JSON，按纯文本处理
            }
        }

        // 非 JSON 纯文本（向后兼容）
        StreamingResponse += data;
        ScrollToBottomRequested?.Invoke();
    }

    [RelayCommand]
    private void Stop()
    {
        _streamCts?.Cancel();
        IsSending = false;
    }

    [RelayCommand]
    private void ToggleAgentDropdown()
    {
        ShowModelDropdown = false;
        ShowAgentDropdown = !ShowAgentDropdown;
    }

    [RelayCommand]
    private void ToggleModelDropdown()
    {
        ShowAgentDropdown = false;
        ShowModelDropdown = !ShowModelDropdown;
    }

    [RelayCommand]
    private void SelectAgent(AgentConfigDto agent)
    {
        SelectedAgent = agent;
        ShowAgentDropdown = false;
    }

    [RelayCommand]
    private void SelectModel(LLMDto model)
    {
        SelectedModel = model;
        ShowModelDropdown = false;
    }

    public void CloseDropdowns()
    {
        ShowAgentDropdown = false;
        ShowModelDropdown = false;
    }
}

/// <summary>
/// 聊天消息项
/// </summary>
public partial class ChatMessageItem : ObservableObject
{
    public ChatMessageItem(string role, string content, DateTime time)
    {
        Role = role;
        Content = content;
        Time = time;
    }

    public string Role { get; }

    [ObservableProperty]
    private string content;

    public DateTime Time { get; }

    public bool IsUser => Role == "user";

    public string TimeText => Time.ToString("HH:mm:ss");
}

/// <summary>
/// ReAct 步骤（一次 LLM 迭代）
/// </summary>
public partial class ReactStepItem : ObservableObject
{
    public int Iteration { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasThinking))]
    private string thinkingContent = string.Empty;

    public bool HasThinking => !string.IsNullOrEmpty(ThinkingContent);

    public ObservableCollection<ToolCallItem> ToolCalls { get; } = [];
}

/// <summary>
/// 工具调用信息
/// </summary>
public partial class ToolCallItem : ObservableObject
{
    public string ToolName { get; set; } = string.Empty;

    public string Arguments { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResult))]
    [NotifyPropertyChangedFor(nameof(TruncatedResult))]
    private string? result;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ResultLabel))]
    private bool isError;

    [ObservableProperty]
    private bool isExecuting;

    public bool HasArguments => !string.IsNullOrEmpty(Arguments);

    public bool HasResult => Result != null;

    public string ResultLabel => IsError ? "错误" : "结果";

    public string TruncatedResult
    {
        get
        {
            var text = Result ?? string.Empty;
            return text.Length > 500 ? text[..500] + "..." : text;
        }
    }

    /// <summary>友好的工具名称（与 Web 端一致）</summary>
    public string FriendlyToolName => ToolName switch
    {
        "SearchAsync" => "网络搜索",
        "SearchNewsAsync" => "新闻搜索",
        "FetchPageAsync" => "访问网页",
        "ExtractTextAsync" => "提取文本",
        "ExtractLinksAsync" => "提取链接",
        "CheckUrlAsync" => "检查链接",
        _ => ToolName
    };

    /// <summary>格式化 JSON 参数（缩进 + 不转义中文）</summary>
    public string FormattedArguments
    {
        get
        {
            if (string.IsNullOrEmpty(Arguments))
            {
                return string.Empty;
            }
            try
            {
                using var doc = JsonDocument.Parse(Arguments);
                return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            }
            catch
            {
                return Arguments;
            }
        }
    }
}
