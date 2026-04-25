using H.AutoTest.Application;
using H.AutoTest.Application.Contracts;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text.Json;
using Volo.Abp.Application.Services;

namespace H.AutoTest.Application;

public class ProjectCaseAppService : AutoTestAppServiceBase<ProjectCaseDto>, IProjectCaseAppService
{
    private readonly string _dataPath;
    
    public ProjectCaseAppService(IConfiguration configuration) : base(Path.Combine(Path.GetTempPath(), "temp-project-cases.json"))
    {
        _dataPath = configuration["DataPath"] ?? "data";
    }
    
    protected override string GetId(ProjectCaseDto item) => item.Id;
    protected override void SetId(ProjectCaseDto item, string id) => item.Id = id;
    protected override string GenerateId(ProjectCaseDto item) => GenerateShortId();
    
    protected override void SetCreatedAt(ProjectCaseDto item, DateTime createdAt) => item.CreatedAt = createdAt;
    protected override void SetUpdatedAt(ProjectCaseDto item, DateTime updatedAt) => item.UpdatedAt = updatedAt;
    
    /// <summary>
    /// 获取所有测试用例（所有项目）
    /// </summary>
    public new async Task<List<ProjectCaseDto>> GetAllAsync()
    {
        var allProjectCases = new List<ProjectCaseDto>();
        
        if (!Directory.Exists(_dataPath))
            return allProjectCases;
            
        var projectDirs = Directory.GetDirectories(_dataPath)
            .Where(d => Directory.Exists(d) && !Path.GetFileName(d).StartsWith("."))
            .ToList();
            
        foreach (var projectDir in projectDirs)
        {
            var projectId = Path.GetFileName(projectDir);
            var projectCases = await GetByProjectIdAsync(projectId);
            allProjectCases.AddRange(projectCases);
        }
        
        return allProjectCases;
    }
    
    /// <summary>
    /// 根据项目 ID 获取测试用例
    /// </summary>
    public async Task<List<ProjectCaseDto>> GetByProjectIdAsync(string projectId)
    {
        var filePath = Path.Combine(_dataPath, projectId, "project-cases.json");
        if (!File.Exists(filePath))
        {
            return new List<ProjectCaseDto>();
        }
        
        var json = await File.ReadAllTextAsync(filePath);
        var projectCases = JsonSerializer.Deserialize<List<ProjectCaseDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ProjectCaseDto>();
        return projectCases;
    }
    
    /// <summary>
    /// 根据 ID 获取测试用例
    /// </summary>
    public new async Task<ProjectCaseDto?> GetByIdAsync(string id)
    {
        var allProjectCases = await GetAllAsync();
        return allProjectCases.FirstOrDefault(tc => tc.Id == id);
    }
    
    /// <summary>
    /// 创建新的测试用例
    /// </summary>
    public new async Task<string> CreateAsync(ProjectCaseDto projectCase)
    {
        projectCase.Id = GenerateShortId();
        projectCase.CreatedAt = DateTime.Now;
        projectCase.UpdatedAt = DateTime.Now;
        
        var projectCases = await GetByProjectIdAsync(projectCase.ProjectId);
        projectCases.Add(projectCase);
        
        await SaveAsync(projectCase.ProjectId, projectCases);
        return projectCase.Id;
    }
    
    /// <summary>
    /// 更新测试用例
    /// </summary>
    public new async Task<bool> UpdateAsync(string id, ProjectCaseDto projectCase)
    {
        var projectCases = await GetByProjectIdAsync(projectCase.ProjectId);
        var index = projectCases.FindIndex(tc => tc.Id == id);
        
        if (index >= 0)
        {
            projectCase.Id = id;
            projectCase.UpdatedAt = DateTime.Now;
            projectCases[index] = projectCase;
            await SaveAsync(projectCase.ProjectId, projectCases);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 删除测试用例
    /// </summary>
    public new async Task<bool> DeleteAsync(string id)
    {
        var allProjectCases = await GetAllAsync();
        var projectCase = allProjectCases.FirstOrDefault(tc => tc.Id == id);
        
        if (projectCase != null)
        {
            var projectCases = await GetByProjectIdAsync(projectCase.ProjectId);
            projectCases.RemoveAll(tc => tc.Id == id);
            await SaveAsync(projectCase.ProjectId, projectCases);
            return true;
        }
        
        return false;
    }
    
    public async Task<List<ProjectCaseDto>> GetActiveProjectCasesAsync()
    {
        var allProjectCases = await GetAllAsync();
        return allProjectCases.Where(tc => tc.Status == ProjectCaseStatus.Active).ToList();
    }
    
    public async Task<List<ProjectCaseDto>> GetByTagsAsync(string tag)
    {
        var allProjectCases = await GetAllAsync();
        return allProjectCases.Where(tc => tc.Tags.Contains(tag)).ToList();
    }
    
    public async Task<List<ProjectCaseDto>> GetByLevelAsync(string level)
    {
        var allProjectCases = await GetAllAsync();
        return allProjectCases.Where(tc => tc.Levels.Contains(level)).ToList();
    }
    
    /// <summary>
    /// 根据搜索条件查找测试用例
    /// </summary>
    public async Task<List<ProjectCaseDto>> SearchAsync(string keyword)
    {
        var allProjectCases = await GetAllAsync();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return allProjectCases;
        }
        
        return allProjectCases.Where(tc => 
            tc.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            tc.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            tc.Tags.Any(t => t.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        ).ToList();
    }
    
    /// <summary>
    /// 复制测试用例
    /// </summary>
    public async Task<string> CopyAsync(string id)
    {
        var originalCase = await GetByIdAsync(id);
        if (originalCase == null)
        {
            throw new ArgumentException($"Test case with ID {id} not found");
        }
        
        // 创建复制的测试用例
        var copiedCase = new ProjectCaseDto
        {
            Id = GenerateShortId(),
            CaseNumber = GenerateNewCaseNumber(originalCase.CaseNumber),
            Name = $"{originalCase.Name} - 副本",
            Description = originalCase.Description,
            ProjectId = originalCase.ProjectId,
            CategoryId = originalCase.CategoryId,
            Levels = new List<string>(originalCase.Levels),
            Tags = new List<string>(originalCase.Tags),
            Order = originalCase.Order,
            Status = originalCase.Status,
            TestData = new Dictionary<string, object>(originalCase.TestData),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            CreatedBy = originalCase.CreatedBy,
            UpdatedBy = originalCase.UpdatedBy
        };
        
        // 深度复制步骤
        copiedCase.Steps = CopySteps(originalCase.Steps);
        
        // 保存复制的测试用例
        var projectCases = await GetByProjectIdAsync(copiedCase.ProjectId);
        projectCases.Add(copiedCase);
        await SaveAsync(copiedCase.ProjectId, projectCases);
        
        return copiedCase.Id;
    }
    
    /// <summary>
    /// 批量复制测试用例
    /// </summary>
    public async Task<List<string>> CopyBatchAsync(List<string> ids)
    {
        var copiedIds = new List<string>();
        
        foreach (var id in ids)
        {
            try
            {
                var copiedId = await CopyAsync(id);
                copiedIds.Add(copiedId);
            }
            catch (Exception)
            {
                // 忽略单个复制失败，继续处理其他用例
                continue;
            }
        }
        
        return copiedIds;
    }
    
    /// <summary>
    /// 生成新的用例编号
    /// </summary>
    private string GenerateNewCaseNumber(string originalCaseNumber)
    {
        if (string.IsNullOrEmpty(originalCaseNumber))
        {
            return string.Empty;
        }
        
        // 如果原编号以数字结尾，尝试递增
        var match = System.Text.RegularExpressions.Regex.Match(originalCaseNumber, @"^(.*)(\d+)$");
        if (match.Success)
        {
            var prefix = match.Groups[1].Value;
            var number = int.Parse(match.Groups[2].Value);
            return $"{prefix}{number + 1}";
        }
        
        // 否则添加 "_copy" 后缀
        return $"{originalCaseNumber}_copy";
    }
    
    /// <summary>
    /// 深度复制测试步骤
    /// </summary>
    private List<ProjectCaseStep> CopySteps(List<ProjectCaseStep> originalSteps)
    {
        var copiedSteps = new List<ProjectCaseStep>();
        
        foreach (var originalStep in originalSteps)
        {
            var copiedStep = new ProjectCaseStep
            {
                Id = Guid.NewGuid().ToString(),
                Name = originalStep.Name,
                Type = originalStep.Type,
                Parameters = new Dictionary<string, object>(originalStep.Parameters),
                ExpectedResult = originalStep.ExpectedResult,
                Order = originalStep.Order,
                IsEnabled = originalStep.IsEnabled
            };
            
            // 复制API配置
            if (originalStep.ApiConfig != null)
            {
                copiedStep.ApiConfig = new ApiStepConfig
                {
                    Method = originalStep.ApiConfig.Method,
                    ServiceId = originalStep.ApiConfig.ServiceId,
                    Url = originalStep.ApiConfig.Url,
                    Headers = new Dictionary<string, string>(originalStep.ApiConfig.Headers),
                    Params = new Dictionary<string, string>(originalStep.ApiConfig.Params),
                    Body = originalStep.ApiConfig.Body,
                    BodyType = originalStep.ApiConfig.BodyType,
                    Cookies = new Dictionary<string, string>(originalStep.ApiConfig.Cookies),
                    TimeoutMs = originalStep.ApiConfig.TimeoutMs,
                    FollowRedirects = originalStep.ApiConfig.FollowRedirects,
                    VerifySSL = originalStep.ApiConfig.VerifySSL,
                    VariableExtractions = new Dictionary<string, string>(originalStep.ApiConfig.VariableExtractions),
                    Assertions = CopyApiAssertions(originalStep.ApiConfig.Assertions),
                    Auth = CopyAuthConfig(originalStep.ApiConfig.Auth)
                };
            }
            
            // 复制UI配置
            if (originalStep.UiConfig != null)
            {
                copiedStep.UiConfig = new UiStepConfig
                {
                    Action = originalStep.UiConfig.Action,
                    Selector = originalStep.UiConfig.Selector,
                    SelectorType = originalStep.UiConfig.SelectorType,
                    Value = originalStep.UiConfig.Value,
                    TimeoutMs = originalStep.UiConfig.TimeoutMs,
                    TakeScreenshot = originalStep.UiConfig.TakeScreenshot,
                    Options = new Dictionary<string, string>(originalStep.UiConfig.Options)
                };
            }
            
            // 复制脚本配置
            if (originalStep.ScriptConfig != null)
            {
                copiedStep.ScriptConfig = new ScriptStepConfig
                {
                    ScriptType = originalStep.ScriptConfig.ScriptType,
                    ScriptContent = originalStep.ScriptConfig.ScriptContent,
                    TimeoutMs = originalStep.ScriptConfig.TimeoutMs,
                    CaptureOutput = originalStep.ScriptConfig.CaptureOutput,
                    ExpectedReturnType = originalStep.ScriptConfig.ExpectedReturnType,
                    Parameters = new Dictionary<string, object>(originalStep.ScriptConfig.Parameters),
                    VariableExtractions = new Dictionary<string, string>(originalStep.ScriptConfig.VariableExtractions)
                };
            }
            
            copiedSteps.Add(copiedStep);
        }
        
        return copiedSteps;
    }
    
    /// <summary>
    /// 复制API断言配置
    /// </summary>
    private List<ApiAssertion> CopyApiAssertions(List<ApiAssertion> originalAssertions)
    {
        var copiedAssertions = new List<ApiAssertion>();
        
        foreach (var assertion in originalAssertions)
        {
            copiedAssertions.Add(new ApiAssertion
            {
                Type = assertion.Type,
                Target = assertion.Target,
                Operator = assertion.Operator,
                ExpectedValue = assertion.ExpectedValue,
                Description = assertion.Description
            });
        }
        
        return copiedAssertions;
    }
    
    /// <summary>
    /// 复制认证配置
    /// </summary>
    private AuthConfig CopyAuthConfig(AuthConfig originalAuth)
    {
        return new AuthConfig
        {
            Type = originalAuth.Type,
            Username = originalAuth.Username,
            Password = originalAuth.Password,
            Token = originalAuth.Token,
            ApiKeyName = originalAuth.ApiKeyName,
            ApiKeyValue = originalAuth.ApiKeyValue,
            ApiKeyLocation = originalAuth.ApiKeyLocation
        };
    }
    
    private async Task SaveAsync(string projectId, List<ProjectCaseDto> projectCases)
    {
        var dirPath = Path.Combine(_dataPath, projectId);
        Directory.CreateDirectory(dirPath);
        
        var filePath = Path.Combine(dirPath, "project-cases.json");
        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var json = JsonSerializer.Serialize(projectCases, options);
        await File.WriteAllTextAsync(filePath, json);
    }
}