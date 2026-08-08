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

    /// <summary>分类描述</summary>
    public string Description { get; set; } = string.Empty;

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

    /// <summary>用例级别（P0~P3）</summary>
    public List<string> Levels { get; set; } = new();

    /// <summary>标签</summary>
    public List<string> Tags { get; set; } = new();
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

    /// <summary>分类描述</summary>
    public string Description { get; set; } = string.Empty;

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

    /// <summary>新描述（为空表示不修改）</summary>
    public string Description { get; set; } = string.Empty;
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

    /// <summary>用例级别（P0~P3）</summary>
    public List<string> Levels { get; set; } = new();

    /// <summary>标签</summary>
    public List<string> Tags { get; set; } = new();

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

    /// <summary>新级别（为空数组表示不修改）</summary>
    public List<string> Levels { get; set; } = new();

    /// <summary>新标签（为空数组表示不修改）</summary>
    public List<string> Tags { get; set; } = new();
}
