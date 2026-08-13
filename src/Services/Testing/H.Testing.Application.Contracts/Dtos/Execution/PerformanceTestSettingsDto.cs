using System.ComponentModel.DataAnnotations;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 性能测试设置
/// </summary>
public class PerformanceTestSettingsDto
{
    /// <summary>
    /// 并发数（同时执行的线程数）
    /// </summary>
    [Range(1, 1000, ErrorMessage = "并发数必须在1-1000之间")]
    public int ConcurrentUsers { get; set; } = 1;

    /// <summary>
    /// 运行时间（秒）
    /// </summary>
    [Range(1, 86400, ErrorMessage = "运行时间必须在1-86400秒之间")]
    public int DurationSeconds { get; set; } = 60;

    /// <summary>
    /// 爬坡时间（秒）- 从1个用户逐渐增加到目标并发数的时间
    /// </summary>
    [Range(0, 3600, ErrorMessage = "爬坡时间必须在0-3600秒之间")]
    public int RampUpSeconds { get; set; } = 10;

    /// <summary>
    /// 是否启用性能测试模式
    /// </summary>
    public bool IsPerformanceTestEnabled { get; set; } = false;

    /// <summary>
    /// 思考时间（每次请求之间的等待时间，毫秒）
    /// </summary>
    [Range(0, 60000, ErrorMessage = "思考时间必须在0-60000毫秒之间")]
    public int ThinkTimeMs { get; set; } = 1000;

    /// <summary>
    /// 最大错误率（百分比，超过此错误率将停止测试）
    /// </summary>
    [Range(0, 100, ErrorMessage = "最大错误率必须在0-100之间")]
    public double MaxErrorRate { get; set; } = 10.0;
}

/// <summary>
/// 批量执行设置
/// </summary>
public class BatchExecutionSettings
{
    /// <summary>
    /// 选中的测试用例ID列表
    /// </summary>
    public List<long> SelectedTestCaseIds { get; set; } = new();

    /// <summary>
    /// 性能测试设置
    /// </summary>
    public PerformanceTestSettingsDto PerformanceSettings { get; set; } = new();

    /// <summary>
    /// 执行环境ID
    /// </summary>
    public long EnvironmentId { get; set; }

    /// <summary>
    /// 是否并行执行（如果为false，则串行执行）
    /// </summary>
    public bool IsParallelExecution { get; set; } = true;

    /// <summary>
    /// 失败时是否继续执行其他用例
    /// </summary>
    public bool ContinueOnFailure { get; set; } = true;
}