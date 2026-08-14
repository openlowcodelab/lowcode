namespace H.Testing.Application.Contracts;

/// <summary>
/// AI 生成输入（口语化描述）
/// </summary>
public class AiGenerateInputDto
{
    /// <summary>
    /// 用户的口语化需求描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// AI 生成的测试项目（含分类与用例，未落库的草稿）
/// </summary>
public class AiGeneratedProjectDto
{
    /// <summary>项目名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>项目描述</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>分类集合（平铺，通过 TempId/ParentTempId 表达层级）</summary>
    public List<AiGeneratedCategoryDto> Categories { get; set; } = new();

    /// <summary>被测服务集合</summary>
    public List<AiGeneratedServiceDto> Services { get; set; } = new();

    /// <summary>测试环境集合</summary>
    public List<AiGeneratedEnvironmentDto> Environments { get; set; } = new();
}

/// <summary>
/// AI 生成的被测服务
/// </summary>
public class AiGeneratedServiceDto
{
    /// <summary>临时ID（如 s1、s2，用于环境配置与步骤引用）</summary>
    public string TempId { get; set; } = string.Empty;

    /// <summary>服务名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>服务描述</summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// AI 生成的测试环境
/// </summary>
public class AiGeneratedEnvironmentDto
{
    /// <summary>环境名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>环境类型：development / testing / staging / production</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>环境描述</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>环境变量</summary>
    public Dictionary<string, string> Variables { get; set; } = new();

    /// <summary>各服务在该环境下的地址配置</summary>
    public List<AiGeneratedServiceConfigDto> ServiceConfigs { get; set; } = new();
}

/// <summary>
/// AI 生成的环境服务配置（服务在某环境下的基础地址）
/// </summary>
public class AiGeneratedServiceConfigDto
{
    /// <summary>引用的服务临时ID（如 s1）</summary>
    public string ServiceTempId { get; set; } = string.Empty;

    /// <summary>服务基础地址（如 http://localhost:8080）</summary>
    public string BaseUrl { get; set; } = string.Empty;
}

/// <summary>
/// AI 生成的测试步骤
/// </summary>
public class AiGeneratedStepDto
{
    /// <summary>步骤名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>步骤类型：api（接口请求）/ ui（界面操作）</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>预期结果描述</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>API：HTTP 方法（GET/POST/PUT/DELETE）</summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>API：请求路径（如 /api/login）</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>API：所属服务引用（服务 tempId，如 s1）</summary>
    public string ServiceRef { get; set; } = string.Empty;

    /// <summary>API：请求体（JSON 字符串）</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>UI：操作类型（navigate/click/input/select/wait/assert）</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>UI：元素选择器</summary>
    public string Selector { get; set; } = string.Empty;

    /// <summary>UI：输入值或期望值</summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// AI 生成的分类
/// </summary>
public class AiGeneratedCategoryDto
{
    /// <summary>临时ID（如 c1、c2，用于表达父子关系）</summary>
    public string TempId { get; set; } = string.Empty;

    /// <summary>分类名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>父分类临时ID（根分类为 null）</summary>
    public string? ParentTempId { get; set; }

    /// <summary>该分类下的用例</summary>
    public List<AiGeneratedCaseDto> Cases { get; set; } = new();
}

/// <summary>
/// AI 生成的用例
/// </summary>
public class AiGeneratedCaseDto
{
    /// <summary>用例名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>用例描述（测试要点与预期结果）</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>用例级别（单选，P0~P3）</summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>测试步骤</summary>
    public List<AiGeneratedStepDto> Steps { get; set; } = new();
}

/// <summary>
/// AI 变更计划（针对已有项目的增量操作）
/// </summary>
public class AiModificationPlanDto
{
    /// <summary>新增分类</summary>
    public List<AiAddCategoryDto> AddCategories { get; set; } = new();

    /// <summary>修改分类</summary>
    public List<AiUpdateCategoryDto> UpdateCategories { get; set; } = new();

    /// <summary>新增用例</summary>
    public List<AiAddCaseDto> AddCases { get; set; } = new();

    /// <summary>修改用例</summary>
    public List<AiUpdateCaseDto> UpdateCases { get; set; } = new();
}

/// <summary>
/// AI 新增分类
/// </summary>
public class AiAddCategoryDto
{
    /// <summary>临时ID（如 c1、c2）</summary>
    public string TempId { get; set; } = string.Empty;

    /// <summary>分类名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>父分类引用：已有分类ID（数字）或本次新增分类的 TempId，根分类为 null</summary>
    public string? ParentRef { get; set; }
}

/// <summary>
/// AI 修改分类（空值字段表示不修改）
/// </summary>
public class AiUpdateCategoryDto
{
    /// <summary>已有分类ID</summary>
    public long Id { get; set; }

    /// <summary>新名称（为空表示不修改）</summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// AI 新增用例
/// </summary>
public class AiAddCaseDto
{
    /// <summary>用例名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>用例描述（测试要点与预期结果）</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>用例级别（单选，P0~P3）</summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>所属分类引用：已有分类ID（数字）或本次新增分类的 TempId，未分类为 null</summary>
    public string? CategoryRef { get; set; }
}

/// <summary>
/// AI 修改用例（空值字段表示不修改）
/// </summary>
public class AiUpdateCaseDto
{
    /// <summary>已有用例ID</summary>
    public long Id { get; set; }

    /// <summary>新名称（为空表示不修改）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>新描述（为空表示不修改）</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>新级别（单选，P0~P3，为空字符串表示不修改）</summary>
    public string Level { get; set; } = string.Empty;
}
