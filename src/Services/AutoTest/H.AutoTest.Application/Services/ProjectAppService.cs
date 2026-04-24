using H.AutoTest.Application.Contracts;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text.Json;

namespace H.AutoTest.Application;

/// <summary>
/// 项目服务
/// </summary>
public class ProjectAppService : AutoTestAppServiceBase<ProjectDto>, IProjectAppService
{
    private readonly ProjectEnvironmentAppService _environmentService;
    private readonly string _dataPath;
    
    public ProjectAppService(IConfiguration configuration, ProjectEnvironmentAppService environmentService) : base(Path.Combine(configuration["DataPath"] ?? "data", "projects.json"))
    {
        _environmentService = environmentService;
        _dataPath = configuration["DataPath"] ?? "data";
    }
    
    public async Task<List<ProjectDto>> GetAllAsync()
    {
        var filePath = Path.Combine(_dataPath, "projects.json");
        if (!File.Exists(filePath))
        {
            return new List<ProjectDto>();
        }
        
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<ProjectDto>>(json) ?? new List<ProjectDto>();
    }
    
    public async Task<ProjectDto?> GetByIdAsync(string id)
    {
        var projects = await GetAllAsync();
        return projects.FirstOrDefault(p => p.Id == id);
    }
    
    public async Task<string> CreateAsync(ProjectDto project)
    {
        project.Id = GenerateShortId();
        project.CreatedAt = DateTime.Now;
        project.UpdatedAt = DateTime.Now;
        
        var projects = await GetAllAsync();
        projects.Add(project);
        
        await SaveAsync(projects);
        return project.Id;
    }
    
    public async Task<bool> UpdateAsync(string id, ProjectDto project)
    {
        var projects = await GetAllAsync();
        var index = projects.FindIndex(p => p.Id == id);
        
        if (index >= 0)
        {
            project.Id = id;
            project.UpdatedAt = DateTime.Now;
            projects[index] = project;
            await SaveAsync(projects);
            return true;
        }
        
        return false;
    }
    
    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var projects = await GetAllAsync();
            var project = projects.FirstOrDefault(p => p.Id == id);
            
            if (project != null)
            {
                projects.Remove(project);
                await SaveAsync(projects);
                
                // 删除项目相关的数据文件夹
                var projectDataPath = Path.Combine(_dataPath, id);
                if (Directory.Exists(projectDataPath))
                {
                    Directory.Delete(projectDataPath, true);
                }
                
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"删除项目 {id} 时发生错误: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 生成8位包含小写字母和数字的短字符ID
    /// </summary>
    private string GenerateShortId()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var result = new char[8];
        for (int i = 0; i < 8; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }
        return new string(result);
    }
    
    private async Task SaveAsync(List<ProjectDto> projects)
    {
        var filePath = Path.Combine(_dataPath, "projects.json");
        var directoryPath = Path.GetDirectoryName(filePath);
        
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        
        var json = JsonSerializer.Serialize(projects, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        await File.WriteAllTextAsync(filePath, json);
    }
    
    /// <summary>
    /// 检查项目ID是否已存在
    /// </summary>
    public async Task<bool> IsIdExistsAsync(string id)
    {
        var projects = await GetAllAsync();
        return projects.Any(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 生成唯一的项目ID
    /// </summary>
    public async Task<string> GenerateUniqueIdAsync()
    {
        string id;
        do
        {
            id = GenerateShortId();
        } while (await IsIdExistsAsync(id));
        
        return id;
    }
    
    /// <summary>
    /// 创建项目（支持自定义ID或自动生成）
    /// </summary>
    public async Task<string> CreateProjectAsync(ProjectDto project)
    {
        // 如果没有提供ID或ID为空，则自动生成
        if (string.IsNullOrWhiteSpace(project.Id))
        {
            project.Id = await GenerateUniqueIdAsync();
        }
        else
        {
            // 检查ID是否已存在
            if (await IsIdExistsAsync(project.Id))
            {
                throw new InvalidOperationException($"项目标识 '{project.Id}' 已存在，请使用其他标识。");
            }
        }
        
        await CreateAsync(project);
        return project.Id;
    }
    
    /// <summary>
    /// 获取项目的环境列表
    /// </summary>
    public async Task<List<ProjectEnvironmentDto>> GetProjectEnvironmentsAsync(string projectId)
    {
        return await _environmentService.GetByProjectIdAsync(projectId);
    }
}

/// <summary>
/// 测试用例分类服务
/// </summary>
public class ProjectCaseCategoryService : IProjectCaseCategoryAppService
{
    private readonly string _dataPath;
    
    public ProjectCaseCategoryService(IConfiguration configuration) 
    {
        _dataPath = configuration["DataPath"] ?? "data";
    }
    
    /// <summary>
    /// 根据项目 ID 获取分类列表
    /// </summary>
    public async Task<List<ProjectCaseCategory>> GetByProjectIdAsync(string projectId)
    {
        var filePath = Path.Combine(_dataPath, projectId, "project-case-categories.json");
        if (!File.Exists(filePath))
        {
            return new List<ProjectCaseCategory>();
        }
        
        var json = await File.ReadAllTextAsync(filePath);
        var categories = System.Text.Json.JsonSerializer.Deserialize<List<ProjectCaseCategory>>(json) ?? new List<ProjectCaseCategory>();
        return categories.OrderBy(c => c.Order).ToList();
    }
    
    /// <summary>
    /// 创建新的测试用例分类
    /// </summary>
    public async Task<ProjectCaseCategory> CreateAsync(ProjectCaseCategory category)
    {
        category.Id = Guid.NewGuid().ToString();
        category.CreatedAt = DateTime.Now;
        
        var categories = await GetByProjectIdAsync(category.ProjectId);
        categories.Add(category);
        
        await SaveAsync(category.ProjectId, categories);
        return category;
    }
    
    /// <summary>
    /// 更新测试用例分类
    /// </summary>
    public async Task<bool> UpdateAsync(string id, ProjectCaseCategory category)
    {
        var categories = await GetByProjectIdAsync(category.ProjectId);
        var index = categories.FindIndex(c => c.Id == id);
        
        if (index >= 0)
        {
            category.Id = id;
            categories[index] = category;
            await SaveAsync(category.ProjectId, categories);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 删除测试用例分类
    /// </summary>
    public async Task<bool> DeleteAsync(string projectId, string id)
    {
        var categories = await GetByProjectIdAsync(projectId);
        var category = categories.FirstOrDefault(c => c.Id == id);
        
        if (category != null)
        {
            categories.Remove(category);
            await SaveAsync(projectId, categories);
            return true;
        }
        
        return false;
    }
    
    private async Task SaveAsync(string projectId, List<ProjectCaseCategory> categories)
    {
        var dirPath = Path.Combine(_dataPath, projectId);
        Directory.CreateDirectory(dirPath);
        
        var filePath = Path.Combine(dirPath, "project-case-categories.json");
        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var json = System.Text.Json.JsonSerializer.Serialize(categories, options);
        await File.WriteAllTextAsync(filePath, json);
    }
    
    /// <summary>
    /// 获取树形结构的分类列表
    /// </summary>
    public async Task<List<ProjectCaseCategory>> GetTreeStructureAsync(string projectId)
    {
        var allCategories = await GetByProjectIdAsync(projectId);
        
        // 构建树形结构
        var rootCategories = allCategories.Where(c => string.IsNullOrEmpty(c.ParentId)).ToList();
        
        foreach (var category in rootCategories)
        {
            BuildCategoryTree(category, allCategories);
        }
        
        return rootCategories;
    }
    
    private void BuildCategoryTree(ProjectCaseCategory parent, List<ProjectCaseCategory> allCategories)
    {
        // 这里可以根据需要扩展子分类逻辑
        // 由于当前模型中没有 Children 属性，暂时保持简单实现
    }
}