using H.SupplyChain.Application.Contracts;
using H.SupplyChain.EntityFrameworkCore;

namespace H.SupplyChain.Application.Mapping;

/// <summary>
/// 实体 <-> DTO 手工映射（与 Order 服务保持一致的轻量映射风格，
/// 避免依赖 ABP 的 AutoObjectMappingProvider）。
/// </summary>
public static class SupplyChainMappers
{
    // === Supplier ===
    public static SupplierDto ToDto(this SupplierEntity e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name,
        DisplayName = e.DisplayName,
        ApiUrl = e.ApiUrl,
        AuthType = (AuthTypeEnum)e.AuthType,
        AuthConfig = e.AuthConfig,
        Protocol = (SupplierProtocolEnum)e.Protocol,
        ProtocolConfig = e.ProtocolConfig,
        IsEnabled = e.IsEnabled,
        Remark = e.Remark,
        CreationTime = e.CreationTime,
        CreatorId = e.CreatorId,
        LastModificationTime = e.LastModificationTime,
        LastModifierId = e.LastModifierId
    };

    public static SupplierEntity ToEntity(this CreateSupplierDto input) => new()
    {
        Code = input.Code,
        Name = input.Name,
        DisplayName = input.DisplayName,
        ApiUrl = input.ApiUrl,
        AuthType = (int)input.AuthType,
        AuthConfig = input.AuthConfig,
        Protocol = (int)input.Protocol,
        ProtocolConfig = input.ProtocolConfig,
        IsEnabled = input.IsEnabled,
        Remark = input.Remark
    };

    public static void Apply(this UpdateSupplierDto input, SupplierEntity entity)
    {
        entity.Name = input.Name;
        entity.DisplayName = input.DisplayName;
        entity.ApiUrl = input.ApiUrl;
        entity.AuthType = (int)input.AuthType;
        entity.AuthConfig = input.AuthConfig;
        entity.Protocol = (int)input.Protocol;
        entity.ProtocolConfig = input.ProtocolConfig;
        entity.IsEnabled = input.IsEnabled;
        entity.Remark = input.Remark;
    }

    // === Product ===
    public static ProductDto ToDto(this ProductEntity e) => new()
    {
        Id = e.Id,
        ProductCode = e.ProductCode,
        Name = e.Name,
        Category = e.Category,
        Description = e.Description,
        Status = (ProductStatusEnum)e.Status,
        Remark = e.Remark,
        CreationTime = e.CreationTime,
        CreatorId = e.CreatorId,
        LastModificationTime = e.LastModificationTime,
        LastModifierId = e.LastModifierId
    };

    public static ProductDetailDto ToDetailDto(this ProductEntity e, List<ProductSkuDto> skus) => new()
    {
        Id = e.Id,
        ProductCode = e.ProductCode,
        Name = e.Name,
        Category = e.Category,
        Description = e.Description,
        Status = (ProductStatusEnum)e.Status,
        Remark = e.Remark,
        CreationTime = e.CreationTime,
        CreatorId = e.CreatorId,
        LastModificationTime = e.LastModificationTime,
        LastModifierId = e.LastModifierId,
        Skus = skus
    };

    public static ProductEntity ToEntity(this CreateProductDto input) => new()
    {
        ProductCode = input.ProductCode,
        Name = input.Name,
        Category = input.Category,
        Description = input.Description,
        Status = (int)input.Status,
        Remark = input.Remark
    };

    public static void Apply(this UpdateProductDto input, ProductEntity entity)
    {
        entity.Name = input.Name;
        entity.Category = input.Category;
        entity.Description = input.Description;
        entity.Status = (int)input.Status;
        entity.Remark = input.Remark;
    }

    // === ProductSku ===
    public static ProductSkuDto ToDto(this ProductSkuEntity e) => new()
    {
        Id = e.Id,
        ProductId = e.ProductId,
        SkuCode = e.SkuCode,
        SkuName = e.SkuName,
        SpecsJson = e.SpecsJson,
        Price = e.Price,
        Stock = e.Stock,
        IsEnabled = e.IsEnabled,
        Remark = e.Remark,
        CreationTime = e.CreationTime,
        CreatorId = e.CreatorId,
        LastModificationTime = e.LastModificationTime,
        LastModifierId = e.LastModifierId
    };

    public static ProductSkuEntity ToEntity(this CreateProductSkuDto input) => new()
    {
        ProductId = input.ProductId,
        SkuCode = input.SkuCode,
        SkuName = input.SkuName,
        SpecsJson = input.SpecsJson,
        Price = input.Price,
        Stock = input.Stock,
        IsEnabled = input.IsEnabled,
        Remark = input.Remark
    };

    public static void Apply(this UpdateProductSkuDto input, ProductSkuEntity entity)
    {
        entity.SkuName = input.SkuName;
        entity.SpecsJson = input.SpecsJson;
        entity.Price = input.Price;
        entity.Stock = input.Stock;
        entity.IsEnabled = input.IsEnabled;
        entity.Remark = input.Remark;
    }

    // === SupplierSkuMapping ===
    public static SupplierSkuMappingDto ToDto(
        this SupplierSkuMappingEntity e, string skuCode, string supplierCode) => new()
    {
        Id = e.Id,
        SkuId = e.SkuId,
        SkuCode = skuCode,
        SupplierId = e.SupplierId,
        SupplierCode = supplierCode,
        SupplierSkuCode = e.SupplierSkuCode,
        SupplierSkuName = e.SupplierSkuName,
        SupplierPrice = e.SupplierPrice,
        IsEnabled = e.IsEnabled,
        Remark = e.Remark,
        CreationTime = e.CreationTime,
        CreatorId = e.CreatorId,
        LastModificationTime = e.LastModificationTime,
        LastModifierId = e.LastModifierId
    };

    public static SupplierSkuMappingEntity ToEntity(this CreateSupplierSkuMappingDto input) => new()
    {
        SkuId = input.SkuId,
        SupplierId = input.SupplierId,
        SupplierSkuCode = input.SupplierSkuCode,
        SupplierSkuName = input.SupplierSkuName,
        SupplierPrice = input.SupplierPrice,
        IsEnabled = input.IsEnabled,
        Remark = input.Remark
    };

    public static void Apply(this UpdateSupplierSkuMappingDto input, SupplierSkuMappingEntity entity)
    {
        entity.SupplierSkuCode = input.SupplierSkuCode;
        entity.SupplierSkuName = input.SupplierSkuName;
        entity.SupplierPrice = input.SupplierPrice;
        entity.IsEnabled = input.IsEnabled;
        entity.Remark = input.Remark;
    }

    // === ApiInterface ===
    public static ApiInterfaceDto ToDto(this ApiInterfaceEntity e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name,
        InterfaceType = (InterfaceTypeEnum)e.InterfaceType,
        HttpMethod = e.HttpMethod,
        Path = e.Path,
        Description = e.Description,
        IsEnabled = e.IsEnabled,
        Remark = e.Remark,
        CreationTime = e.CreationTime,
        CreatorId = e.CreatorId,
        LastModificationTime = e.LastModificationTime,
        LastModifierId = e.LastModifierId
    };

    public static ApiInterfaceEntity ToEntity(this CreateApiInterfaceDto input) => new()
    {
        Code = input.Code,
        Name = input.Name,
        InterfaceType = (int)input.InterfaceType,
        HttpMethod = input.HttpMethod,
        Path = input.Path,
        Description = input.Description,
        IsEnabled = input.IsEnabled,
        Remark = input.Remark
    };

    public static void Apply(this UpdateApiInterfaceDto input, ApiInterfaceEntity entity)
    {
        entity.Name = input.Name;
        entity.InterfaceType = (int)input.InterfaceType;
        entity.HttpMethod = input.HttpMethod;
        entity.Path = input.Path;
        entity.Description = input.Description;
        entity.IsEnabled = input.IsEnabled;
        entity.Remark = input.Remark;
    }

    // === SupplierInterfaceMapping ===
    public static SupplierInterfaceMappingDto ToDto(
        this SupplierInterfaceMappingEntity e, string supplierCode, string interfaceCode) => new()
    {
        Id = e.Id,
        SupplierId = e.SupplierId,
        SupplierCode = supplierCode,
        InterfaceId = e.InterfaceId,
        InterfaceCode = interfaceCode,
        SupplierApiUrl = e.SupplierApiUrl,
        RequestMappingJson = e.RequestMappingJson,
        ResponseMappingJson = e.ResponseMappingJson,
        IsEnabled = e.IsEnabled,
        Remark = e.Remark,
        CreationTime = e.CreationTime,
        CreatorId = e.CreatorId,
        LastModificationTime = e.LastModificationTime,
        LastModifierId = e.LastModifierId
    };

    public static SupplierInterfaceMappingEntity ToEntity(this CreateSupplierInterfaceMappingDto input) => new()
    {
        SupplierId = input.SupplierId,
        InterfaceId = input.InterfaceId,
        SupplierApiUrl = input.SupplierApiUrl,
        RequestMappingJson = input.RequestMappingJson,
        ResponseMappingJson = input.ResponseMappingJson,
        IsEnabled = input.IsEnabled,
        Remark = input.Remark
    };

    public static void Apply(this UpdateSupplierInterfaceMappingDto input, SupplierInterfaceMappingEntity entity)
    {
        entity.SupplierApiUrl = input.SupplierApiUrl;
        entity.RequestMappingJson = input.RequestMappingJson;
        entity.ResponseMappingJson = input.ResponseMappingJson;
        entity.IsEnabled = input.IsEnabled;
        entity.Remark = input.Remark;
    }
}