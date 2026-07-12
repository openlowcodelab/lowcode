using Volo.Abp.Application.Dtos;

namespace H.SupplyChain.Application.Contracts;

/// <summary>
/// 接口定义 DTO（菜单接口、商品接口、下单接口等统一定义）
/// </summary>
public class ApiInterfaceDto : FullAuditedEntityDto<Guid>
{
    /// <summary>接口编码（唯一，如 menu / product-detail / place-order）</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>接口名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>接口类型</summary>
    public InterfaceTypeEnum InterfaceType { get; set; }

    /// <summary>HTTP 方法（GET/POST 等）</summary>
    public string HttpMethod { get; set; } = "POST";

    /// <summary>接口路径（相对于供应商 ApiUrl，可空表示使用供应商配置）</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>标准请求字段定义（JSON，<see cref="List{StandardField}"/>）</summary>
    public string? RequestFieldsJson { get; set; }

    /// <summary>标准返回字段定义（JSON，<see cref="List{StandardField}"/>）</summary>
    public string? ResponseFieldsJson { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 创建接口定义 DTO
/// </summary>
public class CreateApiInterfaceDto
{
    /// <summary>接口编码</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>接口名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>接口类型</summary>
    public InterfaceTypeEnum InterfaceType { get; set; }

    /// <summary>HTTP 方法</summary>
    public string HttpMethod { get; set; } = "POST";

    /// <summary>接口路径</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>标准请求字段定义（JSON）</summary>
    public string? RequestFieldsJson { get; set; }

    /// <summary>标准返回字段定义（JSON）</summary>
    public string? ResponseFieldsJson { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 更新接口定义 DTO
/// </summary>
public class UpdateApiInterfaceDto
{
    /// <summary>接口名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>接口类型</summary>
    public InterfaceTypeEnum InterfaceType { get; set; }

    /// <summary>HTTP 方法</summary>
    public string HttpMethod { get; set; } = "POST";

    /// <summary>接口路径</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>标准请求字段定义（JSON）</summary>
    public string? RequestFieldsJson { get; set; }

    /// <summary>标准返回字段定义（JSON）</summary>
    public string? ResponseFieldsJson { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 接口定义查询参数
/// </summary>
public class ApiInterfaceQueryDto : PagedResultRequestDto
{
    /// <summary>关键词（编码或名称）</summary>
    public string? Filter { get; set; }

    /// <summary>接口类型</summary>
    public InterfaceTypeEnum? InterfaceType { get; set; }

    /// <summary>是否启用</summary>
    public bool? IsEnabled { get; set; }
}