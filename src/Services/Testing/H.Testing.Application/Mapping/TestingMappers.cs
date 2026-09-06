using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace H.Testing.Application.Mapping;

/// <summary>
/// 实体 &lt;-&gt; DTO 手工映射（复杂子对象以 JSON 列持久化）。
/// </summary>
public static class TestingMappers
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static string? Serialize<T>(T value)
        => value is null ? null : JsonSerializer.Serialize(value, JsonOptions);

    private static List<T> DeserializeList<T>(string? json)
        => string.IsNullOrWhiteSpace(json)
            ? new List<T>()
            : JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? new List<T>();

    private static Dictionary<string, T> DeserializeDict<T>(string? json)
        => string.IsNullOrWhiteSpace(json)
            ? new Dictionary<string, T>()
            : JsonSerializer.Deserialize<Dictionary<string, T>>(json, JsonOptions) ?? new Dictionary<string, T>();

    private static T? DeserializeObj<T>(string? json) where T : class
        => string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<T>(json, JsonOptions);

    // === Project ===
    public static ProjectDto ToDto(this ProjectEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description ?? string.Empty,
        Status = (ProjectStatus)e.Status,
        KnowledgeBaseId = e.KnowledgeBaseId
    };

    public static void Apply(this ProjectDto dto, ProjectEntity e)
    {
        e.Name = dto.Name;
        e.Description = dto.Description;
        e.Status = (int)dto.Status;
        e.KnowledgeBaseId = dto.KnowledgeBaseId;
    }

    // === ProjectService ===
    public static ProjectServiceDto ToDto(this ProjectServiceEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description ?? string.Empty,
        ProjectId = e.ProjectId,
        CreationTime = e.CreationTime
    };

    public static void Apply(this ProjectServiceDto dto, ProjectServiceEntity e)
    {
        e.Name = dto.Name;
        e.Description = dto.Description;
        e.ProjectId = dto.ProjectId;
    }

    // === ProjectEnvironment ===
    public static ProjectEnvDto ToDto(this ProjectEnvEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description ?? string.Empty,
        ProjectId = e.ProjectId,
        Type = (EnvironmentType)e.Type,
        EnvironmentServiceConfigs = e.ToConfigDtos(),
        Variables = DeserializeDict<string>(e.VariablesJson),
        Headers = DeserializeDict<string>(e.HeadersJson)
    };

    public static void Apply(this ProjectEnvDto dto, ProjectEnvEntity e)
    {
        e.Name = dto.Name;
        e.Description = dto.Description;
        e.ProjectId = dto.ProjectId;
        e.Type = (int)dto.Type;
        e.VariablesJson = Serialize(dto.Variables);
        e.HeadersJson = Serialize(dto.Headers);
        // 服务配置由 ProjectServiceConfigAppService 单独维护，此处不覆盖 ServiceConfigsJson
    }

    // === EnvironmentServiceConfig（存储于环境的 ServiceConfigsJson 字段） ===
    public static Dictionary<long, string> DeserializeServiceConfigs(string? json)
        => string.IsNullOrWhiteSpace(json)
            ? new Dictionary<long, string>()
            : JsonSerializer.Deserialize<Dictionary<long, string>>(json, JsonOptions) ?? new Dictionary<long, string>();

    public static string SerializeServiceConfigs(Dictionary<long, string> configs)
        => JsonSerializer.Serialize(configs, JsonOptions);

    public static List<ProjectEnvConfigDto> ToConfigDtos(this ProjectEnvEntity e)
        => DeserializeServiceConfigs(e.ServiceConfigsJson)
            .Select(kv => new ProjectEnvConfigDto
            {
                EnvironmentId = e.Id,
                ProjectServiceId = kv.Key,
                BaseUrl = kv.Value ?? string.Empty
            }).ToList();

    // === Environment（执行时视图，由环境 + 服务配置组合而成） ===
    public static EnvironmentDto ToEnvironmentDto(this ProjectEnvEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description ?? string.Empty,
        ProjectId = e.ProjectId,
        Config = DeserializeDict<string>(e.VariablesJson)
            .ToDictionary(kv => kv.Key, kv => (object)(kv.Value ?? string.Empty)),
        ServiceEndpoints = DeserializeServiceConfigs(e.ServiceConfigsJson)
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .ToDictionary(kv => kv.Key, kv => kv.Value)
    };

    // === ProjectCaseCategory ===
    public static CaseCategoryDto ToDto(this CaseCategoryEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        ProjectId = e.ProjectId,
        ParentId = e.ParentId,
        Order = e.Order,
        Childrens = System.Array.Empty<CaseCategoryDto>()
    };

    public static void Apply(this CaseCategoryDto dto, CaseCategoryEntity e)
    {
        e.Name = dto.Name;
        e.ProjectId = dto.ProjectId;
        e.ParentId = dto.ParentId;
        e.Order = dto.Order;
    }

    // === ProjectCase ===
    public static CaseDto ToDto(this CaseEntity e) => new()
    {
        Id = e.Id,
        Name = e.CaseName,
        Description = e.Description ?? string.Empty,
        ProjectId = e.ProjectId,
        CategoryId = e.CategoryId,
        IsTemplate = e.IsTemplate,
        TemplateId = e.TemplateId,
        Level = (CaseLevel)e.Level,
        Order = e.Order,
        Status = (CaseStatus)e.Status,
        LastExecutionResult = e.LastExecutionResult.HasValue ? (ExecutionStatus)e.LastExecutionResult.Value : null,
        LastExecutionTime = e.LastExecutionTime,
        DatasetIds = DeserializeList<long>(e.DatasetIdsJson)
    };

    public static void Apply(this CaseDto dto, CaseEntity e)
    {
        e.CaseName = dto.Name;
        e.Description = dto.Description;
        e.ProjectId = dto.ProjectId;
        e.CategoryId = dto.CategoryId;
        e.IsTemplate = dto.IsTemplate;
        e.TemplateId = dto.TemplateId;
        e.Level = (int)dto.Level;
        e.Order = dto.Order;
        e.Status = (int)dto.Status;
        e.LastExecutionResult = dto.LastExecutionResult.HasValue ? (int)dto.LastExecutionResult.Value : null;
        e.LastExecutionTime = dto.LastExecutionTime;
        e.DatasetIdsJson = Serialize(dto.DatasetIds);
    }

    // === CaseStep ===
    public static CaseStepDto ToDto(this CaseStepEntity e) => new()
    {
        Id = e.Id.ToString(),
        Name = e.Name,
        Type = (StepTypeEnum)e.Type,
        Parameters = DeserializeDict<object>(e.ParametersJson),
        ExpectedResult = e.ExpectedResult ?? string.Empty,
        Order = e.Order,
        IsEnabled = e.IsEnabled,
        ApiConfig = DeserializeObj<ApiStepConfig>(e.ApiConfigJson),
        UiConfig = DeserializeObj<UiStepConfig>(e.UiConfigJson),
        ScriptConfig = DeserializeObj<ScriptStepConfig>(e.ScriptConfigJson)
    };

    public static CaseStepEntity ToEntity(this CaseStepDto dto, long caseId) => new()
    {
        CaseId = caseId,
        Name = dto.Name,
        Type = (int)dto.Type,
        ParametersJson = Serialize(dto.Parameters),
        ExpectedResult = dto.ExpectedResult,
        Order = dto.Order,
        IsEnabled = dto.IsEnabled,
        ApiConfigJson = Serialize(dto.ApiConfig),
        UiConfigJson = Serialize(dto.UiConfig),
        ScriptConfigJson = Serialize(dto.ScriptConfig)
    };

    public static void Apply(this CaseStepDto dto, CaseStepEntity e)
    {
        e.Name = dto.Name;
        e.Type = (int)dto.Type;
        e.ParametersJson = Serialize(dto.Parameters);
        e.ExpectedResult = dto.ExpectedResult;
        e.Order = dto.Order;
        e.IsEnabled = dto.IsEnabled;
        e.ApiConfigJson = Serialize(dto.ApiConfig);
        e.UiConfigJson = Serialize(dto.UiConfig);
        e.ScriptConfigJson = Serialize(dto.ScriptConfig);
    }

    // === ExecutionRecord ===
    public static CaseExecutionRecordDto ToDto(this CaseRecordEntity e) => new()
    {
        Id = e.Id,
        CaseId = e.CaseId,
        CaseName = e.CaseName ?? string.Empty,
        ProjectId = e.ProjectId,
        EnvId = e.EnvId,
        EnvName = e.EnvName ?? string.Empty,
        Status = (ExecutionStatus)e.Status,
        StartTime = e.StartTime,
        EndTime = e.EndTime,
        Duration = e.Duration,
        TotalSteps = e.TotalSteps,
        SuccessSteps = e.SuccessSteps,
        FailedSteps = e.FailedSteps,
        SkippedSteps = e.SkippedSteps,
        StepRecords = DeserializeList<StepExecutionRecord>(e.StepRecordsJson),
        ErrorMessage = e.ErrorMessage,
        ExecutedBy = e.ExecutedBy ?? "System",
        DataTag = e.DataTag,
        EnvSnapshot = DeserializeDict<object>(e.EnvSnapshotJson)
    };

    public static void Apply(this CaseExecutionRecordDto dto, CaseRecordEntity e)
    {
        e.CaseId = dto.CaseId;
        e.CaseName = dto.CaseName;
        e.ProjectId = dto.ProjectId;
        e.EnvId = dto.EnvId;
        e.EnvName = dto.EnvName;
        e.Status = (int)dto.Status;
        e.StartTime = dto.StartTime;
        e.EndTime = dto.EndTime;
        e.Duration = dto.Duration;
        e.TotalSteps = dto.TotalSteps;
        e.SuccessSteps = dto.SuccessSteps;
        e.FailedSteps = dto.FailedSteps;
        e.SkippedSteps = dto.SkippedSteps;
        e.StepRecordsJson = Serialize(dto.StepRecords);
        e.ErrorMessage = dto.ErrorMessage;
        e.ExecutedBy = dto.ExecutedBy;
        e.DataTag = dto.DataTag;
        e.EnvSnapshotJson = Serialize(dto.EnvSnapshot);
    }
}
