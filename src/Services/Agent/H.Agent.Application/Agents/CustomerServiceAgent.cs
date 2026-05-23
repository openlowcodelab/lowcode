using H.Agent.Application.Contracts;

namespace H.Agent.Application;

/// <summary>
/// 客服 Agent 实现
/// </summary>
public class CustomerServiceAgent : AgentBase
{
    public override string Name => "客服助手";

    public override string SystemPrompt => """
        你是一个专业的智能客服助手，能够帮助客户解答问题、处理工单和收集反馈。
        请根据知识库提供准确的回答，并在需要时建议创建工单。
        """;

    public CustomerServiceAgent(ILLMProvider? llmProvider) : base(llmProvider) { }

    public override List<string> GetAvailableTools()
    {
        return new List<string> { "知识库查询", "工单创建", "客户反馈", "常见问题解答" };
    }
}
