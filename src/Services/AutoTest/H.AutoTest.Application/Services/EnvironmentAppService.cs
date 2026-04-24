using H.AutoTest.Application.Contracts;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Volo.Abp.Application.Services;

namespace H.AutoTest.Application;

/// <summary>
/// 测试环境服务
/// </summary>
public class EnvironmentAppService : ApplicationService, IEnvironmentAppService
{
    private readonly string _dataPath;
    private readonly ProjectServiceConfigAppService _projectServiceConfigService;
    
    public EnvironmentAppService(IConfiguration configuration, ProjectServiceConfigAppService projectServiceConfigService)
    {
        _dataPath = configuration["DataPath"] ?? "data";
        _projectServiceConfigService = projectServiceConfigService;
    }

    /// <summary>
    /// 获取指定项目的所有环境
    /// </summary>
    public async Task<List<EnvironmentDto>> GetByProjectIdAsync(string projectId)
    {
        var environments = new List<EnvironmentDto>();

        // 首先尝试读取 environments.json 文件
        var filePath = Path.Combine(_dataPath, projectId, "project-environments.json");

        if (File.Exists(filePath))
        {
            var json = await File.ReadAllTextAsync(filePath);
            var projectEnvironments = JsonSerializer.Deserialize<List<ProjectEnvironmentDto>>(json) ?? new List<ProjectEnvironmentDto>();

            // 将 ProjectEnvironment 转换为 TestEnvironment
            foreach (var projectEnv in projectEnvironments)
            {
                var testEnv = new EnvironmentDto
                {
                    Id = projectEnv.Id,
                    Name = projectEnv.Name,
                    Description = projectEnv.Description,
                    ProjectId = projectEnv.ProjectId,
                    Status = (EnvironmentStatus)((int)projectEnv.Status),
                    CreatedAt = projectEnv.CreatedAt,
                    UpdatedAt = projectEnv.UpdatedAt,
                    CreatedBy = projectEnv.CreatedBy,
                    UpdatedBy = projectEnv.UpdatedBy,
                    Order = 0, // 默认排序值
                    Config = new Dictionary<string, object>(),
                    ServiceEndpoints = new Dictionary<string, string>()
                };

                // 转换环境变量
                foreach (var variable in projectEnv.Variables)
                {
                    testEnv.Config[variable.Key] = variable.Value;
                }

                // 转换服务端点配置（来自 ProjectEnvironment 内嵌配置）
                foreach (var serviceConfig in projectEnv.EnvironmentServiceConfigs)
                {
                    if (!string.IsNullOrEmpty(serviceConfig.BaseUrl))
                    {
                        testEnv.ServiceEndpoints[serviceConfig.ProjectServiceId] = serviceConfig.BaseUrl;
                    }
                }

                // 额外合并磁盘上的 environment-service-configs.json 配置，确保端点完整
                var envServiceConfigs = await _projectServiceConfigService.GetEnvironmentServiceConfigsAsync(projectEnv.Id);
                foreach (var cfg in envServiceConfigs)
                {
                    if (!string.IsNullOrWhiteSpace(cfg.BaseUrl))
                    {
                        testEnv.ServiceEndpoints[cfg.ProjectServiceId] = cfg.BaseUrl;
                    }
                }

                environments.Add(testEnv);
            }
        }

        return environments.OrderBy(e => e.Order).ThenBy(e => e.CreatedAt).ToList();
    }

    /// <summary>
    /// 根据ID获取环境
    /// </summary>
    public async Task<EnvironmentDto?> GetByIdAsync(string projectId, string id)
    {
        var environments = await GetByProjectIdAsync(projectId);
        return environments.FirstOrDefault(e => e.Id == id);
    }
    
    /// <summary>
    /// 创建新环境
    /// </summary>
    public async Task<EnvironmentDto> CreateAsync(EnvironmentDto environment)
    {
        environment.Id = Guid.NewGuid().ToString();
        environment.CreatedAt = DateTime.Now;
        environment.UpdatedAt = DateTime.Now;
        
        var environments = await GetByProjectIdAsync(environment.ProjectId);
        environments.Add(environment);
        
        await SaveAsync(environment.ProjectId, environments);
        return environment;
    }
    
    /// <summary>
    /// 更新环境
    /// </summary>
    public async Task<bool> UpdateAsync(string id, EnvironmentDto environment)
    {
        var environments = await GetByProjectIdAsync(environment.ProjectId);
        var index = environments.FindIndex(e => e.Id == id);
        
        if (index >= 0)
        {
            environment.Id = id;
            environment.UpdatedAt = DateTime.Now;
            environments[index] = environment;
            await SaveAsync(environment.ProjectId, environments);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 删除环境
    /// </summary>
    public async Task<bool> DeleteAsync(string projectId, string id)
    {
        var environments = await GetByProjectIdAsync(projectId);
        var environment = environments.FirstOrDefault(e => e.Id == id);
        
        if (environment != null)
        {
            environments.Remove(environment);
            await SaveAsync(projectId, environments);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 保存环境列表到文件
    /// </summary>
    private async Task SaveAsync(string projectId, List<EnvironmentDto> environments)
    {
        var directoryPath = Path.Combine(_dataPath, projectId);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        
        var filePath = Path.Combine(directoryPath, "environments.json");
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(environments, options);
        await File.WriteAllTextAsync(filePath, json);
    }
}