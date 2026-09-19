namespace H.Workbench.Application.Contracts;

/// <summary>
/// 流式执行任务入参
/// </summary>
public class ExecuteTaskStreamInputDto
{
    public Guid TaskId { get; set; }

    /// <summary>
    /// 本次执行使用的提示词；为空时回退到任务保存的 PromptContent
    /// </summary>
    public string? Prompt { get; set; }
}
