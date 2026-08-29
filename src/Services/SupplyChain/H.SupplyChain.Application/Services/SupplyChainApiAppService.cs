using H.SupplyChain.Application.Contracts;
using H.SupplyChain.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.SupplyChain.Application.Services;

/// <summary>
/// 供应链对外 API 应用服务。
/// 提供菜单、商品详情、下单三类接口供外部系统调用。
/// 下单/查详情时，基于「接口定义 + 供应商接口映射」驱动请求参数映射与返回值字段映射，
/// 经 <see cref="ISupplierApiInvoker"/> 完成供应商接口调用。
/// </summary>
public class SupplyChainApiAppService : ApplicationService, ISupplyChainApiAppService
{
    private readonly IRepository<ProductEntity, long> _productRepo;
    private readonly IRepository<ProductSkuEntity, long> _skuRepo;
    private readonly IRepository<SupplierEntity, string> _supplierRepo;
    private readonly IRepository<SupplierSkuMappingEntity, long> _skuMappingRepo;
    private readonly IRepository<ApiInterfaceEntity, long> _interfaceRepo;
    private readonly IRepository<SupplierInterfaceMappingEntity, long> _interfaceMappingRepo;
    private readonly ISupplierApiInvokerFactory _invokerFactory;

    // 标准接口编码常量（与 InterfaceTypeEnum 对应）
    private const string MenuInterfaceCode = "menu";
    private const string ProductDetailInterfaceCode = "product-detail";
    private const string PlaceOrderInterfaceCode = "place-order";

    public SupplyChainApiAppService(
        IRepository<ProductEntity, long> productRepo,
        IRepository<ProductSkuEntity, long> skuRepo,
        IRepository<SupplierEntity, string> supplierRepo,
        IRepository<SupplierSkuMappingEntity, long> skuMappingRepo,
        IRepository<ApiInterfaceEntity, long> interfaceRepo,
        IRepository<SupplierInterfaceMappingEntity, long> interfaceMappingRepo,
        ISupplierApiInvokerFactory invokerFactory)
    {
        _productRepo = productRepo;
        _skuRepo = skuRepo;
        _supplierRepo = supplierRepo;
        _skuMappingRepo = skuMappingRepo;
        _interfaceRepo = interfaceRepo;
        _interfaceMappingRepo = interfaceMappingRepo;
        _invokerFactory = invokerFactory;
    }

    /// <summary>
    /// 菜单接口：返回内部商品目录。
    /// 指定供应商时，菜单项 SKU 附加该供应商侧 SKU 编码，用于外部系统向该供应商下单。
    /// </summary>
    public async Task<BaseOutput<MenuResultDto>> GetMenuAsync(MenuQueryDto input)
    {
        var productQuery = await _productRepo.GetQueryableAsync();
        var products = productQuery.Where(x => x.Status == (int)ProductStatusEnum.OnShelf);

        if (!string.IsNullOrWhiteSpace(input.Category))
            products = products.Where(x => x.Category == input.Category);
        if (!string.IsNullOrWhiteSpace(input.Filter))
            products = products.Where(x => x.ProductCode.Contains(input.Filter) || x.Name.Contains(input.Filter));

        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 100;
        var productList = await AsyncExecuter.ToListAsync(
            products.OrderBy(x => x.ProductCode).Take(maxResult));

        var productIds = productList.Select(p => p.Id).ToList();
        var skuQuery = await _skuRepo.GetQueryableAsync();
        var skuList = await AsyncExecuter.ToListAsync(
            skuQuery.Where(x => productIds.Contains(x.ProductId) && x.IsEnabled).OrderBy(x => x.SkuCode));

        // 若指定供应商，加载该供应商对内部 SKU 的映射，附加供应商侧 SKU 编码
        Dictionary<long, string>? supplierSkuMap = null;
        if (!string.IsNullOrWhiteSpace(input.SupplierCode))
        {
            var supplierId = await GetSupplierIdByCodeAsync(input.SupplierCode);
            if (!string.IsNullOrEmpty(supplierId))
            {
                var skuIds = skuList.Select(s => s.Id).ToList();
                var mappingQuery = await _skuMappingRepo.GetQueryableAsync();
                var mappings = await AsyncExecuter.ToListAsync(
                    mappingQuery.Where(x => x.SupplierId == supplierId && x.IsEnabled && skuIds.Contains(x.SkuId))
                                .Select(x => new { x.SkuId, x.SupplierSkuCode }));
                supplierSkuMap = mappings.ToDictionary(x => x.SkuId, x => x.SupplierSkuCode);

                // 若配置了菜单接口映射，按供应商接口调用并叠加供应商侧字段
                await TryInvokeSupplierInterfaceAsync(input.SupplierCode, MenuInterfaceCode, new Dictionary<string, object?>());
            }
        }

        var items = productList.Select(p =>
        {
            var item = new MenuItemDto
            {
                ProductId = p.Id,
                ProductCode = p.ProductCode,
                Name = p.Name,
                Category = p.Category,
                Description = p.Description,
                Status = (ProductStatusEnum)p.Status
            };

            item.Skus = skuList.Where(s => s.ProductId == p.Id).Select(s => new MenuSkuDto
            {
                SkuCode = s.SkuCode,
                SkuName = s.SkuName,
                Price = s.Price,
                Stock = s.Stock,
                SupplierSkuCode = supplierSkuMap?.GetValueOrDefault(s.Id)
            }).ToList();

            return item;
        }).ToList();

        return new(new MenuResultDto
        {
            SupplierCode = input.SupplierCode,
            Items = items
        });
    }

    /// <summary>
    /// 商品详情接口：返回商品主信息 + SKU 列表。
    /// 指定供应商时，按其「商品详情」接口映射调用供应商，并合并返回供应商侧字段。
    /// </summary>
    public async Task<BaseOutput<ProductDetailResultDto>> GetProductDetailAsync(ProductDetailQueryDto input)
    {
        var productQuery = await _productRepo.GetQueryableAsync();
        var product = string.IsNullOrWhiteSpace(input.ProductCode)
            ? null
            : await AsyncExecuter.FirstOrDefaultAsync(productQuery.Where(x => x.ProductCode == input.ProductCode));

        // 若按 SKU 查询且未指定商品编码，定位 SKU 所属商品
        if (product is null && !string.IsNullOrWhiteSpace(input.SkuCode))
        {
            var skuQuery = await _skuRepo.GetQueryableAsync();
            var sku = await AsyncExecuter.FirstOrDefaultAsync(skuQuery.Where(x => x.SkuCode == input.SkuCode));
            if (sku is not null)
            {
                product = await _productRepo.FindAsync(sku.ProductId);
            }
        }

        if (product is null)
        {
            return new(new ProductDetailResultDto());
        }

        var skusQuery = await _skuRepo.GetQueryableAsync();
        var skus = await AsyncExecuter.ToListAsync(
            skusQuery.Where(x => x.ProductId == product.Id && x.IsEnabled).OrderBy(x => x.SkuCode));

        var result = new ProductDetailResultDto
        {
            Id = product.Id,
            ProductCode = product.ProductCode,
            Name = product.Name,
            Category = product.Category,
            Description = product.Description,
            Status = (ProductStatusEnum)product.Status,
            Remark = product.Remark,
            Skus = skus.Select(s => new ProductSkuDto
            {
                Id = s.Id,
                ProductId = s.ProductId,
                SkuCode = s.SkuCode,
                SkuName = s.SkuName,
                SpecsJson = s.SpecsJson,
                Price = s.Price,
                Stock = s.Stock,
                IsEnabled = s.IsEnabled
            }).ToList()
        };

        // 指定供应商：按其商品详情接口映射调用供应商，合并供应商侧字段
        if (!string.IsNullOrWhiteSpace(input.SupplierCode))
        {
            var standardInput = new Dictionary<string, object?>
            {
                ["productCode"] = input.ProductCode,
                ["skuCode"] = input.SkuCode
            };

            var resp = await TryInvokeSupplierInterfaceAsync(input.SupplierCode, ProductDetailInterfaceCode, standardInput);
            if (resp is not null)
            {
                result.SupplierCode = input.SupplierCode;
                result.SupplierFields = resp.MappedFields;
            }
        }

        return new(result);
    }

    /// <summary>
    /// 下单接口：按供应商 SKU 映射与下单接口映射向供应商下单。
    /// 输入内部 SKU 编码 -> 查找供应商 SKU 映射 -> 构造标准输入 -> 调用供应商下单接口 ->
    /// 按 ResponseMapping 解析供应商订单号等字段。
    /// </summary>
    public async Task<BaseOutput<PlaceOrderResultDto>> PlaceOrderAsync(PlaceOrderDto input)
    {
        var result = new PlaceOrderResultDto { SupplierCode = input.SupplierCode };

        if (string.IsNullOrWhiteSpace(input.SupplierCode) || string.IsNullOrWhiteSpace(input.SkuCode))
        {
            result.Message = "供应商编码与 SKU 编码不能为空";
            return new(result);
        }

        var supplier = await GetSupplierByCodeAsync(input.SupplierCode);
        if (supplier is null)
        {
            result.Message = $"供应商 {input.SupplierCode} 不存在";
            return new(result);
        }

        if (!supplier.IsEnabled)
        {
            result.Message = $"供应商 {input.SupplierCode} 已禁用";
            return new(result);
        }

        // 内部 SKU 查找
        var skuQuery = await _skuRepo.GetQueryableAsync();
        var sku = await AsyncExecuter.FirstOrDefaultAsync(skuQuery.Where(x => x.SkuCode == input.SkuCode));
        if (sku is null)
        {
            result.Message = $"内部 SKU {input.SkuCode} 不存在";
            return new(result);
        }

        // 供应商 SKU 映射查找
        var mappingQuery = await _skuMappingRepo.GetQueryableAsync();
        var skuMapping = await AsyncExecuter.FirstOrDefaultAsync(
            mappingQuery.Where(x => x.SkuId == sku.Id && x.SupplierId == supplier.Id && x.IsEnabled));
        if (skuMapping is null)
        {
            result.Message = $"未找到 SKU {input.SkuCode} 到供应商 {input.SupplierCode} 的映射";
            return new(result);
        }

        // 构造标准输入（供应商映射值 + 下单业务字段）
        var standardInput = new Dictionary<string, object?>
        {
            ["supplierSkuCode"] = skuMapping.SupplierSkuCode,
            ["supplierSkuName"] = skuMapping.SupplierSkuName,
            ["quantity"] = input.Quantity,
            ["externalOrderNo"] = input.ExternalOrderNo,
            ["receiver"] = input.Receiver,
            ["address"] = input.Address,
            ["phone"] = input.Phone,
            ["remark"] = input.Remark
        };

        var resp = await InvokeSupplierInterfaceAsync(supplier, PlaceOrderInterfaceCode, standardInput);
        if (resp is null)
        {
            result.Message = $"供应商 {input.SupplierCode} 未配置下单接口映射";
            return new(result);
        }

        result.RawResponse = resp.ResponseBody;

        if (!resp.Success)
        {
            result.Status = OrderPlaceStatusEnum.Failed;
            result.Message = resp.ErrorMessage ?? "下单失败";
            return new(result);
        }

        result.Status = OrderPlaceStatusEnum.Success;
        result.Success = true;
        result.MappedFields = resp.MappedFields;
        // 约定：返回值字段映射中 TargetField 为 supplierOrderNo 的项为供应商订单号
        result.SupplierOrderNo = resp.MappedFields.TryGetValue("supplierOrderNo", out var orderNo) ? orderNo : null;
        result.Message = "下单成功";
        return new(result);
    }

    /// <summary>
    /// 调用供应商接口并返回结果。若供应商未配置该接口映射则返回 null。
    /// </summary>
    private async Task<SupplierApiResponse?> InvokeSupplierInterfaceAsync(
        SupplierEntity supplier, string interfaceCode, Dictionary<string, object?> standardInput)
    {
        var (apiInterface, mapping) = await LoadInterfaceMappingAsync(supplier.Id, interfaceCode);
        if (apiInterface is null || mapping is null)
        {
            return null;
        }

        var context = new SupplierApiContext
        {
            Supplier = MapSupplierInfo(supplier),
            Interface = MapInterfaceInfo(apiInterface),
            Mapping = MapMappingInfo(mapping),
            Input = standardInput
        };

        var invoker = _invokerFactory.Get((SupplierProtocolEnum)supplier.Protocol);
        return await invoker.InvokeAsync(context);
    }

    /// <summary>
    /// 调用供应商接口；若未配置映射则忽略（不报错），用于菜单/详情这类可选增强场景。
    /// </summary>
    private async Task<SupplierApiResponse?> TryInvokeSupplierInterfaceAsync(
        string supplierCode, string interfaceCode, Dictionary<string, object?> standardInput)
    {
        var supplier = await GetSupplierByCodeAsync(supplierCode);
        if (supplier is null) return null;
        return await InvokeSupplierInterfaceAsync(supplier, interfaceCode, standardInput);
    }

    private async Task<(ApiInterfaceEntity? Interface, SupplierInterfaceMappingEntity? Mapping)> LoadInterfaceMappingAsync(
        string supplierId, string interfaceCode)
    {
        var interfaceQuery = await _interfaceRepo.GetQueryableAsync();
        var apiInterface = await AsyncExecuter.FirstOrDefaultAsync(
            interfaceQuery.Where(x => x.Code == interfaceCode && x.IsEnabled));
        if (apiInterface is null) return (null, null);

        var mappingQuery = await _interfaceMappingRepo.GetQueryableAsync();
        var mapping = await AsyncExecuter.FirstOrDefaultAsync(
            mappingQuery.Where(x => x.SupplierId == supplierId && x.InterfaceId == apiInterface.Id && x.IsEnabled));
        if (mapping is null) return (null, null);

        return (apiInterface, mapping);
    }

    private async Task<SupplierEntity?> GetSupplierByCodeAsync(string code)
    {
        var query = await _supplierRepo.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Code == code));
    }

    private async Task<string?> GetSupplierIdByCodeAsync(string code)
    {
        var supplier = await GetSupplierByCodeAsync(code);
        return supplier?.Id;
    }

    private static SupplierInfo MapSupplierInfo(SupplierEntity s) => new()
    {
        Code = s.Code,
        Name = s.Name,
        ApiUrl = s.ApiUrl,
        AuthType = (AuthTypeEnum)s.AuthType,
        AuthConfig = s.AuthConfig,
        Protocol = (SupplierProtocolEnum)s.Protocol
    };

    private static ApiInterfaceInfo MapInterfaceInfo(ApiInterfaceEntity i) => new()
    {
        Code = i.Code,
        Name = i.Name,
        InterfaceType = (InterfaceTypeEnum)i.InterfaceType,
        HttpMethod = i.HttpMethod,
        Path = i.Path
    };

    private static SupplierInterfaceMappingInfo MapMappingInfo(SupplierInterfaceMappingEntity m) => new()
    {
        SupplierApiUrl = m.SupplierApiUrl,
        RequestMappings = FieldMappingHelper.FromJson(m.RequestMappingJson),
        ResponseMappings = FieldMappingHelper.FromJson(m.ResponseMappingJson)
    };
}