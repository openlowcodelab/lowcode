using H.Workbench.Application.Contracts;

namespace H.Workbench.Web;

/// <summary>
/// 工作台共享状态：当前员工列表与选中员工（由 WorkbenchLayout 级联下发）
/// </summary>
public class WorkbenchState
{
    public List<AgentDto> Agents { get; private set; } = new();

    public AgentDto? SelectedAgent { get; private set; }

    public bool Loaded { get; private set; }

    public event Action? Changed;

    public void SetAgents(IEnumerable<AgentDto> agents)
    {
        Agents = agents.ToList();
        Loaded = true;

        var selectedId = SelectedAgent?.Id;
        SelectedAgent = Agents.FirstOrDefault(x => x.Id == selectedId);
        Changed?.Invoke();
    }

    public void Select(AgentDto? agent)
    {
        SelectedAgent = agent;
        Changed?.Invoke();
    }

    public void NotifyChanged() => Changed?.Invoke();
}
