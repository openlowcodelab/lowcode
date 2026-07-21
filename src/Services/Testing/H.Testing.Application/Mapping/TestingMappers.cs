using System.Text.Encodings.Web;
using System.Text.Json;
using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;

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
    public static ProjectDto ToDto(this TestingProject e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description ?? string.Empty,
        Status = (ProjectStatus)e.Status,
        EnvironmentIds = DeserializeList<long>(e.EnvironmentIdsJson),
        Metadata = DeserializeDict<object>(e.MetadataJson),
        CreatedAt = e.CreationTime,
        UpdatedAt = e.LastModificationTime ?? e.CreationTime,
        CreatedBy = e.CreatedBy ?? "System",
        UpdatedBy = e.UpdatedBy ?? "System"
    };

    public static void Apply(this ProjectDto dto, TestingProject e)
    {
        e.Name = dto.Name;
        e.Description = dto.Description;
        e.Status = (int)dto.Status;
        e.EnvironmentIdsJson = Serialize(dto.EnvironmentIds);
        e.MetadataJson = Serialize(dto.Metadata);
        e.CreatedBy = dto.CreatedBy;
        e.UpdatedBy = dto.UpdatedBy;
    }

    // === ProjectService ===
    public static ProjectServiceDto ToDto(this TestingProjectService e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description ?? string.Empty,
        ProjectId = e.ProjectId,
        CreatedAt = e.CreationTime,
        UpdatedAt = e.LastModificationTime ?? e.CreationTime,
        CreatedBy = e.CreatedBy ?? "System",
        UpdatedBy = e.UpdatedBy ?? "System"
    };

    public static void Apply(this ProjectServiceDto dto, TestingProjectService e)
    {
        e.Name = dto.Name;
        e.Description = dto.Description;
        e.ProjectId = dto.ProjectId;
        e.CreatedBy = dto.CreatedBy;
        e.UpdatedBy = dto.UpdatedBy;
    }

    // === ProjectEnvironment ===
    public static ProjectEnvironmentDto ToDto(this TestingProjectEnvironment e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description ?? string.Empty,
        ProjectId = e.ProjectId,
        Type = (EnvironmentType)e.Type,
        EnvironmentServiceConfigs = new List<EnvironmentServiceConfigDto>(),
        Variables = DeserializeDict<string>(e.VariablesJson),
        Headers = DeserializeDict<string>(e.HeadersJson),
        DatabaseConfig = DeserializeObj<DatabaseConfig>(e.DatabaseConfigJson),
        Status = (EnvironmentStatus)e.Status,
        CreatedAt = e.CreationTime,
        UpdatedAt = e.LastModificationTime ?? e.CreationTime,
        CreatedBy = e.CreatedBy ?? "System",
        UpdatedBy = e.UpdatedBy ?? "System"
    };

    public static void Apply(this ProjectEnvironmentDto dto, TestingProjectEnvironment e)
    {
        e.Name = dto.Name;
        e.Description = dto.Description;
        e.ProjectId = dto.ProjectId;
        e.Type = (int)dto.Type;
        e.VariablesJson = Serialize(dto.Variables);
        e.HeadersJson = Serialize(dto.Headers);
        e.DatabaseConfigJson = Serialize(dto.DatabaseConfig);
        e.Status = (int)dto.Status;
        e.CreatedBy = dto.CreatedBy;
        e.UpdatedBy = dto.UpdatedBy;
    }

    // === EnvironmentServiceConfig ===
    public static EnvironmentServiceConfigDto ToDto(this TestingEnvironmentServiceConfig e) => new()
    {
        Id = e.Id,
        EnvironmentId = e.EnvironmentId,
        ProjectServiceId = e.ProjectServiceId,
        BaseUrl = e.BaseUrl ?? string.Empty,
        CreatedAt = e.CreationTime,
        UpdatedAt = e.LastModificationTime ?? e.CreationTime,
        CreatedBy = e.CreatedBy ?? string.Empty
    };

    public static void Apply(this EnvironmentServiceConfigDto dto, TestingEnvironmentServiceConfig e)
    {
        e.EnvironmentId = dto.EnvironmentId;
        e.ProjectServiceId = dto.ProjectServiceId;
        e.BaseUrl = dto.BaseUrl;
        e.CreatedBy = dto.CreatedBy;
    }

    // === ProjectCaseCategory ===
    public static ProjectCaseCategory ToDto(this TestingProjectCaseCategory e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description ?? string.Empty,
        ProjectId = e.ProjectId,
        ParentId = e.ParentId,
        Order = e.Order,
        CreatedAt = e.CreationTime,
        CreatedBy = e.CreatedBy ?? string.Empty,
        Childrens = System.Array.Empty<ProjectCaseCategory>()
    };

    public static void Apply(this ProjectCaseCategory dto, TestingProjectCaseCategory e)
    {
        e.Name = dto.Name;
        e.Description = dto.Description;
        e.ProjectId = dto.ProjectId;
        e.ParentId = dto.ParentId;
        e.Order = dto.Order;
        e.CreatedBy = dto.CreatedBy;
    }

    // === ProjectCase ===
    public static ProjectCaseDto ToDto(this TestingProjectCase e) => new()
    {
        Id = e.Id,
        CaseNumber = e.CaseNumber ?? string.Empty,
        Name = e.Name,
        Description = e.Description ?? string.Empty,
        ProjectId = e.ProjectId,
        CategoryId = e.CategoryId,
        IsTemplate = e.IsTemplate,
        TemplateId = e.TemplateId,
        Levels = DeserializeList<string>(e.LevelsJson),
        Steps = DeserializeList<ProjectCaseStep>(e.StepsJson),
        TestData = DeserializeDict<object>(e.TestDataJson),
        Tags = DeserializeList<string>(e.TagsJson),
        Order = e.Order,
        Status = (ProjectCaseStatus)e.Status,
        CreatedAt = e.CreationTime,
        UpdatedAt = e.LastModificationTime ?? e.CreationTime,
        CreatedBy = e.CreatedBy ?? "System",
        UpdatedBy = e.UpdatedBy ?? "System",
        LastExecutionResult = e.LastExecutionResult.HasValue ? (ExecutionStatus)e.LastExecutionResult.Value : null,
        LastExecutionTime = e.LastExecutionTime
    };

    public static void Apply(this ProjectCaseDto dto, TestingProjectCase e)
    {
        e.CaseNumber = dto.CaseNumber;
        e.Name = dto.Name;
        e.Description = dto.Description;
        e.ProjectId = dto.ProjectId;
        e.CategoryId = dto.CategoryId;
        e.IsTemplate = dto.IsTemplate;
        e.TemplateId = dto.TemplateId;
        e.LevelsJson = Serialize(dto.Levels);
        e.StepsJson = Serialize(dto.Steps);
        e.TestDataJson = Serialize(dto.TestData);
        e.TagsJson = Serialize(dto.Tags);
        e.Order = dto.Order;
        e.Status = (int)dto.Status;
        e.LastExecutionResult = dto.LastExecutionResult.HasValue ? (int)dto.LastExecutionResult.Value : null;
        e.LastExecutionTime = dto.LastExecutionTime;
        e.CreatedBy = dto.CreatedBy;
        e.UpdatedBy = dto.UpdatedBy;
    }

    // === ExecutionRecord ===
    public static ExecutionRecordDto ToDto(this TestingExecutionRecord e) => new()
    {
        Id = e.Id,
        TestCaseId = e.TestCaseId,
        TestCaseName = e.TestCaseName ?? string.Empty,
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

    public static void Apply(this ExecutionRecordDto dto, TestingExecutionRecord e)
    {
        e.TestCaseId = dto.TestCaseId;
        e.TestCaseName = dto.TestCaseName;
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
