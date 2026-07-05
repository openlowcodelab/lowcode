using System.ComponentModel.DataAnnotations;

namespace H.Enterprise.Application.Contracts;

/// <summary>
/// 企业 DTO（输出）
/// </summary>
public class EnterpriseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DatabaseMode { get; set; } = string.Empty;
    public bool IsActivated { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Remark { get; set; }
    public int UserCount { get; set; }
}

/// <summary>
/// 创建企业 DTO
/// </summary>
public class CreateEnterpriseDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? ContactName { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    [MaxLength(100)]
    public string? ContactEmail { get; set; }
}

/// <summary>
/// 更新企业 DTO
/// </summary>
public class UpdateEnterpriseDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Logo { get; set; }

    [MaxLength(100)]
    public string? ContactName { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    [MaxLength(100)]
    public string? ContactEmail { get; set; }

    [MaxLength(500)]
    public string? Remark { get; set; }
}

/// <summary>
/// 激活企业 DTO
/// </summary>
public class ActivateEnterpriseDto
{
    /// <summary>
    /// 数据库模式: Shared / Independent
    /// </summary>
    [Required]
    public string DatabaseMode { get; set; } = "Shared";

    /// <summary>
    /// 独立数据库连接字符串（仅 Independent 模式必填）
    /// </summary>
    public string? ConnectionString { get; set; }
}

/// <summary>
/// 企业查询参数
/// </summary>
public class EnterpriseQueryParams
{
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    public string? DatabaseMode { get; set; }
    public bool? IsActivated { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// 分页结果
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
}
