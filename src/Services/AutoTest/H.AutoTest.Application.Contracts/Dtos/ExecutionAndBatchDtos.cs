using System;
using System.Collections.Generic;

namespace H.AutoTest.Application.Contracts;

/// <summary>
/// 服务配置视图模型
/// </summary>
public class ServiceConfigView
{
    /// <summary>
    /// 项目服务
    /// </summary>
    public ProjectServiceDto ProjectService { get; set; } = new();
    
    /// <summary>
    /// 环境配置
    /// </summary>
    public EnvironmentServiceConfigDto? EnvironmentConfig { get; set; }
    
    /// <summary>
    /// 是否已配置
    /// </summary>
    public bool IsConfigured { get; set; }
    
    /// <summary>
    /// 状态文本
    /// </summary>
    public string StatusText { get; set; } = string.Empty;
    
    /// <summary>
    /// 状态样式类
    /// </summary>
    public string StatusClass { get; set; } = string.Empty;
    
    /// <summary>
    /// 环境服务配置ID
    /// </summary>
    public string? EnvironmentServiceConfigId { get; set; }
}

/// <summary>
/// 执行统计信息
/// </summary>
public class ExecutionStatistics
{
    public int TotalExecutions { get; set; }
    public int SuccessExecutions { get; set; }
    public int FailedExecutions { get; set; }
    public int CancelledExecutions { get; set; }
    public double AverageDuration { get; set; }
    public long TotalDuration { get; set; }
    
    public double SuccessRate => TotalExecutions > 0 ? (double)SuccessExecutions / TotalExecutions * 100 : 0;
}

/// <summary>
/// 批量执行结果
/// </summary>
public class BatchExecutionResult
{
    public string BatchId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public ExecutionStatus Status { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    
    public BatchExecutionSettings Settings { get; set; } = new();
    public string EnvironmentId { get; set; } = string.Empty;
    
    public int TotalTestCases { get; set; }
    public int SuccessfulTestCases { get; set; }
    public int FailedTestCases { get; set; }
    public int TotalExecutions { get; set; } // 性能测试时的总执行次数
    
    public List<ExecutionRecordDto> ExecutionRecords { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate => TotalExecutions > 0 ? (double)SuccessfulTestCases / TotalExecutions * 100 : 0;
    
    /// <summary>
    /// 错误率
    /// </summary>
    public double ErrorRate => TotalExecutions > 0 ? (double)FailedTestCases / TotalExecutions * 100 : 0;
    
    /// <summary>
    /// 平均执行时间
    /// </summary>
    public TimeSpan AverageExecutionTime => ExecutionRecords.Any() 
        ? TimeSpan.FromMilliseconds(ExecutionRecords.Average(r => r.Duration))
        : TimeSpan.Zero;
}
