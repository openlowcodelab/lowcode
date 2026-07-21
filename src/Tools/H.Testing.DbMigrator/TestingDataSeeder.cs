using System.Text.Json;
using System.Text.Json.Nodes;
using H.Testing.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace H.Testing.DbMigrator;

/// <summary>
/// 从 SeedData 下的 JSON 读取历史数据，重映射字符串 ID 为 long，并写入指定租户。
/// </summary>
public class TestingDataSeeder
{
    private readonly TestingSeedDbContext _db;
    private readonly Guid _tenantId;
    private readonly string _seedDir;

    public TestingDataSeeder(TestingSeedDbContext db, Guid tenantId, string seedDir)
    {
        _db = db;
        _tenantId = tenantId;
        _seedDir = seedDir;
    }

    public async Task SeedAsync()
    {
        if (await _db.Projects.AnyAsync(p => p.TenantId == _tenantId))
        {
            Console.WriteLine("目标租户下已存在测试数据，跳过种子。");
            return;
        }

        var projectMap = await SeedProjectsAsync();
        var serviceMap = await SeedServicesAsync(projectMap);
        var envMap = await SeedEnvironmentsAsync(projectMap);
        await SeedEnvServiceConfigsAsync(envMap, serviceMap);
        var categoryMap = await SeedCategoriesAsync(projectMap);
        var caseMap = await SeedCasesAsync(projectMap, categoryMap, serviceMap);
        await SeedExecutionRecordsAsync(projectMap, caseMap, envMap);

        Console.WriteLine("种子数据写入完成。");
    }

    private async Task<Dictionary<string, long>> SeedProjectsAsync()
    {
        var pairs = new List<(string oldId, TestingProject entity)>();
        foreach (var o in ReadArray("projects.json"))
        {
            var oldId = Str(o, "id");
            if (string.IsNullOrEmpty(oldId)) continue;
            var e = new TestingProject
            {
                TenantId = _tenantId,
                Name = Str(o, "name"),
                Description = Str(o, "description"),
                Status = IntVal(o, "status", 1),
                MetadataJson = RawJson(o, "metadata"),
                CreatedBy = Str(o, "createdBy"),
                UpdatedBy = Str(o, "updatedBy")
            };
            ApplyAudit(e, o);
            pairs.Add((oldId, e));
        }

        _db.Projects.AddRange(pairs.Select(p => p.entity));
        await _db.SaveChangesAsync();
        Console.WriteLine($"项目: {pairs.Count} 条");
        return pairs.ToDictionary(p => p.oldId, p => p.entity.Id);
    }

    private async Task<Dictionary<string, long>> SeedServicesAsync(Dictionary<string, long> projectMap)
    {
        var pairs = new List<(string oldId, TestingProjectService entity)>();
        foreach (var o in ReadArray("catering/project-services.json"))
        {
            var oldId = Str(o, "id");
            if (string.IsNullOrEmpty(oldId)) continue;
            if (!projectMap.TryGetValue(Str(o, "projectId"), out var projectId)) continue;
            var e = new TestingProjectService
            {
                TenantId = _tenantId,
                ProjectId = projectId,
                Name = Str(o, "name"),
                Description = Str(o, "description"),
                CreatedBy = Str(o, "createdBy"),
                UpdatedBy = Str(o, "updatedBy")
            };
            ApplyAudit(e, o);
            pairs.Add((oldId, e));
        }

        _db.ProjectServices.AddRange(pairs.Select(p => p.entity));
        await _db.SaveChangesAsync();
        Console.WriteLine($"项目服务: {pairs.Count} 条");
        return pairs.ToDictionary(p => p.oldId, p => p.entity.Id);
    }

    private async Task<Dictionary<string, long>> SeedEnvironmentsAsync(Dictionary<string, long> projectMap)
    {
        var pairs = new List<(string oldId, TestingProjectEnvironment entity)>();
        foreach (var o in ReadArray("catering/project-environments.json"))
        {
            var oldId = Str(o, "id");
            if (string.IsNullOrEmpty(oldId)) continue;
            if (!projectMap.TryGetValue(Str(o, "projectId"), out var projectId)) continue;
            var e = new TestingProjectEnvironment
            {
                TenantId = _tenantId,
                ProjectId = projectId,
                Name = Str(o, "name"),
                Description = Str(o, "description"),
                Type = IntVal(o, "type", 1),
                Status = IntVal(o, "status", 1),
                VariablesJson = RawJson(o, "variables"),
                HeadersJson = RawJson(o, "headers"),
                DatabaseConfigJson = RawJson(o, "databaseConfig"),
                CreatedBy = Str(o, "createdBy"),
                UpdatedBy = Str(o, "updatedBy")
            };
            ApplyAudit(e, o);
            pairs.Add((oldId, e));
        }

        _db.ProjectEnvironments.AddRange(pairs.Select(p => p.entity));
        await _db.SaveChangesAsync();
        Console.WriteLine($"项目环境: {pairs.Count} 条");
        return pairs.ToDictionary(p => p.oldId, p => p.entity.Id);
    }

    private async Task SeedEnvServiceConfigsAsync(Dictionary<string, long> envMap, Dictionary<string, long> serviceMap)
    {
        var list = new List<TestingEnvironmentServiceConfig>();
        foreach (var o in ReadArray("catering/environment-service-configs.json"))
        {
            if (!envMap.TryGetValue(Str(o, "environmentId"), out var envId)) continue;
            if (!serviceMap.TryGetValue(Str(o, "projectServiceId"), out var serviceId)) continue;
            var e = new TestingEnvironmentServiceConfig
            {
                TenantId = _tenantId,
                EnvironmentId = envId,
                ProjectServiceId = serviceId,
                BaseUrl = Str(o, "baseUrl"),
                CreatedBy = Str(o, "createdBy")
            };
            ApplyAudit(e, o);
            list.Add(e);
        }

        _db.EnvironmentServiceConfigs.AddRange(list);
        await _db.SaveChangesAsync();
        Console.WriteLine($"环境服务配置: {list.Count} 条");
    }

    private async Task<Dictionary<string, long>> SeedCategoriesAsync(Dictionary<string, long> projectMap)
    {
        var raw = ReadArray("catering/project-case-categories.json");
        var pairs = new List<(string oldId, string oldParentId, TestingProjectCaseCategory entity)>();
        foreach (var o in raw)
        {
            var oldId = Str(o, "id");
            if (string.IsNullOrEmpty(oldId)) continue;
            if (!projectMap.TryGetValue(Str(o, "projectId"), out var projectId)) continue;
            var e = new TestingProjectCaseCategory
            {
                TenantId = _tenantId,
                ProjectId = projectId,
                ParentId = null,
                Name = Str(o, "name"),
                Description = Str(o, "description"),
                Order = IntVal(o, "order"),
                CreatedBy = Str(o, "createdBy")
            };
            ApplyAudit(e, o);
            pairs.Add((oldId, Str(o, "parentId"), e));
        }

        _db.ProjectCaseCategories.AddRange(pairs.Select(p => p.entity));
        await _db.SaveChangesAsync();

        var map = pairs.ToDictionary(p => p.oldId, p => p.entity.Id);

        // 二次回填父级引用
        foreach (var (_, oldParentId, entity) in pairs)
        {
            if (!string.IsNullOrEmpty(oldParentId) && map.TryGetValue(oldParentId, out var parentId))
            {
                entity.ParentId = parentId;
            }
        }
        await _db.SaveChangesAsync();
        Console.WriteLine($"用例分类: {pairs.Count} 条");
        return map;
    }

    private async Task<Dictionary<string, long>> SeedCasesAsync(
        Dictionary<string, long> projectMap,
        Dictionary<string, long> categoryMap,
        Dictionary<string, long> serviceMap)
    {
        var raw = ReadArray("catering/project-cases.json");
        var pairs = new List<(string oldId, string? oldTemplateId, TestingProjectCase entity)>();
        foreach (var o in raw)
        {
            var oldId = Str(o, "id");
            if (string.IsNullOrEmpty(oldId)) continue;
            if (!projectMap.TryGetValue(Str(o, "projectId"), out var projectId)) continue;

            long? categoryId = null;
            var oldCat = Str(o, "categoryId");
            if (!string.IsNullOrEmpty(oldCat) && categoryMap.TryGetValue(oldCat, out var cid)) categoryId = cid;

            var e = new TestingProjectCase
            {
                TenantId = _tenantId,
                ProjectId = projectId,
                CategoryId = categoryId,
                TemplateId = null,
                CaseNumber = Str(o, "caseNumber"),
                Name = Str(o, "name"),
                Description = Str(o, "description"),
                IsTemplate = BoolVal(o, "isTemplate"),
                LevelsJson = RawJson(o, "levels"),
                TagsJson = RawJson(o, "tags"),
                StepsJson = RemapSteps(Get(o, "steps"), serviceMap),
                TestDataJson = RawJson(o, "testData"),
                Order = IntVal(o, "order"),
                Status = IntVal(o, "status", 1),
                LastExecutionResult = NullableInt(o, "lastExecutionResult"),
                LastExecutionTime = DateVal(o, "lastExecutionTime"),
                CreatedBy = Str(o, "createdBy"),
                UpdatedBy = Str(o, "updatedBy")
            };
            ApplyAudit(e, o);
            var oldTemplateId = Str(o, "templateId");
            pairs.Add((oldId, string.IsNullOrEmpty(oldTemplateId) ? null : oldTemplateId, e));
        }

        _db.ProjectCases.AddRange(pairs.Select(p => p.entity));
        await _db.SaveChangesAsync();

        var map = pairs.ToDictionary(p => p.oldId, p => p.entity.Id);

        foreach (var (_, oldTemplateId, entity) in pairs)
        {
            if (!string.IsNullOrEmpty(oldTemplateId) && map.TryGetValue(oldTemplateId, out var templateId))
            {
                entity.TemplateId = templateId;
            }
        }
        await _db.SaveChangesAsync();
        Console.WriteLine($"测试用例: {pairs.Count} 条");
        return map;
    }

    private async Task SeedExecutionRecordsAsync(
        Dictionary<string, long> projectMap,
        Dictionary<string, long> caseMap,
        Dictionary<string, long> envMap)
    {
        var list = new List<TestingExecutionRecord>();
        foreach (var o in ReadArray("catering/execution-records.json"))
        {
            if (!projectMap.TryGetValue(Str(o, "projectId"), out var projectId)) continue;
            caseMap.TryGetValue(Str(o, "testCaseId"), out var testCaseId);
            envMap.TryGetValue(Str(o, "environmentId"), out var envId);

            var e = new TestingExecutionRecord
            {
                TenantId = _tenantId,
                ProjectId = projectId,
                TestCaseId = testCaseId,
                EnvironmentId = envId,
                TestCaseName = Str(o, "testCaseName"),
                EnvironmentName = Str(o, "environmentName"),
                Status = IntVal(o, "status"),
                StartTime = DateVal(o, "startTime") ?? DateTime.Now,
                EndTime = DateVal(o, "endTime"),
                Duration = LongVal(o, "duration"),
                TotalSteps = IntVal(o, "totalSteps"),
                SuccessSteps = IntVal(o, "successSteps"),
                FailedSteps = IntVal(o, "failedSteps"),
                SkippedSteps = IntVal(o, "skippedSteps"),
                StepRecordsJson = RawJson(o, "stepRecords"),
                ErrorMessage = Str(o, "errorMessage"),
                ExecutedBy = Str(o, "executedBy"),
                EnvironmentSnapshotJson = RawJson(o, "environmentSnapshot")
            };
            ApplyAudit(e, o);
            list.Add(e);
        }

        _db.ExecutionRecords.AddRange(list);
        await _db.SaveChangesAsync();
        Console.WriteLine($"执行记录: {list.Count} 条");
    }

    // ===== helpers =====

    private List<JsonObject> ReadArray(string relPath)
    {
        var path = Path.Combine(_seedDir, relPath);
        if (!File.Exists(path)) return new List<JsonObject>();
        var node = JsonNode.Parse(File.ReadAllText(path));
        return node is JsonArray arr ? arr.OfType<JsonObject>().ToList() : new List<JsonObject>();
    }

    private static string? RemapSteps(JsonNode? stepsNode, Dictionary<string, long> serviceMap)
    {
        if (stepsNode is not JsonArray steps) return null;
        foreach (var step in steps.OfType<JsonObject>())
        {
            if (Get(step, "apiConfig") is JsonObject api)
            {
                var oldServiceId = Get(api, "serviceId")?.ToString();
                long sid = 0;
                if (!string.IsNullOrEmpty(oldServiceId) && serviceMap.TryGetValue(oldServiceId, out var mapped))
                {
                    sid = mapped;
                }
                foreach (var key in api.Select(k => k.Key).Where(k => string.Equals(k, "serviceId", StringComparison.OrdinalIgnoreCase)).ToList())
                {
                    api.Remove(key);
                }
                api["ServiceId"] = sid;
            }
        }
        return steps.ToJsonString();
    }

    private static JsonNode? Get(JsonObject o, string name)
    {
        foreach (var kv in o)
        {
            if (string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase)) return kv.Value;
        }
        return null;
    }

    private static string Str(JsonObject o, string name)
    {
        var v = Get(o, name);
        if (v is null) return string.Empty;
        try { return v.GetValue<string>(); }
        catch { return v.ToString(); }
    }

    private static int IntVal(JsonObject o, string name, int def = 0)
    {
        var v = Get(o, name);
        if (v is null) return def;
        try { return v.GetValue<int>(); }
        catch { return int.TryParse(v.ToString(), out var i) ? i : def; }
    }

    private static int? NullableInt(JsonObject o, string name)
    {
        var v = Get(o, name);
        if (v is null) return null;
        try { return v.GetValue<int>(); }
        catch { return int.TryParse(v.ToString(), out var i) ? i : (int?)null; }
    }

    private static long LongVal(JsonObject o, string name, long def = 0)
    {
        var v = Get(o, name);
        if (v is null) return def;
        try { return v.GetValue<long>(); }
        catch { return long.TryParse(v.ToString(), out var l) ? l : def; }
    }

    private static bool BoolVal(JsonObject o, string name, bool def = false)
    {
        var v = Get(o, name);
        if (v is null) return def;
        try { return v.GetValue<bool>(); }
        catch { return bool.TryParse(v.ToString(), out var b) ? b : def; }
    }

    private static DateTime? DateVal(JsonObject o, string name)
    {
        var s = Str(o, name);
        return DateTime.TryParse(s, out var d) ? d : (DateTime?)null;
    }

    private static string? RawJson(JsonObject o, string name)
    {
        var v = Get(o, name);
        return v?.ToJsonString();
    }

    private static void ApplyAudit(object entity, JsonObject o)
    {
        var created = DateVal(o, "createdAt");
        var updated = DateVal(o, "updatedAt");
        var creationProp = entity.GetType().GetProperty("CreationTime");
        creationProp?.GetSetMethod(true)?.Invoke(entity, new object[] { created ?? DateTime.Now });
        if (updated.HasValue)
        {
            var modProp = entity.GetType().GetProperty("LastModificationTime");
            modProp?.GetSetMethod(true)?.Invoke(entity, new object?[] { updated });
        }
    }
}
