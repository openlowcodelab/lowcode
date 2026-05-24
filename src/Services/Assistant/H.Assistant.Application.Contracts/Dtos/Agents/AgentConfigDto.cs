namespace H.Assistant.Application.Contracts;

/// <summary>
/// Agent 配置 DTO
/// </summary>
public class AgentConfigDto
{
    public string AgentType { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<string> Capabilities { get; set; } = new();
}
