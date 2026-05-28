using H.Assistant.Application.Contracts;

namespace H.Assistant.Core;

/// <summary>
/// 数据分析 Agent 实现
/// </summary>
public class DataAnalysisAgent : AgentBase
{
    public override string Name => "数据分析助手";

    public override string SystemPrompt => """
        你是一个数据分析智能助手，能够帮助用户进行数据查询、统计分析和报表生成。
        请提供清晰的数据分析结果，并给出专业的解读建议。
        """;

    public DataAnalysisAgent(ILLMProvider? llmProvider) : base(llmProvider) { }

    public override List<string> GetAvailableTools()
    {
        return new List<string> { "SQL 查询", "统计分析", "图表生成", "趋势预测", "报表导出" };
    }
}
