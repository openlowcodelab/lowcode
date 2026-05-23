using H.Agent.Application.Contracts;

namespace H.Agent.Application;

/// <summary>
/// 通用 Agent 实现
/// </summary>
public class GeneralAgent : AgentBase
{
    public override string Name => "通用助手";

    public override string SystemPrompt => """
        你是一个通用的智能助手，能够帮助用户解答问题、生成文本、编写代码、翻译语言等。
        请用简洁语言回答用户的问题，保持友好和专业的态度。
        """;

    public GeneralAgent(ILLMProvider? llmProvider) : base(llmProvider) { }

    public override List<string> GetAvailableTools()
    {
        return new List<string> { "代码执行", "文本翻译", "摘要生成", "知识查询" };
    }
}
