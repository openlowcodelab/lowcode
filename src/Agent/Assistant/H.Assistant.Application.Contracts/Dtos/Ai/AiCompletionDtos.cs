using System.ComponentModel.DataAnnotations;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// AI 文本生成输入
/// </summary>
public class AiCompletionInputDto
{
    /// <summary>
    /// 系统提示词（可选）
    /// </summary>
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// 用户消息
    /// </summary>
    [Required(ErrorMessage = "AI 输入内容不能为空")]
    public string UserMessage { get; set; } = string.Empty;

    /// <summary>
    /// 生成温度（0~2，越低越稳定）
    /// </summary>
    public float Temperature { get; set; } = 0.3f;

    /// <summary>
    /// 最大输出 Token 数
    /// </summary>
    public int MaxTokens { get; set; } = 4096;
}

/// <summary>
/// AI 文本生成结果
/// </summary>
public class AiCompletionResultDto
{
    /// <summary>
    /// 生成内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 实际使用的模型
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// 消耗 Token 数
    /// </summary>
    public int UsageTokens { get; set; }
}
