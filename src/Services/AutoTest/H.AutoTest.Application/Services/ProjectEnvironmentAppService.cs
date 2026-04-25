using H.AutoTest.Application.Contracts;
using H.Util.Ids;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text.Json;
using Volo.Abp.Application.Services;

namespace H.AutoTest.Application;

public class ProjectEnvironmentAppService : ApplicationService, IProjectEnvironmentAppService
{
    private readonly string _dataPath;
    
    public ProjectEnvironmentAppService(IConfiguration configuration)
    {
        _dataPath = configuration["DataPath"] ?? "data";
    }
    
    /// <summary>
    /// 获取所有项目环境（所有项目）
    /// </summary>
    public new async Task<List<ProjectEnvironmentDto>> GetAllAsync()
    {
        var allEnvironments = new List<ProjectEnvironmentDto>();
        
        if (!Directory.Exists(_dataPath))
            return allEnvironments;
            
        var projectDirs = Directory.GetDirectories(_dataPath)
            .Where(d => Directory.Exists(d) && !Path.GetFileName(d).StartsWith("."))
            .ToList();
            
        foreach (var projectDir in projectDirs)
        {
            var projectId = Path.GetFileName(projectDir);
            var environments = await GetByProjectIdAsync(projectId);
            allEnvironments.AddRange(environments);
        }
        
        return allEnvironments;
    }
    
    /// <summary>
    /// 根据项目ID获取环境列表
    /// </summary>
    public async Task<List<ProjectEnvironmentDto>> GetByProjectIdAsync(string projectId)
    {
        var filePath = Path.Combine(_dataPath, projectId, "project-environments.json");
        if (!File.Exists(filePath))
        {
            return new List<ProjectEnvironmentDto>();
        }
        
        var json = await File.ReadAllTextAsync(filePath);
        var environments = JsonSerializer.Deserialize<List<ProjectEnvironmentDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ProjectEnvironmentDto>();
        return environments;
    }
    
    /// <summary>
    /// 根据ID获取项目环境
    /// </summary>
    public new async Task<ProjectEnvironmentDto?> GetByIdAsync(string id)
    {
        var allEnvironments = await GetAllAsync();
        return allEnvironments.FirstOrDefault(env => env.Id == id);
    }
    
    /// <summary>
    /// 创建新的项目环境
    /// </summary>
    public new async Task<string> CreateAsync(ProjectEnvironmentDto environment)
    {
        environment.Id = ShortIdGenerator.Generate();
        environment.CreatedAt = DateTime.Now;
        environment.UpdatedAt = DateTime.Now;
        
        var environments = await GetByProjectIdAsync(environment.ProjectId);
        environments.Add(environment);
        
        await SaveAsync(environment.ProjectId, environments);
        return environment.Id;
    }
    
    /// <summary>
    /// 更新项目环境
    /// </summary>
    public new async Task<bool> UpdateAsync(string id, ProjectEnvironmentDto environment)
    {
        var environments = await GetByProjectIdAsync(environment.ProjectId);
        var index = environments.FindIndex(env => env.Id == id);
        
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
    /// 删除项目环境
    /// </summary>
    public new async Task<bool> DeleteAsync(string id)
    {
        var allEnvironments = await GetAllAsync();
        var environment = allEnvironments.FirstOrDefault(env => env.Id == id);
        
        if (environment != null)
        {
            var projectEnvironments = await GetByProjectIdAsync(environment.ProjectId);
            projectEnvironments.RemoveAll(env => env.Id == id);
            await SaveAsync(environment.ProjectId, projectEnvironments);
            return true;
        }
        
        return false;
    }
    
    public async Task<List<ProjectEnvironmentDto>> GetByTypeAsync(EnvironmentType type)
    {
        var allEnvironments = await GetAllAsync();
        return allEnvironments.Where(env => env.Type == type).ToList();
    }
    
    public async Task<List<ProjectEnvironmentDto>> GetActiveEnvironmentsAsync()
    {
        var allEnvironments = await GetAllAsync();
        return allEnvironments.Where(env => env.Status == EnvironmentStatus.Active).ToList();
    }
    
    /// <summary>
    /// 获取项目下的活跃环境
    /// </summary>
    public async Task<List<ProjectEnvironmentDto>> GetActiveEnvironmentsByProjectAsync(string projectId)
    {
        var environments = await GetByProjectIdAsync(projectId);
        return environments.Where(env => env.Status == EnvironmentStatus.Active).ToList();
    }
    
    /// <summary>
    /// 根据搜索条件查找项目环境
    /// </summary>
    public new async Task<List<ProjectEnvironmentDto>> SearchAsync(Func<ProjectEnvironmentDto, bool> predicate)
    {
        var allEnvironments = await GetAllAsync();
        return allEnvironments.Where(predicate).ToList();
    }
    
    private async Task SaveAsync(string projectId, List<ProjectEnvironmentDto> environments)
    {
        var dirPath = Path.Combine(_dataPath, projectId);
        Directory.CreateDirectory(dirPath);
        
        var filePath = Path.Combine(dirPath, "project-environments.json");
        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var json = JsonSerializer.Serialize(environments, options);
        await File.WriteAllTextAsync(filePath, json);
    }
}