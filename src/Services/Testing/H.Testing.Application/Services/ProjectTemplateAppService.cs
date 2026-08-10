using System.Text.Json.Nodes;
using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 测试项目模板服务
/// 模板以 JSON 文件存储：{模板根目录}/projects.json 为索引，每个模板一个同名子目录，
/// 子目录内含 project-services / project-environments / environment-service-configs /
/// project-case-categories / project-cases 等文件（字符串 ID，导入时重映射为数据库 long ID）。
/// </summary>
public class ProjectTemplateAppService : ApplicationService, IProjectTemplateAppService
{
    private readonly IRepository<TestingProject, long> _repository;
    private readonly IRepository<TestingProjectService, long> _serviceRepository;
    private readonly IRepository<TestingProjectEnvironment, long> _environmentRepository;
    private readonly IRepository<TestingEnvironmentServiceConfig, long> _serviceConfigRepository;
    private readonly IRepository<TestingProjectCaseCategory, long> _categoryRepository;
    private readonly IRepository<TestingProjectCase, long> _caseRepository;
    private readonly IConfiguration _configuration;

    public ProjectTemplateAppService(
        IRepository<TestingProject, long> repository,
        IRepository<TestingProjectService, long> serviceRepository,
        IRepository<TestingProjectEnvironment, long> environmentRepository,
        IRepository<TestingEnvironmentServiceConfig, long> serviceConfigRepository,
        IRepository<TestingProjectCaseCategory, long> categoryRepository,
        IRepository<TestingProjectCase, long> caseRepository,
        IConfiguration configuration)
    {
        _repository = repository;
        _serviceRepository = serviceRepository;
        _environmentRepository = environmentRepository;
        _serviceConfigRepository = serviceConfigRepository;
        _categoryRepository = categoryRepository;
        _caseRepository = caseRepository;
        _configuration = configuration;
    }

    #region 模板查询与管理

    public Task<List<ProjectTemplateDto>> GetTemplatesAsync()
    {
        var root = ResolveTemplatesRoot();
        var result = new List<ProjectTemplateDto>();
        foreach (var o in TemplateJson.ReadArray(Path.Combine(root, "projects.json")))
        {
            var id = TemplateJson.Str(o, "id");
            if (string.IsNullOrEmpty(id) || !Directory.Exists(Path.Combine(root, id))) continue;

            var dir = Path.Combine(root, id);
            result.Add(new ProjectTemplateDto
            {
                Id = id,
                Name = TemplateJson.Str(o, "name"),
                Description = TemplateJson.Str(o, "description"),
                ServiceCount = TemplateJson.ReadArray(Path.Combine(dir, "project-services.json")).Count,
                EnvironmentCount = TemplateJson.ReadArray(Path.Combine(dir, "project-environments.json")).Count,
                CategoryCount = TemplateJson.ReadArray(Path.Combine(dir, "project-case-categories.json")).Count,
                CaseCount = TemplateJson.ReadArray(Path.Combine(dir, "project-cases.json")).Count,
                UpdatedAt = TemplateJson.DateVal(o, "updatedAt") ?? DateTime.MinValue
            });
        }
        return Task.FromResult(result);
    }

    public Task<bool> UpdateTemplateAsync(string templateId, string name, string? description)
    {
        ValidateTemplateId(templateId);
        if (string.IsNullOrWhiteSpace(name)) throw new UserFriendlyException("模板名称不能为空");

        var root = ResolveTemplatesRoot();
        var index = TemplateJson.ReadArray(Path.Combine(root, "projects.json"));
        var entry = index.FirstOrDefault(o => string.Equals(TemplateJson.Str(o, "id"), templateId, StringComparison.OrdinalIgnoreCase));
        if (entry == null) return Task.FromResult(false);

        foreach (var key in KeysOf(entry, "name")) entry[key] = name.Trim();
        foreach (var key in KeysOf(entry, "description")) entry[key] = description ?? string.Empty;
        foreach (var key in KeysOf(entry, "updatedAt")) entry[key] = DateTime.Now.ToString("O");

        TemplateJson.Write(Path.Combine(root, "projects.json"), new JsonArray(index.Select(o => (JsonNode)o.DeepClone()).ToArray()));
        return Task.FromResult(true);
    }

    public Task<bool> DeleteTemplateAsync(string templateId)
    {
        ValidateTemplateId(templateId);

        var root = ResolveTemplatesRoot();
        var index = TemplateJson.ReadArray(Path.Combine(root, "projects.json"));
        var remaining = index.Where(o => !string.Equals(TemplateJson.Str(o, "id"), templateId, StringComparison.OrdinalIgnoreCase)).ToList();
        if (remaining.Count == index.Count && !Directory.Exists(Path.Combine(root, templateId)))
        {
            return Task.FromResult(false);
        }

        TemplateJson.Write(Path.Combine(root, "projects.json"), new JsonArray(remaining.Select(o => (JsonNode)o.DeepClone()).ToArray()));

        var dir = Path.Combine(root, templateId);
        if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        return Task.FromResult(true);
    }

    #endregion

    #region 从模板创建项目

    public async Task<long> CreateProjectFromTemplateAsync(string templateId, string name, string? description)
    {
        ValidateTemplateId(templateId);
        if (string.IsNullOrWhiteSpace(name)) throw new UserFriendlyException("项目名称不能为空");

        var root = ResolveTemplatesRoot();
        var dir = Path.Combine(root, templateId);
        if (!Directory.Exists(dir)) throw new UserFriendlyException($"模板 '{templateId}' 不存在");

        var entry = TemplateJson.ReadArray(Path.Combine(root, "projects.json"))
            .FirstOrDefault(o => string.Equals(TemplateJson.Str(o, "id"), templateId, StringComparison.OrdinalIgnoreCase));

        // 1. 项目
        var project = new TestingProject
        {
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? TemplateJson.Str(entry ?? new JsonObject(), "description") : description,
            Status = 1,
            EnvironmentIdsJson = "[]",
            MetadataJson = "{}",
            CreatedBy = "System",
            UpdatedBy = "System"
        };
        project = await _repository.InsertAsync(project, autoSave: true);
        var projectId = project.Id;

        // 2. 服务
        var serviceMap = new Dictionary<string, long>();
        foreach (var o in TemplateJson.ReadArray(Path.Combine(dir, "project-services.json")))
        {
            var entity = await _serviceRepository.InsertAsync(new TestingProjectService
            {
                ProjectId = projectId,
                Name = TemplateJson.Str(o, "name"),
                Description = TemplateJson.Str(o, "description"),
                CreatedBy = "System",
                UpdatedBy = "System"
            }, autoSave: true);
            var oldId = TemplateJson.Str(o, "id");
            if (!string.IsNullOrEmpty(oldId)) serviceMap[oldId] = entity.Id;
        }

        // 3. 环境
        var envMap = new Dictionary<string, long>();
        foreach (var o in TemplateJson.ReadArray(Path.Combine(dir, "project-environments.json")))
        {
            var entity = await _environmentRepository.InsertAsync(new TestingProjectEnvironment
            {
                ProjectId = projectId,
                Name = TemplateJson.Str(o, "name"),
                Description = TemplateJson.Str(o, "description"),
                Type = TemplateJson.IntVal(o, "type", 1),
                Status = TemplateJson.IntVal(o, "status", 1),
                VariablesJson = TemplateJson.RawJson(o, "variables"),
                HeadersJson = TemplateJson.RawJson(o, "headers"),
                DatabaseConfigJson = TemplateJson.RawJson(o, "databaseConfig"),
                CreatedBy = "System",
                UpdatedBy = "System"
            }, autoSave: true);
            var oldId = TemplateJson.Str(o, "id");
            if (!string.IsNullOrEmpty(oldId)) envMap[oldId] = entity.Id;
        }

        // 4. 环境服务配置
        var configs = new List<TestingEnvironmentServiceConfig>();
        foreach (var o in TemplateJson.ReadArray(Path.Combine(dir, "environment-service-configs.json")))
        {
            if (!envMap.TryGetValue(TemplateJson.Str(o, "environmentId"), out var envId)) continue;
            if (!serviceMap.TryGetValue(TemplateJson.Str(o, "projectServiceId"), out var serviceId)) continue;
            configs.Add(new TestingEnvironmentServiceConfig
            {
                EnvironmentId = envId,
                ProjectServiceId = serviceId,
                BaseUrl = TemplateJson.Str(o, "baseUrl"),
                CreatedBy = "System"
            });
        }
        if (configs.Count > 0) await _serviceConfigRepository.InsertManyAsync(configs, autoSave: true);

        // 5. 用例分类（两遍回填父级引用）
        var categoryPairs = new List<(string OldId, string OldParentId, TestingProjectCaseCategory Entity)>();
        foreach (var o in TemplateJson.ReadArray(Path.Combine(dir, "project-case-categories.json")))
        {
            var oldId = TemplateJson.Str(o, "id");
            if (string.IsNullOrEmpty(oldId)) continue;
            categoryPairs.Add((oldId, TemplateJson.Str(o, "parentId"), new TestingProjectCaseCategory
            {
                ProjectId = projectId,
                ParentId = null,
                Name = TemplateJson.Str(o, "name"),
                Description = TemplateJson.Str(o, "description"),
                Order = TemplateJson.IntVal(o, "order"),
                CreatedBy = "System"
            }));
        }
        await _categoryRepository.InsertManyAsync(categoryPairs.Select(p => p.Entity).ToList(), autoSave: true);
        var categoryMap = categoryPairs.ToDictionary(p => p.OldId, p => p.Entity.Id);
        var categoriesToUpdate = new List<TestingProjectCaseCategory>();
        foreach (var (_, oldParentId, entity) in categoryPairs)
        {
            if (!string.IsNullOrEmpty(oldParentId) && categoryMap.TryGetValue(oldParentId, out var parentId))
            {
                entity.ParentId = parentId;
                categoriesToUpdate.Add(entity);
            }
        }
        if (categoriesToUpdate.Count > 0) await _categoryRepository.UpdateManyAsync(categoriesToUpdate, autoSave: true);

        // 6. 用例（重映射分类/服务引用；两遍回填模板引用）
        var casePairs = new List<(string OldId, string OldTemplateId, TestingProjectCase Entity)>();
        foreach (var o in TemplateJson.ReadArray(Path.Combine(dir, "project-cases.json")))
        {
            var oldId = TemplateJson.Str(o, "id");
            if (string.IsNullOrEmpty(oldId)) continue;

            long? categoryId = null;
            var oldCategoryId = TemplateJson.Str(o, "categoryId");
            if (!string.IsNullOrEmpty(oldCategoryId) && categoryMap.TryGetValue(oldCategoryId, out var cid)) categoryId = cid;

            casePairs.Add((oldId, TemplateJson.Str(o, "templateId"), new TestingProjectCase
            {
                ProjectId = projectId,
                CategoryId = categoryId,
                TemplateId = null,
                CaseNumber = TemplateJson.Str(o, "caseNumber"),
                Name = TemplateJson.Str(o, "name"),
                Description = TemplateJson.Str(o, "description"),
                IsTemplate = TemplateJson.BoolVal(o, "isTemplate"),
                LevelsJson = TemplateJson.RawJson(o, "levels"),
                TagsJson = TemplateJson.RawJson(o, "tags"),
                StepsJson = RemapStepsToDb(TemplateJson.Get(o, "steps"), serviceMap),
                TestDataJson = TemplateJson.RawJson(o, "testData"),
                Order = TemplateJson.IntVal(o, "order"),
                Status = TemplateJson.IntVal(o, "status", 1),
                CreatedBy = "System",
                UpdatedBy = "System"
            }));
        }
        await _caseRepository.InsertManyAsync(casePairs.Select(p => p.Entity).ToList(), autoSave: true);
        var caseMap = casePairs.ToDictionary(p => p.OldId, p => p.Entity.Id);
        var casesToUpdate = new List<TestingProjectCase>();
        foreach (var (_, oldTemplateId, entity) in casePairs)
        {
            if (!string.IsNullOrEmpty(oldTemplateId) && caseMap.TryGetValue(oldTemplateId, out var templateCaseId))
            {
                entity.TemplateId = templateCaseId;
                casesToUpdate.Add(entity);
            }
        }
        if (casesToUpdate.Count > 0) await _caseRepository.UpdateManyAsync(casesToUpdate, autoSave: true);

        return projectId;
    }

    #endregion

    #region 保存项目为模板

    public async Task<string> SaveProjectAsTemplateAsync(long projectId, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new UserFriendlyException("模板名称不能为空");
        var project = await _repository.FindAsync(projectId) ?? throw new UserFriendlyException("项目不存在");

        var root = ResolveTemplatesRoot();
        var templateId = GenerateTemplateId(root, name);
        var dir = Path.Combine(root, templateId);
        Directory.CreateDirectory(dir);

        var services = await ProjectChildrenAsync(_serviceRepository, s => s.ProjectId == projectId, s => s.Id);
        var environments = await ProjectChildrenAsync(_environmentRepository, e => e.ProjectId == projectId, e => e.Id);
        var envIds = environments.Select(e => e.Id).ToList();
        var configs = await AsyncExecuter.ToListAsync((await _serviceConfigRepository.GetQueryableAsync())
            .Where(c => envIds.Contains(c.EnvironmentId)).OrderBy(c => c.Id));
        var categories = await ProjectChildrenAsync(_categoryRepository, c => c.ProjectId == projectId, c => c.Order);
        var cases = await ProjectChildrenAsync(_caseRepository, c => c.ProjectId == projectId, c => c.Order);

        // 数据库 long ID → 模板字符串 ID
        string SvcId(long id) => services.FirstOrDefault(s => s.Id == id)?.Id.ToString() ?? string.Empty;
        string EnvId(long id) => environments.FirstOrDefault(e => e.Id == id)?.Id.ToString() ?? string.Empty;
        string CatId(long? id) => id.HasValue ? categories.FirstOrDefault(c => c.Id == id)?.Id.ToString() ?? string.Empty : string.Empty;
        string CaseId(long? id) => id.HasValue ? cases.FirstOrDefault(c => c.Id == id)?.Id.ToString() ?? string.Empty : string.Empty;

        // 服务
        TemplateJson.Write(Path.Combine(dir, "project-services.json"), new JsonArray(services.Select(s => (JsonNode)new JsonObject
        {
            ["id"] = s.Id.ToString(),
            ["name"] = s.Name,
            ["description"] = s.Description ?? string.Empty,
            ["projectId"] = templateId,
            ["createdAt"] = s.CreationTime.ToString("O"),
            ["updatedAt"] = (s.LastModificationTime ?? s.CreationTime).ToString("O"),
            ["createdBy"] = s.CreatedBy ?? "System",
            ["updatedBy"] = s.UpdatedBy ?? "System"
        }).ToArray()));

        // 环境
        TemplateJson.Write(Path.Combine(dir, "project-environments.json"), new JsonArray(environments.Select(e => (JsonNode)new JsonObject
        {
            ["Id"] = e.Id.ToString(),
            ["Name"] = e.Name,
            ["Description"] = e.Description ?? string.Empty,
            ["ProjectId"] = templateId,
            ["Type"] = e.Type,
            ["EnvironmentServiceConfigs"] = new JsonArray(),
            ["Variables"] = ParseNode(e.VariablesJson) ?? new JsonObject(),
            ["Headers"] = ParseNode(e.HeadersJson) ?? new JsonObject(),
            ["DatabaseConfig"] = ParseNode(e.DatabaseConfigJson),
            ["Status"] = e.Status,
            ["CreatedAt"] = e.CreationTime.ToString("O"),
            ["UpdatedAt"] = (e.LastModificationTime ?? e.CreationTime).ToString("O"),
            ["CreatedBy"] = e.CreatedBy ?? "System",
            ["UpdatedBy"] = e.UpdatedBy ?? "System"
        }).ToArray()));

        // 环境服务配置
        TemplateJson.Write(Path.Combine(dir, "environment-service-configs.json"), new JsonArray(configs.Select(c => (JsonNode)new JsonObject
        {
            ["id"] = c.Id.ToString(),
            ["environmentId"] = EnvId(c.EnvironmentId),
            ["projectServiceId"] = SvcId(c.ProjectServiceId),
            ["baseUrl"] = c.BaseUrl ?? string.Empty,
            ["createdAt"] = c.CreationTime.ToString("O"),
            ["createdBy"] = c.CreatedBy ?? string.Empty,
            ["updatedAt"] = (c.LastModificationTime ?? c.CreationTime).ToString("O")
        }).ToArray()));

        // 用例分类
        TemplateJson.Write(Path.Combine(dir, "project-case-categories.json"), new JsonArray(categories.Select(c => (JsonNode)new JsonObject
        {
            ["Id"] = c.Id.ToString(),
            ["Name"] = c.Name,
            ["Description"] = c.Description ?? string.Empty,
            ["ProjectId"] = templateId,
            ["ParentId"] = CatId(c.ParentId),
            ["Order"] = c.Order,
            ["CreatedAt"] = c.CreationTime.ToString("O"),
            ["CreatedBy"] = c.CreatedBy ?? "System"
        }).ToArray()));

        // 用例（模板不包含执行状态；步骤内的服务引用重映射为字符串 ID）
        TemplateJson.Write(Path.Combine(dir, "project-cases.json"), new JsonArray(cases.Select(c => (JsonNode)new JsonObject
        {
            ["Id"] = c.Id.ToString(),
            ["CaseNumber"] = c.CaseNumber ?? string.Empty,
            ["Name"] = c.Name,
            ["Description"] = c.Description ?? string.Empty,
            ["ProjectId"] = templateId,
            ["CategoryId"] = CatId(c.CategoryId),
            ["IsTemplate"] = c.IsTemplate,
            ["TemplateId"] = CaseId(c.TemplateId),
            ["Levels"] = ParseNode(c.LevelsJson) ?? new JsonArray(),
            ["Steps"] = RemapStepsToFile(c.StepsJson, SvcId) ?? new JsonArray(),
            ["TestData"] = ParseNode(c.TestDataJson) ?? new JsonObject(),
            ["Tags"] = ParseNode(c.TagsJson) ?? new JsonArray(),
            ["Order"] = c.Order,
            ["Status"] = c.Status,
            ["CreatedAt"] = c.CreationTime.ToString("O"),
            ["UpdatedAt"] = (c.LastModificationTime ?? c.CreationTime).ToString("O"),
            ["CreatedBy"] = c.CreatedBy ?? "System",
            ["UpdatedBy"] = c.UpdatedBy ?? "System",
            ["LastExecutionResult"] = null,
            ["LastExecutionTime"] = null
        }).ToArray()));

        // 索引（projects.json）
        var indexPath = Path.Combine(root, "projects.json");
        var index = TemplateJson.ReadArray(indexPath);
        index.Add(new JsonObject
        {
            ["id"] = templateId,
            ["name"] = name.Trim(),
            ["description"] = description ?? string.Empty,
            ["status"] = 1,
            ["environmentIds"] = new JsonArray(),
            ["metadata"] = new JsonObject(),
            ["createdAt"] = DateTime.Now.ToString("O"),
            ["updatedAt"] = DateTime.Now.ToString("O"),
            ["createdBy"] = "System",
            ["updatedBy"] = "System"
        });
        TemplateJson.Write(indexPath, new JsonArray(index.Select(o => (JsonNode)o.DeepClone()).ToArray()));

        return templateId;
    }

    #endregion

    #region 私有辅助

    /// <summary>
    /// 解析模板根目录：优先配置 Testing:TemplatesPath，否则使用输出目录下随构建携带的 data/templates
    /// </summary>
    private string ResolveTemplatesRoot()
    {
        var configured = _configuration["Testing:TemplatesPath"];
        var dir = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(AppContext.BaseDirectory, "data", "templates")
            : Path.GetFullPath(configured);
        Directory.CreateDirectory(dir);
        return dir;
    }

    /// <summary>
    /// 校验模板ID（目录名）合法性
    /// </summary>
    private static void ValidateTemplateId(string templateId)
    {
        if (string.IsNullOrWhiteSpace(templateId)
            || templateId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || templateId == "." || templateId == "..")
        {
            throw new UserFriendlyException("无效的模板ID");
        }
    }

    /// <summary>
    /// 根据模板名称生成不重复的模板ID（目录名）
    /// </summary>
    private static string GenerateTemplateId(string root, string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var baseId = new string(name.Trim().Select(c => invalid.Contains(c) ? '-' : c).ToArray()).Trim('-', ' ');
        if (string.IsNullOrEmpty(baseId)) baseId = "template";

        var index = TemplateJson.ReadArray(Path.Combine(root, "projects.json"));
        var existing = new HashSet<string>(index.Select(o => TemplateJson.Str(o, "id")), StringComparer.OrdinalIgnoreCase);

        var candidate = baseId;
        var suffix = 2;
        while (existing.Contains(candidate) || Directory.Exists(Path.Combine(root, candidate)))
        {
            candidate = $"{baseId}-{suffix++}";
        }
        return candidate;
    }

    /// <summary>
    /// 查询项目下的子实体（过滤软删除并按指定字段排序）
    /// </summary>
    private async Task<List<TEntity>> ProjectChildrenAsync<TEntity, TKey>(
        IRepository<TEntity, long> repository,
        System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate,
        System.Linq.Expressions.Expression<Func<TEntity, TKey>> orderBy)
        where TEntity : class, Volo.Abp.Domain.Entities.IEntity<long>
    {
        var query = await repository.GetQueryableAsync();
        return await AsyncExecuter.ToListAsync(query.Where(predicate).OrderBy(orderBy));
    }

    /// <summary>
    /// 导入：将步骤 JSON 中字符串形式的服务 ID 重映射为数据库 long ID
    /// </summary>
    private static string? RemapStepsToDb(JsonNode? stepsNode, Dictionary<string, long> serviceMap)
    {
        if (stepsNode is not JsonArray steps) return null;
        foreach (var step in steps.OfType<JsonObject>())
        {
            if (TemplateJson.Get(step, "apiConfig") is JsonObject api)
            {
                var oldServiceId = TemplateJson.Get(api, "serviceId")?.ToString();
                long sid = 0;
                if (!string.IsNullOrEmpty(oldServiceId) && serviceMap.TryGetValue(oldServiceId, out var mapped))
                {
                    sid = mapped;
                }
                foreach (var key in KeysOf(api, "serviceId")) api.Remove(key);
                api["ServiceId"] = sid;
            }
        }
        return steps.ToJsonString();
    }

    /// <summary>
    /// 导出：将步骤 JSON 中 long 形式的服务 ID 重映射为模板字符串 ID
    /// </summary>
    private static JsonNode? RemapStepsToFile(string? stepsJson, Func<long, string> toTemplateServiceId)
    {
        var steps = TemplateJson.ParseArray(stepsJson);
        if (steps == null) return null;
        foreach (var step in steps.OfType<JsonObject>())
        {
            if (TemplateJson.Get(step, "apiConfig") is not JsonObject api) continue;

            long serviceId = 0;
            foreach (var key in KeysOf(api, "serviceId"))
            {
                var v = api[key];
                if (v != null && long.TryParse(v.ToString(), out var parsed)) serviceId = parsed;
                api.Remove(key);
            }
            api["ServiceId"] = toTemplateServiceId(serviceId);
        }
        return steps;
    }

    /// <summary>
    /// 大小写不敏感地找出对象中匹配的所有键
    /// </summary>
    private static List<string> KeysOf(JsonObject o, string name)
        => o.Select(kv => kv.Key).Where(k => string.Equals(k, name, StringComparison.OrdinalIgnoreCase)).ToList();

    private static JsonNode? ParseNode(string? json)
        => string.IsNullOrWhiteSpace(json) ? null : JsonNode.Parse(json);

    #endregion
}
