using H.Assistant.Application.Contracts;

namespace H.Assistant.Extensions;

/// <summary>
/// 自动化测试 Agent 实现
/// </summary>
public class AutomationTestAgent : AgentBase
{
    public override string Name => "自动化测试助手";

    public override string SystemPrompt => """
        你是一个自动化测试智能助手，能够帮助用户生成测试用例、执行测试并分析结果。
        请提供专业的测试建议和详细的测试报告。
        """;

    public AutomationTestAgent(ILLMProvider? llmProvider) : base(llmProvider) { }

    public override List<string> GetAvailableTools()
    {
        return new List<string> { "测试用例生成", "Playwright 测试", "API 测试", "结果分析", "缺陷报告" };
    }
}
