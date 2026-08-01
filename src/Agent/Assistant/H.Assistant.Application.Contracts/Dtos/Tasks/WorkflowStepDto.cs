namespace H.Assistant.Application.Contracts;

/// <summary>
/// 工作流步骤定义（任务创建方式为"工作流"时使用，序列化存储于 WorkflowContent）
/// </summary>
public class WorkflowStepDto
{
    /// <summary>步骤名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>步骤提示词/指令</summary>
    public string Prompt { get; set; } = string.Empty;
}
