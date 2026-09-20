using H.Workbench.Core;
using H.Workbench.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace H.Workbench.Application.Services.Execution;

/// <summary>
/// 一次执行一个实例：把事件 JSON 流转换为 TaskExecutionStep 行并增量落库。
/// 粒度规则——thinking 按 iteration 合并为一行（防流式 chunk 行数爆炸）；
/// tool_call/tool_result 各一行按 ToolCallId 配对；approval 请求插 Pending 行、裁决回填同一行。
/// 一切写入失败只记日志，绝不影响主执行。
/// </summary>
public class ExecutionTraceRecorder
{
    private readonly ExecutionTraceStore _store;
    private readonly TraceOptions _options;
    private readonly ILogger _logger;
    private readonly Guid _taskLogId;
    private readonly Guid _taskId;

    private readonly List<TaskExecutionStepEntity> _pending = new();
    private readonly List<ArtifactEntity> _pendingArtifacts = new();
    private readonly Dictionary<Guid, TaskExecutionStepEntity> _approvalRows = new();
    private readonly HashSet<Guid> _flushedApprovalIds = new();

    private int _seq;
    private int? _bufferIteration;
    private int _currentStepIndex;
    private TaskExecutionStepEntity? _activeStepHeader;
    private bool _stepHeaderPersisted;
    private readonly System.Text.StringBuilder _thinkingBuf = new();
    private int _stepCount;
    private int _artifactCount;
    private int _activeApprovals;
    private int _worstTerminalRank = -1;
    private string? _worstTerminal;

    public ExecutionTraceRecorder(
        ExecutionTraceStore store,
        TraceOptions options,
        ILogger logger,
        Guid taskLogId,
        Guid taskId)
    {
        _store = store;
        _options = options;
        _logger = logger;
        _taskLogId = taskLogId;
        _taskId = taskId;
    }

    /// <summary>
    /// 供 ProcessMessageAsync(onEventJson) 直接传引用的回调
    /// </summary>
    public Func<string, Task> Tap => HandleAsync;

    public async Task HandleAsync(string? eventJson)
    {
        if (!_options.Enabled) return;

        try
        {
            var e = StreamEventDecoder.Parse(eventJson);
            if (e is null) return;

            switch (e.Kind)
            {
                case StreamEventKind.Thinking:
                    await OnThinkingAsync(e);
                    break;
                case StreamEventKind.ToolCall:
                    FlushThinkingAs(e.Iteration, "Thinking");
                    AddRow("ToolCall", e.Iteration, toolName: e.ToolName, skillName: e.SkillName,
                        toolCallId: e.ToolCallId, arguments: e.Arguments);
                    break;
                case StreamEventKind.ToolResult:
                    AddRow("ToolResult", e.Iteration, toolName: e.ToolName, skillName: e.SkillName,
                        toolCallId: e.ToolCallId, result: e.Result, isError: e.IsError,
                        truncated: e.Truncated, durationMs: e.DurationMs);
                    CollectArtifacts(e);
                    await FlushAsync();
                    await FlushArtifactsAsync();
                    break;
                case StreamEventKind.ApprovalRequired:
                    FlushThinkingAs(e.Iteration, "Thinking");
                    OnApprovalRequired(e);
                    break;
                case StreamEventKind.ApprovalResolved:
                    await OnApprovalResolvedAsync(e);
                    break;
                case StreamEventKind.Answer:
                    OnAnswer(e);
                    await FlushAsync();
                    break;
                case StreamEventKind.Error:
                    FlushThinkingAs(e.Iteration, "Thinking");
                    AddRow("Error", e.Iteration, content: e.Message, isError: e.IsFatal);
                    await FlushAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "执行轨迹记录失败（不影响主执行） TaskLogId={TaskLogId}", _taskLogId);
        }
    }

    /// <summary>
    /// 收尾：冲掉残余行，返回计数与审批聚合状态供回写 TaskLog
    /// </summary>
    public async Task<(int StepCount, int ArtifactCount, string? ApprovalState)> FinishAsync()
    {
        if (!_options.Enabled) return (0, 0, null);

        try
        {
            FlushThinkingAs(_bufferIteration ?? 0, "Thinking");
            await FlushAsync();
            await FlushArtifactsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "执行轨迹收尾失败 TaskLogId={TaskLogId}", _taskLogId);
        }

        return (_stepCount, _artifactCount, AggregateApprovalState());
    }

    /// <summary>
    /// 工作流步骤开始：插入 Kind=Step 头部行，后续所有行归属该步骤
    /// </summary>
    public async Task BeginStepAsync(int stepIndex, string stepName)
    {
        _currentStepIndex = stepIndex;
        FlushThinkingAs(_bufferIteration ?? 0, "Thinking");
        await FlushAsync();

        _stepHeaderPersisted = false;
        _activeStepHeader = new TaskExecutionStepEntity
        {
            TaskLogId = _taskLogId,
            TaskId = _taskId,
            StepIndex = stepIndex,
            Iteration = stepIndex,
            Seq = ++_seq,
            Kind = "Step",
            ToolName = stepName,
            Content = "执行中",
            StartedAt = DateTime.Now
        };
        _pending.Add(_activeStepHeader);
    }

    /// <summary>
    /// 工作流步骤收尾：回填头部行状态（未落库则原地改，已落库则短 UoW 更新）
    /// </summary>
    public async Task EndStepAsync(string state, string? error = null)
    {
        if (_activeStepHeader is null) return;

        var header = _activeStepHeader;
        _activeStepHeader = null;
        FlushThinkingAs(_bufferIteration ?? header.Iteration, "Thinking");

        header.Content = error is null ? state : $"{state}：{Truncate(error, 500, out _)}";
        header.IsError = state != "Success";

        if (_pending.Contains(header))
        {
            return; // 随下一次 flush 落库
        }

        if (_stepHeaderPersisted)
        {
            try
            {
                await _store.UpdateStepOutcomeAsync(header.Id, header.Content, header.IsError);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "步骤头部行状态回填失败 TaskLogId={TaskLogId}", _taskLogId);
            }
        }

        await FlushAsync();
    }

    private Task OnThinkingAsync(DecodedEvent e)
    {
        if (_bufferIteration.HasValue && _bufferIteration.Value != e.Iteration)
        {
            FlushThinkingAs(_bufferIteration.Value, "Thinking");
        }

        _bufferIteration = e.Iteration;
        _thinkingBuf.Append(e.Content);

        // 防御：单轮思考过长时提前落一段，避免无界内存增长
        if (_thinkingBuf.Length > _options.MaxContentChars * 4)
        {
            FlushThinkingAs(e.Iteration, "Thinking");
        }

        return Task.CompletedTask;
    }

    private void OnAnswer(DecodedEvent e)
    {
        var content = e.Content;
        if (string.IsNullOrEmpty(content) && _bufferIteration == e.Iteration)
        {
            // FinalAnswerEvent 为空的历史口径：答案即本轮累积的思考增量
            content = _thinkingBuf.ToString();
        }
        ClearThinkingBuffer();

        if (!string.IsNullOrEmpty(content))
        {
            AddRow("Answer", e.Iteration, content: content);
        }
    }

    private void OnApprovalRequired(DecodedEvent e)
    {
        var row = new TaskExecutionStepEntity
        {
            TaskLogId = _taskLogId,
            TaskId = _taskId,
            Iteration = e.Iteration,
            Seq = ++_seq,
            Kind = "Approval",
            ToolName = e.ToolName,
            SkillName = e.SkillName,
            ToolCallId = e.ToolCallId,
            Arguments = Truncate(e.Arguments, _options.MaxArgumentsChars, out _),
            ApprovalState = "Pending",
            StartedAt = DateTime.Now
        };
        _pending.Add(row);
        _approvalRows[e.ApprovalId] = row;
        _activeApprovals++;
        MergeApprovalAggregate("Pending");
    }

    private async Task OnApprovalResolvedAsync(DecodedEvent e)
    {
        if (!_approvalRows.TryGetValue(e.ApprovalId, out var row))
        {
            return;
        }

        row.ApprovalState = MapDecision(e.Decision);
        row.ApproverId = e.ApproverId ?? (e.Decision == "SkippedNonInteractive" ? "auto-policy" : row.ApproverId);
        row.WaitMs = e.WaitMs.HasValue ? (int)Math.Min(e.WaitMs.Value, int.MaxValue) : null;
        if (_approvalRows.Remove(e.ApprovalId))
        {
            _activeApprovals = Math.Max(0, _activeApprovals - 1);
        }
        MergeApprovalAggregate(row.ApprovalState);

        if (_pending.Contains(row))
        {
            return; // 尚未落库，Flush 时随行写入
        }

        if (_flushedApprovalIds.Contains(row.Id))
        {
            await _store.UpdateApprovalStepAsync(row.Id, row.ApprovalState!, row.ApproverId, row.WaitMs);
        }
    }

    private static string MapDecision(string? decision) => decision switch
    {
        "Approved" => "Approved",
        "DeniedByUser" => "Denied",
        "Timeout" => "Timeout",
        "Cancelled" => "Denied",
        "SkippedNonInteractive" => "SkippedNonInteractive",
        _ => "Denied"
    };

    /// <summary>
    /// 聚合徽标：有进行中的审批时=Pending；全部裁决后取最差终态（Timeout > Denied > Skipped > Approved）
    /// </summary>
    private void MergeApprovalAggregate(string state)
    {
        if (state == "Pending") return;

        var rank = ApprovalRank(state);
        if (rank > _worstTerminalRank)
        {
            _worstTerminalRank = rank;
            _worstTerminal = state;
        }
    }

    private static int ApprovalRank(string? state) => state switch
    {
        "Timeout" => 5,
        "Denied" => 4,
        "SkippedNonInteractive" => 3,
        "Approved" => 1,
        _ => -1
    };

    private string? AggregateApprovalState() => _activeApprovals > 0 ? "Pending" : _worstTerminal;

    private void FlushThinkingAs(int iteration, string kind)
    {
        if (_thinkingBuf.Length == 0)
        {
            return;
        }

        AddRow(kind, _bufferIteration ?? iteration, content: _thinkingBuf.ToString());
        ClearThinkingBuffer();
    }

    private void ClearThinkingBuffer()
    {
        _thinkingBuf.Clear();
        _bufferIteration = null;
    }

    private void AddRow(
        string kind,
        int iteration,
        string? toolName = null,
        string? skillName = null,
        string? toolCallId = null,
        string? content = null,
        string? arguments = null,
        string? result = null,
        bool isError = false,
        bool truncated = false,
        int? durationMs = null)
    {
        content = Truncate(content, _options.MaxContentChars, out var contentCut);
        arguments = Truncate(arguments, _options.MaxArgumentsChars, out var argsCut);

        _pending.Add(new TaskExecutionStepEntity
        {
            TaskLogId = _taskLogId,
            TaskId = _taskId,
            StepIndex = _currentStepIndex,
            Iteration = iteration,
            Seq = ++_seq,
            Kind = kind,
            ToolName = toolName,
            SkillName = skillName,
            ToolCallId = toolCallId,
            Content = content,
            Arguments = arguments,
            Result = result,
            IsError = isError,
            Truncated = truncated || contentCut || argsCut,
            DurationMs = durationMs,
            StartedAt = DateTime.Now
        });
    }

    private static string? Truncate(string? text, int max, out bool truncated)
    {
        truncated = false;
        if (string.IsNullOrEmpty(text) || max <= 0 || text.Length <= max)
        {
            return text;
        }

        truncated = true;
        return text[..max] + "\n...[已截断]";
    }

    private async Task FlushAsync()
    {
        if (_pending.Count == 0) return;

        var rows = _pending.ToList();
        _pending.Clear();

        foreach (var row in rows)
        {
            if (row.Kind == "Approval")
            {
                _flushedApprovalIds.Add(row.Id);
            }
            if (ReferenceEquals(row, _activeStepHeader))
            {
                _stepHeaderPersisted = true;
            }
        }

        await _store.AppendStepsAsync(rows);
        _stepCount += rows.Count;
    }

    /// <summary>
    /// 从成功的工具结果派生产物；同一下一次 flush 节奏落库，不额外增加 UoW 次数
    /// </summary>
    private void CollectArtifacts(DecodedEvent e)
    {
        if (e.IsError) return;

        foreach (var artifact in ArtifactExtractor.TryExtract(e.ToolName, e.Arguments, e.Result))
        {
            artifact.TaskLogId = _taskLogId;
            artifact.TaskId = _taskId;
            artifact.Iteration = e.Iteration;
            artifact.ToolCallId = e.ToolCallId;
            _pendingArtifacts.Add(artifact);
        }
    }

    private async Task FlushArtifactsAsync()
    {
        if (_pendingArtifacts.Count == 0) return;

        var drafts = _pendingArtifacts.ToList();
        _pendingArtifacts.Clear();
        await _store.AddArtifactsAsync(drafts);
        _artifactCount += drafts.Count;
    }
}
