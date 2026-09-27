namespace H.Workbench.Application.Contracts;

/// <summary>
/// 提交一次运行（脱离 HTTP 请求的后台执行）。返回的 runId 即 TaskLog 主键，
/// 之后凭它订阅事件流、查轨迹、或取消。
/// </summary>
public class StartRunInputDto
{
    public Guid TaskId { get; set; }

    /// <summary>本次执行的提问；留空则用任务默认提示词</summary>
    public string? Prompt { get; set; }
}

/// <summary>
/// 运行状态：供"重连时先看还在不在跑"这类判断使用。
/// </summary>
public class RunStatusDto
{
    public Guid RunId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool Tracked { get; set; }
    public int StepCount { get; set; }
    public string? Verdict { get; set; }
}
