namespace H.SupplyChain.Application.Contracts;

/// <summary>
/// 供应商调用上下文：供应商定义 + 接口映射 + 请求输入（标准字段）
/// </summary>
public class SupplierApiContext
{
    /// <summary>供应商信息（脱敏后传给调用器，避免直接操作实体）</summary>
    public SupplierInfo Supplier { get; set; } = new();

    /// <summary>接口定义</summary>
    public ApiInterfaceInfo Interface { get; set; } = new();

    /// <summary>供应商接口映射（请求/应答字段映射）</summary>
    public SupplierInterfaceMappingInfo Mapping { get; set; } = new();

    /// <summary>标准输入载荷（已用业务字段填充）</summary>
    public Dictionary<string, object?> Input { get; set; } = new();
}

/// <summary>供应商信息（脱敏）</summary>
public class SupplierInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ApiUrl { get; set; }
    public AuthTypeEnum AuthType { get; set; }
    public string? AuthConfig { get; set; }
    public SupplierProtocolEnum Protocol { get; set; }
}

/// <summary>接口定义（脱敏）</summary>
public class ApiInterfaceInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public InterfaceTypeEnum InterfaceType { get; set; }
    public string HttpMethod { get; set; } = "POST";
    public string Path { get; set; } = string.Empty;
}

/// <summary>供应商接口映射（脱敏）</summary>
public class SupplierInterfaceMappingInfo
{
    public string? SupplierApiUrl { get; set; }
    public List<FieldMapping> RequestMappings { get; set; } = new();
    public List<FieldMapping> ResponseMappings { get; set; } = new();
}