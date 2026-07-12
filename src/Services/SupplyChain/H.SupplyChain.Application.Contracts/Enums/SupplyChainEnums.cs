namespace H.SupplyChain.Application.Contracts;

/// <summary>
/// 接口类型：菜单、商品详情、下单、自定义
/// </summary>
public enum InterfaceTypeEnum
{
    /// <summary>菜单接口</summary>
    Menu = 0,

    /// <summary>商品详情接口</summary>
    ProductDetail = 1,

    /// <summary>下单接口</summary>
    PlaceOrder = 2,

    /// <summary>自定义接口</summary>
    Custom = 99
}

/// <summary>
/// 商品状态：上架、下架
/// </summary>
public enum ProductStatusEnum
{
    /// <summary>下架</summary>
    OffShelf = 0,

    /// <summary>上架</summary>
    OnShelf = 1
}

/// <summary>
/// 供应商对接协议
/// </summary>
public enum SupplierProtocolEnum
{
    /// <summary>HTTP 协议</summary>
    Http = 0,

    /// <summary>模拟协议（不实际调用外部）</summary>
    Mock = 1
}

/// <summary>
/// 供应商认证方式
/// </summary>
public enum AuthTypeEnum
{
    /// <summary>无需认证</summary>
    None = 0,

    /// <summary>ApiKey（query 参数或头）</summary>
    ApiKey = 1,

    /// <summary>自定义请求头</summary>
    Header = 2,

    /// <summary>Basic 认证</summary>
    Basic = 3,

    /// <summary>Bearer Token</summary>
    Bearer = 4
}

/// <summary>
/// 下单结果状态
/// </summary>
public enum OrderPlaceStatusEnum
{
    /// <summary>失败</summary>
    Failed = 0,

    /// <summary>成功</summary>
    Success = 1
}