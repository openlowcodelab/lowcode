using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace H.SupplyChain.Application.Contracts;

/// <summary>
/// 供应商管理接口
/// </summary>
public interface ISupplyChainSupplierAppService
    : ICrudAppService<SupplierDto, Guid, SupplierQueryDto, CreateSupplierDto, UpdateSupplierDto>
{
}

/// <summary>
/// 商品管理接口
/// </summary>
public interface IProductAppService
    : ICrudAppService<ProductDto, Guid, ProductQueryDto, CreateProductDto, UpdateProductDto>
{
    /// <summary>获取商品详情（含 SKU 列表）</summary>
    Task<ProductDetailDto> GetDetailAsync(Guid id);
}

/// <summary>
/// 商品 SKU 管理接口
/// </summary>
public interface IProductSkuAppService
    : ICrudAppService<ProductSkuDto, Guid, ProductSkuQueryDto, CreateProductSkuDto, UpdateProductSkuDto>
{
}

/// <summary>
/// 供应商 SKU 映射管理接口（一个 SKU 可映射多个供应商）
/// </summary>
public interface ISupplierSkuMappingAppService
    : ICrudAppService<SupplierSkuMappingDto, Guid, SupplierSkuMappingQueryDto, CreateSupplierSkuMappingDto, UpdateSupplierSkuMappingDto>
{
}

/// <summary>
/// 接口定义管理接口（菜单接口、商品接口、下单接口等增删改）
/// </summary>
public interface IApiInterfaceAppService
    : ICrudAppService<ApiInterfaceDto, Guid, ApiInterfaceQueryDto, CreateApiInterfaceDto, UpdateApiInterfaceDto>
{
}

/// <summary>
/// 供应商接口映射管理接口（基于接口定义配置请求参数映射/返回值字段映射）
/// </summary>
public interface ISupplierInterfaceMappingAppService
    : ICrudAppService<SupplierInterfaceMappingDto, Guid, SupplierInterfaceMappingQueryDto, CreateSupplierInterfaceMappingDto, UpdateSupplierInterfaceMappingDto>
{
}