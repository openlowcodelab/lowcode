namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试项目模板
/// </summary>
public class ProjectTemplateDto
{
    /// <summary>模板ID（模板目录名）</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>模板名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>模板描述</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>服务数量</summary>
    public int ServiceCount { get; set; }

    /// <summary>环境数量</summary>
    public int EnvironmentCount { get; set; }

    /// <summary>用例分类数量</summary>
    public int CategoryCount { get; set; }

    /// <summary>用例数量</summary>
    public int CaseCount { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? ModificationTime { get; set; }
}
