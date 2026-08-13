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
        EnvironmentServiceConfigs = new List<ProjectEnvConfigDto>(),
        Variables = DeserializeDict<string>(e.VariablesJson),
        Headers = DeserializeDict<string>(e.HeadersJson),
        DatabaseConfig = DeserializeObj<DatabaseConfig>(e.DatabaseConfigJson),
        Status = (EnvironmentStatus)e.Status
    };

    public static void Apply(this ProjectEnvDto dto, ProjectEnvEntity e)
    {
        e.Name = dto.Name;
        e.Description = dto.Description;
        e.ProjectId = dto.ProjectId;
        e.Type = (int)dto.Type;
        e.VariablesJson = Serialize(dto.Variables);
        e.HeadersJson = Serialize(dto.Headers);
        e.DatabaseConfigJson = Serialize(dto.DatabaseConfig);
        e.Status = (int)dto.Status;
    }

    // === EnvironmentServiceConfig ===
    public static ProjectEnvConfigDto ToDto(this ProjectEnvConfigEntity e) => new()
    {
        Id = e.Id,
        EnvironmentId = e.EnvId,
        ProjectServiceId = e.ProjectServiceId,
        BaseUrl = e.BaseUrl ?? string.Empty
    };

    public static void Apply(this ProjectEnvConfigDto dto, ProjectEnvConfigEntity e)
    {
        e.EnvId = dto.EnvironmentId;
        e.ProjectServiceId = dto.ProjectServiceId;
        e.BaseUrl = dto.BaseUrl;
    }

    // === Environment（执行时视图，由环境 + 服务配置组合而成） ===
    public static EnvironmentDto ToEnvironmentDto(this ProjectEnvEntity e, IEnumerable<ProjectEnvConfigEntity> configs) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description ?? string.Empty,
        ProjectId = e.ProjectId,
        Config = DeserializeDict<string>(e.VariablesJson)
            .ToDictionary(kv => kv.Key, kv => (object)(kv.Value ?? string.Empty)),
        ServiceEndpoints = configs
            .Where(c => c.EnvId == e.Id && !string.IsNullOrWhiteSpace(c.BaseUrl))
            .ToDictionary(c => c.ProjectServiceId, c => c.BaseUrl!),
        Status = (EnvironmentStatus)e.Status
    };

    // === ProjectCaseCategory ===
    public static ProjectCaseCategory ToDto(this CaseCategoryEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description ?? string.Empty,
        ProjectId = e.ProjectId,
        ParentId = e.ParentId,
        Order = e.Order,
        Childrens = System.Array.Empty<ProjectCaseCategory>()
    };

    public static void Apply(this ProjectCaseCategory dto, CaseCategoryEntity e)
    {
        e.Name = dto.Name;
        e.Description = dto.Description;
        e.ProjectId = dto.ProjectId;
        e.ParentId = dto.ParentId;
        e.Order = dto.Order;
    }

    // === ProjectCase ===
    public static CaseDto ToDto(this CaseEntity e, List<CaseStepDto>? steps = null) => new()
    {
        Id = e.Id,
        CaseNumber = e.CaseNumber ?? string.Empty,
        Name = e.CaseName,
        Description = e.Description ?? string.Empty,
        ProjectId = e.ProjectId,
        CategoryId = e.CategoryId,
        IsTemplate = e.IsTemplate,
        TemplateId = e.TemplateId,
        Levels = DeserializeList<string>(e.LevelsJson),
        Steps = steps ?? new List<CaseStepDto>(),
        TestData = DeserializeDict<object>(e.TestDataJson),
        Tags = DeserializeList<string>(e.TagsJson),
        Order = e.Order,
        Status = (CaseStatus)e.Status,
        LastExecutionResult = e.LastExecutionResult.HasValue ? (ExecutionStatus)e.LastExecutionResult.Value : null,
        LastExecutionTime = e.LastExecutionTime
    };

    public static void Apply(this CaseDto dto, CaseEntity e)
    {
        e.CaseNumber = dto.CaseNumber;
        e.CaseName = dto.Name;
        e.Description = dto.Description;
        e.ProjectId = dto.ProjectId;
        e.CategoryId = dto.CategoryId;
        e.IsTemplate = dto.IsTemplate;
        e.TemplateId = dto.TemplateId;
        e.LevelsJson = Serialize(dto.Levels);
        e.TestDataJson = Serialize(dto.TestData);
        e.TagsJson = Serialize(dto.Tags);
        e.Order = dto.Order;
        e.Status = (int)dto.Status;
        e.LastExecutionResult = dto.LastExecutionResult.HasValue ? (int)dto.LastExecutionResult.Value : null;
        e.LastExecutionTime = dto.LastExecutionTime;
    }

    // === CaseStep ===
    public static CaseStepDto ToDto(this CaseStepEntity e) => new()
    {
        Id = e.StepKey,
        Name = e.Name,
        Type = (StepType)e.Type,
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
        StepKey = string.IsNullOrWhiteSpace(dto.Id) ? Guid.NewGuid().ToString() : dto.Id,
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

    // === ExecutionRecord ===
    public static CaseExecutionRecordDto ToDto(this CaseRecordEntity e) => new()
    {
        Id = e.Id,
        TestCaseId = e.TestCaseId,
        TestCaseName = e.CaseName ?? string.Empty,
        ProjectId = e.ProjectId,
        EnvironmentId = e.EnvironmentId,
        EnvironmentName = e.EnvironmentName ?? string.Empty,
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
        EnvironmentSnapshot = DeserializeDict<object>(e.EnvironmentSnapshotJson)
    };

    public static void Apply(this CaseExecutionRecordDto dto, CaseRecordEntity e)
    {
        e.TestCaseId = dto.TestCaseId;
        e.CaseName = dto.TestCaseName;
        e.ProjectId = dto.ProjectId;
        e.EnvironmentId = dto.EnvironmentId;
        e.EnvironmentName = dto.EnvironmentName;
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
        e.EnvironmentSnapshotJson = Serialize(dto.EnvironmentSnapshot);
    }
}
