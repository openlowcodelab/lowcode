using H.AutoTest.Application.Contracts;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using ProjectServiceModel = H.AutoTest.Application.Contracts.ProjectServiceDto;

namespace H.AutoTest.Application;

/// <summary>
/// 项目服务配置管理服务
/// </summary>
public class ProjectServiceConfigAppService : AutoTestAppServiceBase, IProjectServiceConfigAppService
{
    public ProjectServiceConfigAppService(IConfiguration configuration) 
        : base(configuration)
    {
    }
    
    /// <summary>
    /// 获取项目的所有服务
    /// </summary>
    public async Task<List<ProjectServiceModel>> GetProjectServicesAsync(string projectId)
    {
        var filePath = GetProjectServicesFilePath(projectId);
        
        if (!File.Exists(filePath))
            return new List<ProjectServiceModel>();
            
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var services = JsonSerializer.Deserialize<List<ProjectServiceModel>>(json, GetJsonOptions()) ?? new List<ProjectServiceModel>();
            return services.Where(s => s.ProjectId == projectId).ToList();
        }
        catch
        {
            return new List<ProjectServiceModel>();
        }
    }
    
    /// <summary>
    /// 创建项目服务
    /// </summary>
    public async Task<ProjectServiceModel> CreateProjectServiceAsync(ProjectServiceModel service)
    {
        service.Id = Guid.NewGuid().ToString();
        service.CreatedAt = DateTime.Now;
        service.UpdatedAt = DateTime.Now;
        
        var services = await GetProjectServicesAsync(service.ProjectId);
        services.Add(service);
        
        await SaveProjectServicesAsync(service.ProjectId, services);
        
        // 为所有环境创建默认服务配置
        await CreateEnvironmentServiceConfigsForAllEnvironmentsAsync(service.ProjectId, service.Id);
        
        return service;
    }
    
    /// <summary>
    /// 更新项目服务
    /// </summary>
    public async Task<ProjectServiceModel> UpdateProjectServiceAsync(string serviceId, ProjectServiceModel service)
    {
        var services = await GetProjectServicesAsync(service.ProjectId);
        var existingService = services.FirstOrDefault(s => s.Id == serviceId);
        
        if (existingService != null)
        {
            existingService.Name = service.Name;
            existingService.Description = service.Description;
            existingService.UpdatedAt = DateTime.Now;
            existingService.UpdatedBy = service.UpdatedBy;
            
            await SaveProjectServicesAsync(service.ProjectId, services);
            return existingService;
        }
        
        throw new ArgumentException($"Service with ID {serviceId} not found");
    }
    
    /// <summary>
    /// 删除项目服务
    /// </summary>
    public async Task DeleteProjectServiceAsync(string projectId, string serviceId)
    {
        var services = await GetProjectServicesAsync(projectId);
        services.RemoveAll(s => s.Id == serviceId);
        
        await SaveProjectServicesAsync(projectId, services);
        
        // 删除相关的环境服务配置
        await DeleteEnvironmentServiceConfigsAsync(serviceId);
    }
    

    
    #region Private Methods
    

    
    private Dictionary<string, string> MergeHeaders(Dictionary<string, string> defaultHeaders, Dictionary<string, string> environmentHeaders)
    {
        var merged = new Dictionary<string, string>(defaultHeaders);
        
        foreach (var header in environmentHeaders)
        {
            merged[header.Key] = header.Value; // 环境特定的头部会覆盖默认头部
        }
        
        return merged;
    }
    
    private async Task SaveProjectServicesAsync(string projectId, List<ProjectServiceModel> services)
    {
        var filePath = GetProjectServicesFilePath(projectId);
        var directoryPath = Path.GetDirectoryName(filePath);
        
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        
        var json = JsonSerializer.Serialize(services, GetJsonOptions());
        await File.WriteAllTextAsync(filePath, json);
    }
    
    #region 环境服务配置管理
    
    /// <summary>
    /// 获取环境的所有服务配置
    /// </summary>
    public async Task<List<EnvironmentServiceConfigDto>> GetEnvironmentServiceConfigsAsync(string environmentId)
    {
        var filePath = GetEnvironmentServiceConfigsFilePath(environmentId);
        
        if (!File.Exists(filePath))
            return new List<EnvironmentServiceConfigDto>();
            
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var configs = JsonSerializer.Deserialize<List<EnvironmentServiceConfigDto>>(json, GetJsonOptions()) ?? new List<EnvironmentServiceConfigDto>();
            return configs.Where(c => c.EnvironmentId == environmentId).ToList();
        }
        catch
        {
            return new List<EnvironmentServiceConfigDto>();
        }
    }
    
    /// <summary>
    /// 获取单个环境服务配置
    /// </summary>
    public async Task<EnvironmentServiceConfigDto?> GetEnvironmentServiceConfigAsync(string configId)
    {
        // 这里需要遍历所有环境配置文件来查找
        // 简化实现，实际项目中可能需要更高效的查找方式
        var dataDir = new DirectoryInfo(GetTenantDataPath());
        if (!dataDir.Exists) return null;
        
        foreach (var projectDir in dataDir.GetDirectories())
        {
            var configFile = Path.Combine(projectDir.FullName, "environment-service-configs.json");
            if (File.Exists(configFile))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(configFile);
                    var configs = JsonSerializer.Deserialize<List<EnvironmentServiceConfigDto>>(json, GetJsonOptions()) ?? new List<EnvironmentServiceConfigDto>();
                    var config = configs.FirstOrDefault(c => c.Id == configId);
                    if (config != null) return config;
                }
                catch { }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 更新环境服务配置
    /// </summary>
    public async Task<EnvironmentServiceConfigDto> UpdateEnvironmentServiceConfigAsync(EnvironmentServiceConfigDto config)
    {
        config.UpdatedAt = DateTime.Now;
        
        var configs = await GetEnvironmentServiceConfigsAsync(config.EnvironmentId);
        var existingIndex = configs.FindIndex(c => c.Id == config.Id);
        
        if (existingIndex >= 0)
        {
            configs[existingIndex] = config;
        }
        else
        {
            configs.Add(config);
        }
        
        await SaveEnvironmentServiceConfigsAsync(config.EnvironmentId, configs);
        return config;
    }
    
    /// <summary>
    /// 创建环境服务配置
    /// </summary>
    public async Task<EnvironmentServiceConfigDto> CreateEnvironmentServiceConfigAsync(EnvironmentServiceConfigDto config)
    {
        config.Id = Guid.NewGuid().ToString();
        config.CreatedAt = DateTime.Now;
        config.UpdatedAt = DateTime.Now;
        
        var configs = await GetEnvironmentServiceConfigsAsync(config.EnvironmentId);
        configs.Add(config);
        
        await SaveEnvironmentServiceConfigsAsync(config.EnvironmentId, configs);
        return config;
    }
    
    /// <summary>
    /// 删除环境服务配置
    /// </summary>
    public async Task DeleteEnvironmentServiceConfigAsync(string configId)
    {
        var config = await GetEnvironmentServiceConfigAsync(configId);
        if (config == null) return;
        
        var configs = await GetEnvironmentServiceConfigsAsync(config.EnvironmentId);
        configs.RemoveAll(c => c.Id == configId);
        
        await SaveEnvironmentServiceConfigsAsync(config.EnvironmentId, configs);
    }
    
    /// <summary>
    /// 获取服务配置视图（包含服务信息）
    /// </summary>
    public async Task<List<ServiceConfigView>> GetServiceConfigViewsAsync(string environmentId, string projectId)
    {
        var services = await GetProjectServicesAsync(projectId);
        var configs = await GetEnvironmentServiceConfigsAsync(environmentId);
        
        var views = new List<ServiceConfigView>();
        
        foreach (var service in services)
        {
            var config = configs.FirstOrDefault(c => c.ProjectServiceId == service.Id);
            var view = new ServiceConfigView
            {
                ProjectService = service,
                EnvironmentConfig = config,
                IsConfigured = config != null,
                StatusText = config != null ? "已配置" : "未配置",
                StatusClass = config != null ? "text-success" : "text-muted",
                EnvironmentServiceConfigId = config?.Id
            };
            views.Add(view);
        }
        
        return views;
    }
    
    /// <summary>
    /// 为所有环境创建服务配置
    /// </summary>
    private async Task CreateEnvironmentServiceConfigsForAllEnvironmentsAsync(string projectId, string serviceId)
    {
        // 这里需要获取项目的所有环境，然后为每个环境创建默认配置
        // 简化实现，实际项目中需要从环境服务获取环境列表
    }
    
    /// <summary>
    /// 删除服务的所有环境配置
    /// </summary>
    private async Task DeleteEnvironmentServiceConfigsAsync(string serviceId)
    {
        var dataDir = new DirectoryInfo(GetTenantDataPath());
        if (!dataDir.Exists) return;
        
        foreach (var projectDir in dataDir.GetDirectories())
        {
            var configFile = Path.Combine(projectDir.FullName, "environment-service-configs.json");
            if (File.Exists(configFile))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(configFile);
                    var configs = JsonSerializer.Deserialize<List<EnvironmentServiceConfigDto>>(json, GetJsonOptions()) ?? new List<EnvironmentServiceConfigDto>();
                    
                    var originalCount = configs.Count;
                    configs.RemoveAll(c => c.ProjectServiceId == serviceId);
                    
                    if (configs.Count != originalCount)
                    {
                        var updatedJson = JsonSerializer.Serialize(configs, GetJsonOptions());
                        await File.WriteAllTextAsync(configFile, updatedJson);
                    }
                }
                catch { }
            }
        }
    }
    
    /// <summary>
    /// 保存环境服务配置
    /// </summary>
    private async Task SaveEnvironmentServiceConfigsAsync(string environmentId, List<EnvironmentServiceConfigDto> configs)
    {
        var filePath = GetEnvironmentServiceConfigsFilePath(environmentId);
        var directoryPath = Path.GetDirectoryName(filePath);
        
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        
        var json = JsonSerializer.Serialize(configs, GetJsonOptions());
        await File.WriteAllTextAsync(filePath, json);
    }
    
    /// <summary>
    /// 获取环境服务配置文件路径
    /// </summary>
    private string GetEnvironmentServiceConfigsFilePath(string environmentId)
    {
        // 通过遍历项目目录查找包含该environmentId的项目
        var dataDir = new DirectoryInfo(GetTenantDataPath());
        if (!dataDir.Exists) 
        {
            // 如果数据目录不存在，使用默认路径
            return Path.Combine(GetTenantDataPath(), "default", "environment-service-configs.json");
        }
        
        foreach (var projectDir in dataDir.GetDirectories())
        {
            var envFile = Path.Combine(projectDir.FullName, "project-environments.json");
            if (File.Exists(envFile))
            {
                try
                 {
                     var json = File.ReadAllText(envFile);
                     var environments = JsonSerializer.Deserialize<List<JsonElement>>(json, GetJsonOptions());
                     if (environments?.Any(env => env.GetProperty("Id").GetString() == environmentId) == true)
                     {
                         return Path.Combine(projectDir.FullName, "environment-service-configs.json");
                     }
                 }
                catch { }
            }
        }
        
        // 如果找不到，使用默认路径
        return Path.Combine(GetTenantDataPath(), "default", "environment-service-configs.json");
    }
    
    #endregion
    
    private string GetProjectServicesFilePath(string projectId)
    {
        return Path.Combine(GetTenantDataPath(), projectId, "project-services.json");
    }
    
    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }
    
    #endregion
}