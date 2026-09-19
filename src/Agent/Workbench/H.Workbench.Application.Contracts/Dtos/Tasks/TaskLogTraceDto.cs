namespace H.Workbench.Application.Contracts;

/// <summary>
/// 一次任务执行的完整轨迹（步骤时间线 + 产物），供刷新后回放
/// </summary>
public class TaskLogTraceDto
{
    public string Status { get; set; } = string.Empty;
    public List<ExecutionStepDto> Steps { get; set; } = new();
    public List<ArtifactDto> Artifacts { get; set; } = new();
}
