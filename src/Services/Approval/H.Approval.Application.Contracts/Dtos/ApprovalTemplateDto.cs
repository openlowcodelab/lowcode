namespace H.Approval.Application.Contracts;

/// <summary>
/// 审批模板 DTO(用于"从模板创建")
/// <para>模板数据来源于内嵌 JSON 资源,不落库,仅在创建审批表单时提供预置字段与默认审批流。</para>
/// </summary>
public class ApprovalTemplateDto
{
    /// <summary>
    /// 模板唯一标识(稳定 Key)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 模板名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 模板图标(emoji)
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 所属分类名称
    /// </summary>
    public string? CategoryName { get; set; }

    /// <summary>
    /// 所属分类排序(升序)
    /// </summary>
    public int CategorySort { get; set; }

    /// <summary>
    /// 模板在分类内的排序(升序)
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 模板描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 表单设计 JSON(字段 Schema)
    /// </summary>
    public string? FormJson { get; set; }

    /// <summary>
    /// 审批流定义 JSON
    /// </summary>
    public string DefinitionJson { get; set; } = string.Empty;
}
