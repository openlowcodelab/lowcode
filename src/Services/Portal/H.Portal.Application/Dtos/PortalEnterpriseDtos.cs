using System.ComponentModel.DataAnnotations;

namespace H.Portal.Application;

/// <summary>
/// 门户视角的企业信息（与系统级 Enterprise 服务的输出 JSON 结构兼容，编译期不依赖 System 层）
/// </summary>
public class PortalEnterpriseDto
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
/// 创建企业请求（门户）
/// </summary>
public class PortalCreateEnterpriseDto
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
