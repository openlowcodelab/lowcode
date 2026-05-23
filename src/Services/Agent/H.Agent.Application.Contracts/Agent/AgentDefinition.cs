namespace H.Agent.Application.Contracts;

/// <summary>
/// Agent 定义
/// </summary>
public class AgentDefinition
{
    public string AgentType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Capabilities { get; set; } = new();
}
