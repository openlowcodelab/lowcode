using H.AutoTest.Application.Contracts;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace H.AutoTest.Application;

/// <summary>
/// 测试执行记录服务
/// </summary>
public class ExecutionRecordAppService
{
    private readonly string _dataPath;
    
    public ExecutionRecordAppService(IConfiguration configuration)
    {
        _dataPath = configuration["DataPath"] ?? "data";
    }
    
    /// <summary>
    /// 获取指定项目的所有执行记录
    /// </summary>
    public async Task<List<ExecutionRecordDto>> GetByProjectIdAsync(string projectId)
    {
        var filePath = Path.Combine(_dataPath, projectId, "execution-records.json");
        
        if (!File.Exists(filePath))
        {
            return new List<ExecutionRecordDto>();
        }
        
        var json = await File.ReadAllTextAsync(filePath);
        
        // 修复：读取时启用属性名大小写不敏感，兼容保存时使用的 camelCase
        var readOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var records = JsonSerializer.Deserialize<List<ExecutionRecordDto>>(json, readOptions) ?? new List<ExecutionRecordDto>();
        
        // 过滤掉无效的空记录（可能由历史数据或错误写入产生）
        records = records.Where(r => !string.IsNullOrWhiteSpace(r.Id) && !string.IsNullOrWhiteSpace(r.ProjectId)).ToList();
        
        return records.OrderByDescending(r => r.StartTime).ToList();
    }
    
    /// <summary>
    /// 获取指定测试用例的执行记录
    /// </summary>
    public async Task<List<ExecutionRecordDto>> GetByTestCaseIdAsync(string projectId, string testCaseId)
    {
        var records = await GetByProjectIdAsync(projectId);
        return records.Where(r => r.TestCaseId == testCaseId)
                     .OrderByDescending(r => r.StartTime)
                     .ToList();
    }
    
    /// <summary>
    /// 根据ID获取执行记录
    /// </summary>
    public async Task<ExecutionRecordDto?> GetByIdAsync(string projectId, string id)
    {
        var records = await GetByProjectIdAsync(projectId);
        return records.FirstOrDefault(r => r.Id == id);
    }
    
    /// <summary>
    /// 创建新的执行记录
    /// </summary>
    public async Task<ExecutionRecordDto> CreateAsync(ExecutionRecordDto record)
    {
        record.Id = Guid.NewGuid().ToString();
        record.StartTime = DateTime.Now;
        
        var records = await GetByProjectIdAsync(record.ProjectId);
        records.Add(record);
        
        await SaveAsync(record.ProjectId, records);
        return record;
    }
    
    /// <summary>
    /// 更新执行记录
    /// </summary>
    public async Task<bool> UpdateAsync(string projectId, ExecutionRecordDto record)
    {
        var records = await GetByProjectIdAsync(projectId);
        var index = records.FindIndex(r => r.Id == record.Id);
        
        if (index >= 0)
        {
            records[index] = record;
            await SaveAsync(projectId, records);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 删除执行记录
    /// </summary>
    public async Task<bool> DeleteAsync(string projectId, string id)
    {
        var records = await GetByProjectIdAsync(projectId);
        var record = records.FirstOrDefault(r => r.Id == id);
        
        if (record != null)
        {
            records.Remove(record);
            await SaveAsync(projectId, records);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 清理旧的执行记录（保留最近N条）
    /// </summary>
    public async Task CleanupOldRecordsAsync(string projectId, int keepCount = 100)
    {
        var records = await GetByProjectIdAsync(projectId);
        
        if (records.Count > keepCount)
        {
            var recordsToKeep = records.OrderByDescending(r => r.StartTime)
                                      .Take(keepCount)
                                      .ToList();
            
            await SaveAsync(projectId, recordsToKeep);
        }
    }
    
    /// <summary>
    /// 获取执行统计信息
    /// </summary>
    public async Task<ExecutionStatistics> GetStatisticsAsync(string projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var records = await GetByProjectIdAsync(projectId);
        
        if (startDate.HasValue)
        {
            records = records.Where(r => r.StartTime >= startDate.Value).ToList();
        }
        
        if (endDate.HasValue)
        {
            records = records.Where(r => r.StartTime <= endDate.Value).ToList();
        }
        
        return new ExecutionStatistics
        {
            TotalExecutions = records.Count,
            SuccessExecutions = records.Count(r => r.Status == ExecutionStatus.Success),
            FailedExecutions = records.Count(r => r.Status == ExecutionStatus.Failed),
            CancelledExecutions = records.Count(r => r.Status == ExecutionStatus.Cancelled),
            AverageDuration = records.Any() ? records.Average(r => r.Duration) : 0,
            TotalDuration = records.Sum(r => r.Duration)
        };
    }
    
    /// <summary>
    /// 保存执行记录列表到文件
    /// </summary>
    private async Task SaveAsync(string projectId, List<ExecutionRecordDto> records)
    {
        var directoryPath = Path.Combine(_dataPath, projectId);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        
        var filePath = Path.Combine(directoryPath, "execution-records.json");
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(records, options);
        await File.WriteAllTextAsync(filePath, json);
    }
}