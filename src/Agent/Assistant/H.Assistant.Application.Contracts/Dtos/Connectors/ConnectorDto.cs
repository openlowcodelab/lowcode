using H.Abp.Application.Contracts;
using System.ComponentModel.DataAnnotations;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 连接器 DTO
/// </summary>
public class ConnectorDto : AuditedEntityDto<Guid>
{
    public string ConnectorKey { get; set; } = string.Empty;
    public string ConnectorName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>来源：Market(连接器市场)/Custom(自定义)</summary>
    public string Source { get; set; } = "Custom";

    public string Icon { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string? Config { get; set; }

    /// <summary>是否来自连接器市场</summary>
    public bool IsMarket => Source == "Market";
}

public class CreateConnectorDto
{
    [Required(ErrorMessage = "连接器名称不能为空")]
    [StringLength(200, ErrorMessage = "连接器名称不能超过200个字符")]
    public string ConnectorName { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "连接器描述不能超过1000个字符")]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string Icon { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Config { get; set; }
}

public class UpdateConnectorDto
{
    [Required(ErrorMessage = "连接器名称不能为空")]
    [StringLength(200, ErrorMessage = "连接器名称不能超过200个字符")]
    public string ConnectorName { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "连接器描述不能超过1000个字符")]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string Icon { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Config { get; set; }
}

public class ConnectorQueryDto : PagedResultRequestDto
{
    public string? Filter { get; set; }

    /// <summary>按来源过滤：Market/Custom</summary>
    public string? Source { get; set; }
}
