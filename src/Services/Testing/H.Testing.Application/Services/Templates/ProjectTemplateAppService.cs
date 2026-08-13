using H.Testing.Application.Contracts;
using H.Testing.Application.Mapping;
using H.Testing.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Nodes;
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
    private readonly IRepository<ProjectEntity, long> _repository;
    private readonly IRepository<ProjectServiceEntity, long> _serviceRepository;
    private readonly IRepository<ProjectEnvEntity, long> _environmentRepository;
    private readonly IRepository<CaseCategoryEntity, long> _categoryRepository;
    private readonly IRepository<CaseEntity, long> _caseRepository;
    private readonly IRepository<CaseStepEntity, long> _stepRepository;
    private readonly IConfiguration _configuration;

    public ProjectTemplateAppService(
        IRepository<ProjectEntity, long> repository,
        IRepository<ProjectServiceEntity, long> serviceRepository,
        IRepository<ProjectEnvEntity, long> environmentRepository,
        IRepository<CaseCategoryEntity, long> categoryRepository,
        IRepository<CaseEntity, long> caseRepository,
        IRepository<CaseStepEntity, long> stepRepository,
        IConfiguration configuration)
    {
        _repository = repository;
        _serviceRepository = serviceRepository;
        _environmentRepository = environmentRepository;
        _categoryRepository = categoryRepository;
        _caseRepository = caseRepository;
        _stepRepository = stepRepository;
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
                CaseCount = TemplateJson.ReadArray(Path.Combine(dir, "project-cases.json")).Count
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
        var project = new ProjectEntity
        {
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? TemplateJson.Str(entry ?? new JsonObject(), "description") : description,
            Status = 1,
            //EnvironmentIdsJson = "[]",
            //MetadataJson = "{}"
        };

        project = await _repository.InsertAsync(project, autoSave: true);
        var projectId = project.Id;

        // 2. 服务
        var serviceMap = new Dictionary<string, long>();
        foreach (var o in TemplateJson.ReadArray(Path.Combine(dir, "project-services.json")))
        {
            var entity = await _serviceRepository.InsertAsync(new ProjectServiceEntity
            {
                ProjectId = projectId,
                Name = TemplateJson.Str(o, "name"),
                Description = TemplateJson.Str(o, "description")
            }, autoSave: true);
            var oldId = TemplateJson.Str(o, "id");
            if (!string.IsNullOrEmpty(oldId)) serviceMap[oldId] = entity.Id;
        }

        // 3. 环境
        var envMap = new Dictionary<string, long>();
        var envEntities = new List<ProjectEnvEntity>();
        foreach (var o in TemplateJson.ReadArray(Path.Combine(dir, "project-environments.json")))
        {
            var entity = await _environmentRepository.InsertAsync(new ProjectEnvEntity
            {
                ProjectId = projectId,
                Name = TemplateJson.Str(o, "name"),
                Description = TemplateJson.Str(o, "description"),
                Type = TemplateJson.IntVal(o, "type", 1),
                VariablesJson = TemplateJson.RawJson(o, "variables"),
                HeadersJson = TemplateJson.RawJson(o, "headers")
            }, autoSave: true);
            envEntities.Add(entity);
            var oldId = TemplateJson.Str(o, "id");
            if (!string.IsNullOrEmpty(oldId)) envMap[oldId] = entity.Id;
        }

        // 4. 环境服务配置（重映射后合并写入环境的 ServiceConfigsJson）
        var configsByEnv = new Dictionary<long, Dictionary<long, string>>();
        foreach (var o in TemplateJson.ReadArray(Path.Combine(dir, "environment-service-configs.json")))
        {
            if (!envMap.TryGetValue(TemplateJson.Str(o, "environmentId"), out var envId)) continue;
            if (!serviceMap.TryGetValue(TemplateJson.Str(o, "projectServiceId"), out var serviceId)) continue;
            if (!configsByEnv.TryGetValue(envId, out var serviceConfigs)) configsByEnv[envId] = serviceConfigs = new Dictionary<long, string>();
            serviceConfigs[serviceId] = TemplateJson.Str(o, "baseUrl");
        }
        var envsToUpdate = envEntities.Where(e => configsByEnv.ContainsKey(e.Id)).ToList();
        foreach (var env in envsToUpdate)
        {
            env.ServiceConfigsJson = TestingMappers.SerializeServiceConfigs(configsByEnv[env.Id]);
        }
        if (envsToUpdate.Count > 0) await _environmentRepository.UpdateManyAsync(envsToUpdate, autoSave: true);

        // 5. 用例分类（两遍回填父级引用）
        var categoryPairs = new List<(string OldId, string OldParentId, CaseCategoryEntity Entity)>();
        foreach (var o in TemplateJson.ReadArray(Path.Combine(dir, "project-case-categories.json")))
        {
            var oldId = TemplateJson.Str(o, "id");
            if (string.IsNullOrEmpty(oldId)) continue;
            categoryPairs.Add((oldId, TemplateJson.Str(o, "parentId"), new CaseCategoryEntity
            {
                ProjectId = projectId,
                ParentId = null,
                Name = TemplateJson.Str(o, "name"),
                Description = TemplateJson.Str(o, "description"),
                Order = TemplateJson.IntVal(o, "order")
            }));
        }
        await _categoryRepository.InsertManyAsync(categoryPairs.Select(p => p.Entity).ToList(), autoSave: true);
        var categoryMap = categoryPairs.ToDictionary(p => p.OldId, p => p.Entity.Id);
        var categoriesToUpdate = new List<CaseCategoryEntity>();
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
        var casePairs = new List<(string OldId, string OldTemplateId, CaseEntity Entity, JsonNode? StepsNode)>();
        foreach (var o in TemplateJson.ReadArray(Path.Combine(dir, "project-cases.json")))
        {
            var oldId = TemplateJson.Str(o, "id");
            if (string.IsNullOrEmpty(oldId)) continue;

            long? categoryId = null;
            var oldCategoryId = TemplateJson.Str(o, "categoryId");
            if (!string.IsNullOrEmpty(oldCategoryId) && categoryMap.TryGetValue(oldCategoryId, out var cid)) categoryId = cid;

            casePairs.Add((oldId, TemplateJson.Str(o, "templateId"), new CaseEntity
            {
                ProjectId = projectId,
                CategoryId = categoryId,
                TemplateId = null,
                CaseName = TemplateJson.Str(o, "name"),
                Description = TemplateJson.Str(o, "description"),
                IsTemplate = TemplateJson.BoolVal(o, "isTemplate"),
                Level = ParseLevel(o),
                Order = TemplateJson.IntVal(o, "order"),
                Status = TemplateJson.IntVal(o, "status", 1)
            }, TemplateJson.Get(o, "steps")));
        }

        await _caseRepository.InsertManyAsync(casePairs.Select(p => p.Entity).ToList(), autoSave: true);

        // 6.1 用例步骤（重映射步骤内的服务引用后写入 CaseStep 表）
        var stepEntities = new List<CaseStepEntity>();
        foreach (var (_, _, entity, stepsNode) in casePairs)
        {
            foreach (var step in DeserializeSteps(RemapStepsToDb(stepsNode, serviceMap)))
            {
                stepEntities.Add(step.ToEntity(entity.Id));
            }
        }
        if (stepEntities.Count > 0) await _stepRepository.InsertManyAsync(stepEntities, autoSave: true);

        var caseMap = casePairs.ToDictionary(p => p.OldId, p => p.Entity.Id);
        var casesToUpdate = new List<CaseEntity>();
        foreach (var (_, oldTemplateId, entity, _) in casePairs)
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
        var categories = await ProjectChildrenAsync(_categoryRepository, c => c.ProjectId == projectId, c => c.Order);
        var cases = await ProjectChildrenAsync(_caseRepository, c => c.ProjectId == projectId, c => c.Order);
        var caseIds = cases.Select(c => c.Id).ToList();
        var stepQuery = await _stepRepository.GetQueryableAsync();
        var stepEntities = await AsyncExecuter.ToListAsync(
            stepQuery.Where(s => caseIds.Contains(s.CaseId)).OrderBy(s => s.Order).ThenBy(s => s.Id));
        var stepsByCase = stepEntities.GroupBy(s => s.CaseId)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ToDto()).ToList());

        // 数据库 long ID → 模板字符串 ID
        string SvcId(long id) => services.FirstOrDefault(s => s.Id == id)?.Id.ToString() ?? string.Empty;
        string CatId(long? id) => id.HasValue ? categories.FirstOrDefault(c => c.Id == id)?.Id.ToString() ?? string.Empty : string.Empty;
        string CaseId(long? id) => id.HasValue ? cases.FirstOrDefault(c => c.Id == id)?.Id.ToString() ?? string.Empty : string.Empty;

        // 服务
        TemplateJson.Write(Path.Combine(dir, "project-services.json"), new JsonArray(services.Select(s => (JsonNode)new JsonObject
        {
            ["id"] = s.Id.ToString(),
            ["name"] = s.Name,
            ["description"] = s.Description ?? string.Empty,
            ["projectId"] = templateId
        }).ToArray()));

        // 环境
        TemplateJson.Write(Path.Combine(dir, "project-environments.json"), new JsonArray(environments.Select(e => (JsonNode)new JsonObject
        {
            ["Id"] = e.Id.ToString(),
            ["Name"] = e.Name,
            ["Description"] = e.Description ?? string.Empty,
            ["ProjectId"] = templateId,
            ["Type"] = e.Type,
            ["Variables"] = ParseNode(e.VariablesJson) ?? new JsonObject(),
            ["Headers"] = ParseNode(e.HeadersJson) ?? new JsonObject()
        }).ToArray()));

        // 环境服务配置（从环境的 ServiceConfigsJson 提取，服务引用重映射为字符串 ID）
        var configNodes = new List<JsonNode>();
        foreach (var e in environments)
        {
            foreach (var kv in TestingMappers.DeserializeServiceConfigs(e.ServiceConfigsJson))
            {
                configNodes.Add(new JsonObject
                {
                    ["environmentId"] = e.Id.ToString(),
                    ["projectServiceId"] = SvcId(kv.Key),
                    ["baseUrl"] = kv.Value ?? string.Empty
                });
            }
        }
        TemplateJson.Write(Path.Combine(dir, "environment-service-configs.json"), new JsonArray(configNodes.ToArray()));

        // 用例分类
        TemplateJson.Write(Path.Combine(dir, "project-case-categories.json"), new JsonArray(categories.Select(c => (JsonNode)new JsonObject
        {
            ["Id"] = c.Id.ToString(),
            ["Name"] = c.Name,
            ["Description"] = c.Description ?? string.Empty,
            ["ProjectId"] = templateId,
            ["ParentId"] = CatId(c.ParentId),
            ["Order"] = c.Order
        }).ToArray()));

        // 用例（模板不包含执行状态；步骤内的服务引用重映射为字符串 ID）
        TemplateJson.Write(Path.Combine(dir, "project-cases.json"), new JsonArray(cases.Select(c => (JsonNode)new JsonObject
        {
            ["Id"] = c.Id.ToString(),
            ["Name"] = c.CaseName,
            ["Description"] = c.Description ?? string.Empty,
            ["ProjectId"] = templateId,
            ["CategoryId"] = CatId(c.CategoryId),
            ["IsTemplate"] = c.IsTemplate,
            ["TemplateId"] = CaseId(c.TemplateId),
            ["Level"] = ((CaseLevel)c.Level).ToString(),
            ["Steps"] = RemapStepsToFile(SerializeSteps(stepsByCase.GetValueOrDefault(c.Id)), SvcId) ?? new JsonArray(),
            ["Order"] = c.Order,
            ["Status"] = c.Status,
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

    /// <summary>
    /// 导入：解析模板级别（新格式 level 字符串 / 旧格式 levels 数组取首个有效值），无则默认 P1
    /// </summary>
    private static int ParseLevel(JsonObject o)
    {
        var value = TemplateJson.Str(o, "level");
        if (string.IsNullOrWhiteSpace(value) && TemplateJson.Get(o, "levels") is JsonArray levels)
        {
            foreach (var item in levels)
            {
                var candidate = item?.ToString().Trim().ToUpperInvariant();
                if (candidate is "P0" or "P1" or "P2" or "P3")
                {
                    return (int)Enum.Parse<CaseLevel>(candidate);
                }
            }
        }

        value = value?.Trim().ToUpperInvariant();
        return value is "P0" or "P1" or "P2" or "P3" ? (int)Enum.Parse<CaseLevel>(value) : (int)CaseLevel.P1;
    }

    /// <summary>
    /// 导入：将模板步骤 JSON 反序列化为步骤 DTO 列表
    /// </summary>
    private static List<CaseStepDto> DeserializeSteps(string? json)
        => string.IsNullOrWhiteSpace(json)
            ? new List<CaseStepDto>()
            : JsonSerializer.Deserialize<List<CaseStepDto>>(json, TestingMappers.JsonOptions) ?? new List<CaseStepDto>();

    /// <summary>
    /// 导出：将步骤 DTO 列表序列化为 JSON
    /// </summary>
    private static string? SerializeSteps(List<CaseStepDto>? steps)
        => steps == null || steps.Count == 0 ? null : JsonSerializer.Serialize(steps, TestingMappers.JsonOptions);

    #endregion
}
