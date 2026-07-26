using System;

namespace H.Approval.Application.Contracts;

/// <summary>
/// 审批分类(分组) DTO
/// </summary>
public class ApprovalCategoryDto
{
    /// <summary>
    /// 分类 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 分类名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 排序值(升序)
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; }
}

/// <summary>
/// 创建审批分类输入 DTO
/// </summary>
public class CreateApprovalCategoryDto
{
    /// <summary>
    /// 分类名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 重命名审批分类输入 DTO
/// </summary>
public class RenameApprovalCategoryDto
{
    /// <summary>
    /// 分类 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 新的分类名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
