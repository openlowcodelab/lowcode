using H.Abp.Application.Contracts;
using H.SupplyChain.Application.Contracts;
using H.SupplyChain.Application.Mapping;
using H.SupplyChain.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.SupplyChain.Application.Services;

/// <summary>
/// 供应商 SKU 映射管理：CRUD（一个内部 SKU 可映射多个供应商）
/// </summary>
public class SupplierSkuMappingAppService
    : ApplicationService,
      ISupplierSkuMappingAppService
{
    protected readonly IRepository<SupplierSkuMappingEntity, Guid> Repository;
    private readonly IRepository<ProductSkuEntity, Guid> _skuRepo;
    private readonly IRepository<SupplierEntity, Guid> _supplierRepo;

    public SupplierSkuMappingAppService(
        IRepository<SupplierSkuMappingEntity, Guid> repository,
        IRepository<ProductSkuEntity, Guid> skuRepo,
        IRepository<SupplierEntity, Guid> supplierRepo)
    {
        Repository = repository;
        _skuRepo = skuRepo;
        _supplierRepo = supplierRepo;
    }

    public async Task<PagedResultDto<SupplierSkuMappingDto>> GetListAsync(SupplierSkuMappingQueryDto input)
    {
        var query = await Repository.GetQueryableAsync();
        if (input.SkuId.HasValue)
            query = query.Where(x => x.SkuId == input.SkuId!.Value);
        if (input.SupplierId.HasValue)
            query = query.Where(x => x.SupplierId == input.SupplierId!.Value);
        if (input.IsEnabled.HasValue)
            query = query.Where(x => x.IsEnabled == input.IsEnabled!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(maxResult));

        var dtos = await BuildDtosAsync(entities);
        return new PagedResultDto<SupplierSkuMappingDto>(totalCount, dtos);
    }

    public async Task<SupplierSkuMappingDto> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        return (await BuildDtosAsync(new[] { entity }))[0];
    }

    public async Task<SupplierSkuMappingDto> CreateAsync(CreateSupplierSkuMappingDto input)
    {
        var existsQuery = await Repository.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(
            existsQuery.Where(x => x.SkuId == input.SkuId && x.SupplierId == input.SupplierId));
        if (exists)
        {
            throw new Exception("该 SKU 已映射到此供应商，请勿重复添加");
        }

        var entity = input.ToEntity();
        await Repository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return (await BuildDtosAsync(new[] { entity }))[0];
    }

    public async Task<SupplierSkuMappingDto> UpdateAsync(Guid id, UpdateSupplierSkuMappingDto input)
    {
        var entity = await Repository.GetAsync(id);
        input.Apply(entity);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return (await BuildDtosAsync(new[] { entity }))[0];
    }

    public async Task DeleteAsync(Guid id)
    {
        await Repository.DeleteAsync(id);
    }

    /// <summary>加载 SKU 编码与供应商编码，组装展示 DTO</summary>
    private async Task<List<SupplierSkuMappingDto>> BuildDtosAsync(IEnumerable<SupplierSkuMappingEntity> entities)
    {
        var list = entities.ToList();
        var skuIds = list.Select(x => x.SkuId).Distinct().ToList();
        var supplierIds = list.Select(x => x.SupplierId).Distinct().ToList();

        var skuQuery = await _skuRepo.GetQueryableAsync();
        var skuCodes = await AsyncExecuter.ToListAsync(
            skuQuery.Where(x => skuIds.Contains(x.Id)).Select(x => new { x.Id, x.SkuCode }));

        var supplierQuery = await _supplierRepo.GetQueryableAsync();
        var supplierCodes = await AsyncExecuter.ToListAsync(
            supplierQuery.Where(x => supplierIds.Contains(x.Id)).Select(x => new { x.Id, x.Code }));

        var skuMap = skuCodes.ToDictionary(x => x.Id, x => x.SkuCode);
        var supplierMap = supplierCodes.ToDictionary(x => x.Id, x => x.Code);

        return list.Select(e => e.ToDto(
            skuMap.GetValueOrDefault(e.SkuId, string.Empty),
            supplierMap.GetValueOrDefault(e.SupplierId, string.Empty))).ToList();
    }
}

/// <summary>
/// 接口定义管理：CRUD（菜单接口、商品接口、下单接口等）
/// </summary>
public class ApiInterfaceAppService
    : ApplicationService,
      IApiInterfaceAppService
{
    protected readonly IRepository<ApiInterfaceEntity, Guid> Repository;

    public ApiInterfaceAppService(IRepository<ApiInterfaceEntity, Guid> repository) { Repository = repository; }

    public async Task<ApiInterfaceDto> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        return entity.ToDto();
    }

    public async Task<PagedResultDto<ApiInterfaceDto>> GetListAsync(ApiInterfaceQueryDto input)
    {
        var query = await Repository.GetQueryableAsync();
        if (!string.IsNullOrWhiteSpace(input.Filter))
            query = query.Where(x => x.Code.Contains(input.Filter) || x.Name.Contains(input.Filter));
        if (input.InterfaceType.HasValue)
            query = query.Where(x => x.InterfaceType == (int)input.InterfaceType!.Value);
        if (input.IsEnabled.HasValue)
            query = query.Where(x => x.IsEnabled == input.IsEnabled!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(maxResult));

        var dtos = entities.Select(e => e.ToDto()).ToList();
        return new PagedResultDto<ApiInterfaceDto>(totalCount, dtos);
    }

    public async Task<ApiInterfaceDto> CreateAsync(CreateApiInterfaceDto input)
    {
        var existsQuery = await Repository.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(existsQuery.Where(x => x.Code == input.Code));
        if (exists)
        {
            throw new Exception($"接口编码 {input.Code} 已存在");
        }

        var entity = input.ToEntity();
        await Repository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return entity.ToDto();
    }

    public async Task<ApiInterfaceDto> UpdateAsync(Guid id, UpdateApiInterfaceDto input)
    {
        var entity = await Repository.GetAsync(id);
        input.Apply(entity);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return entity.ToDto();
    }

    public async Task DeleteAsync(Guid id)
    {
        await Repository.DeleteAsync(id);
    }
}