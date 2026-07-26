using H.Abstractions;

namespace H.SupplyChain.Application.Contracts;

/// <summary>
/// 供应链对外 API 接口（菜单、商品详情、下单），供外部系统调用。
/// ABP 约定控制器将自动生成 RESTful 端点：
///  - GET  /api/supply-chain/supply-chain-api/menu
///  - GET  /api/supply-chain/supply-chain-api/product-detail
///  - POST /api/supply-chain/supply-chain-api/place-order
/// </summary>
public interface ISupplyChainApiAppService : IAppService
{
    /// <summary>
    /// 菜单接口：返回内部商品目录（可按供应商映射后展示供应商侧 SKU 编码）。
    /// </summary>
    Task<MenuResultDto> GetMenuAsync(MenuQueryDto input);

    /// <summary>
    /// 商品详情接口：返回商品主信息 + SKU 列表。
    /// 指定供应商时，按其接口映射调用供应商并合并返回供应商侧字段。
    /// </summary>
    Task<ProductDetailResultDto> GetProductDetailAsync(ProductDetailQueryDto input);

    /// <summary>
    /// 下单接口：按供应商 SKU 映射与下单接口映射向供应商下单。
    /// </summary>
    Task<PlaceOrderResultDto> PlaceOrderAsync(PlaceOrderDto input);
}