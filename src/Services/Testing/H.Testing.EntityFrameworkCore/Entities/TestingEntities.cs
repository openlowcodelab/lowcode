using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Testing.EntityFrameworkCore;

/// <summary>
/// 测试项目
/// </summary>
public class TestingProject : AuditedEntity<long>, IMultiTenant
{
    /// <summary>租户ID（多租户）</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>项目名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>项目描述</summary>
    public string? Description { get; set; }

    /// <summary>项目状态</summary>
    public int Status { get; set; }

    /// <summary>关联环境ID集合（List&lt;long&gt; 序列化）</summary>
    public string? EnvironmentIdsJson { get; set; }

    /// <summary>元数据（Dictionary 序列化）</summary>
    public string? MetadataJson { get; set; }

    /// <summary>创建者（业务名，历史数据兼容）</summary>
    public string? CreatedBy { get; set; }

    /// <summary>更新者（业务名，历史数据兼容）</summary>
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// 项目级别服务定义
/// </summary>
public class TestingProjectService : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属项目ID</summary>
    public long ProjectId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
}

/// <summary>
/// 项目环境
/// </summary>
public class TestingProjectEnvironment : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属项目ID</summary>
    public long ProjectId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>环境类型</summary>
    public int Type { get; set; }

    /// <summary>环境状态</summary>
    public int Status { get; set; }

    /// <summary>环境变量（Dictionary 序列化）</summary>
    public string? VariablesJson { get; set; }

    /// <summary>请求头（Dictionary 序列化）</summary>
    public string? HeadersJson { get; set; }

    /// <summary>数据库配置（对象 序列化）</summary>
    public string? DatabaseConfigJson { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
}

/// <summary>
/// 环境服务配置（环境 + 服务 的 BaseUrl 绑定）
/// </summary>
public class TestingEnvironmentServiceConfig : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>环境ID</summary>
    public long EnvironmentId { get; set; }

    /// <summary>项目服务ID</summary>
    public long ProjectServiceId { get; set; }

    /// <summary>服务基础URL</summary>
    public string? BaseUrl { get; set; }

    public string? CreatedBy { get; set; }
}

/// <summary>
/// 测试用例分类（树形，自引用 ParentId）
/// </summary>
public class TestingProjectCaseCategory : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属项目ID</summary>
    public long ProjectId { get; set; }

    /// <summary>父分类ID（根为 null）</summary>
    public long? ParentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Order { get; set; }

    public string? CreatedBy { get; set; }
}

/// <summary>
/// 测试用例
/// </summary>
public class TestingProjectCase : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属项目ID</summary>
    public long ProjectId { get; set; }

    /// <summary>分类ID</summary>
    public long? CategoryId { get; set; }

    /// <summary>关联模板用例ID</summary>
    public long? TemplateId { get; set; }

    /// <summary>用例编号（业务字符串）</summary>
    public string? CaseNumber { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>是否为测试模板</summary>
    public bool IsTemplate { get; set; }

    /// <summary>用例级别（List&lt;string&gt; 序列化）</summary>
    public string? LevelsJson { get; set; }

    /// <summary>标签（List&lt;string&gt; 序列化）</summary>
    public string? TagsJson { get; set; }

    /// <summary>步骤（List&lt;ProjectCaseStep&gt; 序列化）</summary>
    public string? StepsJson { get; set; }

    /// <summary>测试数据（Dictionary 序列化）</summary>
    public string? TestDataJson { get; set; }

    /// <summary>排序</summary>
    public int Order { get; set; }

    /// <summary>用例状态</summary>
    public int Status { get; set; }

    /// <summary>上一次执行结果</summary>
    public int? LastExecutionResult { get; set; }

    /// <summary>上一次执行时间</summary>
    public DateTime? LastExecutionTime { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
}

/// <summary>
/// 测试执行记录
/// </summary>
public class TestingExecutionRecord : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属项目ID</summary>
    public long ProjectId { get; set; }

    /// <summary>测试用例ID</summary>
    public long TestCaseId { get; set; }

    /// <summary>执行环境ID</summary>
    public long EnvironmentId { get; set; }

    public string? TestCaseName { get; set; }

    public string? EnvironmentName { get; set; }

    /// <summary>执行状态</summary>
    public int Status { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    /// <summary>总耗时（毫秒）</summary>
    public long Duration { get; set; }

    public int TotalSteps { get; set; }

    public int SuccessSteps { get; set; }

    public int FailedSteps { get; set; }

    public int SkippedSteps { get; set; }

    /// <summary>步骤执行记录（List&lt;StepExecutionRecord&gt; 序列化）</summary>
    public string? StepRecordsJson { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ExecutedBy { get; set; }

    /// <summary>环境配置快照（Dictionary 序列化）</summary>
    public string? EnvironmentSnapshotJson { get; set; }
}
