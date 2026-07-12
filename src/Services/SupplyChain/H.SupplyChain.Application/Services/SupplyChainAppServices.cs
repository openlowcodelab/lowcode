using H.SupplyChain.Application.Contracts;
using H.SupplyChain.Application.Mapping;
using H.SupplyChain.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.SupplyChain.Application.Services;

/// <summary>
/// 供应商管理：CRUD
/// </summary>
public class SupplyChainSupplierAppService
    : CrudAppService<SupplierEntity, SupplierDto, Guid, SupplierQueryDto, CreateSupplierDto, UpdateSupplierDto>,
      ISupplyChainSupplierAppService
{
    public SupplyChainSupplierAppService(IRepository<SupplierEntity, Guid> repository) : base(repository) { }

    public override async Task<PagedResultDto<SupplierDto>> GetListAsync(SupplierQueryDto input)
    {
        var query = await Repository.GetQueryableAsync();
        if (!string.IsNullOrWhiteSpace(input.Filter))
            query = query.Where(x => x.Code.Contains(input.Filter) || x.Name.Contains(input.Filter));
        if (input.IsEnabled.HasValue)
            query = query.Where(x => x.IsEnabled == input.IsEnabled!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(maxResult));

        var dtos = entities.Select(e => e.ToDto()).ToList();
        return new PagedResultDto<SupplierDto>(totalCount, dtos);
    }

    public override async Task<SupplierDto> CreateAsync(CreateSupplierDto input)
    {
        var existsQuery = await Repository.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(existsQuery.Where(x => x.Code == input.Code));
        if (exists)
        {
            throw new Exception($"供应商编码 {input.Code} 已存在");
        }

        var entity = input.ToEntity();
        await Repository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return entity.ToDto();
    }

    public override async Task<SupplierDto> UpdateAsync(Guid id, UpdateSupplierDto input)
    {
        var entity = await Repository.GetAsync(id);
        input.Apply(entity);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return entity.ToDto();
    }
}

/// <summary>
/// 商品管理：CRUD + 详情（含 SKU 列表）
/// </summary>
public class ProductAppService
    : CrudAppService<ProductEntity, ProductDto, Guid, ProductQueryDto, CreateProductDto, UpdateProductDto>,
      IProductAppService
{
    private readonly IRepository<ProductSkuEntity, Guid> _skuRepo;

    public ProductAppService(
        IRepository<ProductEntity, Guid> repository,
        IRepository<ProductSkuEntity, Guid> skuRepo) : base(repository)
    {
        _skuRepo = skuRepo;
    }

    public override async Task<PagedResultDto<ProductDto>> GetListAsync(ProductQueryDto input)
    {
        var query = await Repository.GetQueryableAsync();
        if (!string.IsNullOrWhiteSpace(input.Filter))
            query = query.Where(x => x.ProductCode.Contains(input.Filter) || x.Name.Contains(input.Filter));
        if (!string.IsNullOrWhiteSpace(input.Category))
            query = query.Where(x => x.Category == input.Category);
        if (input.Status.HasValue)
            query = query.Where(x => x.Status == (int)input.Status!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(maxResult));

        var dtos = entities.Select(e => e.ToDto()).ToList();
        return new PagedResultDto<ProductDto>(totalCount, dtos);
    }

    public override async Task<ProductDto> CreateAsync(CreateProductDto input)
    {
        var existsQuery = await Repository.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(existsQuery.Where(x => x.ProductCode == input.ProductCode));
        if (exists)
        {
            throw new Exception($"商品编码 {input.ProductCode} 已存在");
        }

        var entity = input.ToEntity();
        await Repository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return entity.ToDto();
    }

    public override async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto input)
    {
        var entity = await Repository.GetAsync(id);
        input.Apply(entity);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return entity.ToDto();
    }

    /// <summary>商品详情：主表信息 + SKU 列表</summary>
    public async Task<ProductDetailDto> GetDetailAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        var skuQuery = await _skuRepo.GetQueryableAsync();
        var skus = await AsyncExecuter.ToListAsync(
            skuQuery.Where(x => x.ProductId == id).OrderBy(x => x.SkuCode));

        var skuDtos = skus.Select(s => s.ToDto()).ToList();
        return entity.ToDetailDto(skuDtos);
    }
}

/// <summary>
/// 商品 SKU 管理：CRUD
/// </summary>
public class ProductSkuAppService
    : CrudAppService<ProductSkuEntity, ProductSkuDto, Guid, ProductSkuQueryDto, CreateProductSkuDto, UpdateProductSkuDto>,
      IProductSkuAppService
{
    public ProductSkuAppService(IRepository<ProductSkuEntity, Guid> repository) : base(repository) { }

    public override async Task<PagedResultDto<ProductSkuDto>> GetListAsync(ProductSkuQueryDto input)
    {
        var query = await Repository.GetQueryableAsync();
        if (input.ProductId.HasValue)
            query = query.Where(x => x.ProductId == input.ProductId!.Value);
        if (!string.IsNullOrWhiteSpace(input.Filter))
            query = query.Where(x => x.SkuCode.Contains(input.Filter) || x.SkuName.Contains(input.Filter));
        if (input.IsEnabled.HasValue)
            query = query.Where(x => x.IsEnabled == input.IsEnabled!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.SkuCode).Skip(input.SkipCount).Take(maxResult));

        var dtos = entities.Select(e => e.ToDto()).ToList();
        return new PagedResultDto<ProductSkuDto>(totalCount, dtos);
    }

    public override async Task<ProductSkuDto> CreateAsync(CreateProductSkuDto input)
    {
        var existsQuery = await Repository.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(existsQuery.Where(x => x.SkuCode == input.SkuCode));
        if (exists)
        {
            throw new Exception($"SKU 编码 {input.SkuCode} 已存在");
        }

        var entity = input.ToEntity();
        await Repository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return entity.ToDto();
    }

    public override async Task<ProductSkuDto> UpdateAsync(Guid id, UpdateProductSkuDto input)
    {
        var entity = await Repository.GetAsync(id);
        input.Apply(entity);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return entity.ToDto();
    }
}