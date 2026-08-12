using H.Abp.Application.Contracts;
using H.SupplyChain.Application.Contracts;
using H.SupplyChain.Application.Mapping;
using H.SupplyChain.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.SupplyChain.Application.Services;

/// <summary>
/// 供应商接口映射管理：CRUD。
/// 基于接口定义，配置对应供应商的接口请求参数映射、返回值字段映射。
/// </summary>
public class SupplierInterfaceMappingAppService
    : ApplicationService,
      ISupplierInterfaceMappingAppService
{
    protected readonly IRepository<SupplierInterfaceMappingEntity, Guid> Repository;
    private readonly IRepository<SupplierEntity, Guid> _supplierRepo;
    private readonly IRepository<ApiInterfaceEntity, Guid> _interfaceRepo;

    public SupplierInterfaceMappingAppService(
        IRepository<SupplierInterfaceMappingEntity, Guid> repository,
        IRepository<SupplierEntity, Guid> supplierRepo,
        IRepository<ApiInterfaceEntity, Guid> interfaceRepo)
    {
        Repository = repository;
        _supplierRepo = supplierRepo;
        _interfaceRepo = interfaceRepo;
    }

    public async Task<PagedResultDto<SupplierInterfaceMappingDto>> GetListAsync(SupplierInterfaceMappingQueryDto input)
    {
        var query = await Repository.GetQueryableAsync();
        if (input.SupplierId.HasValue)
            query = query.Where(x => x.SupplierId == input.SupplierId!.Value);
        if (input.InterfaceId.HasValue)
            query = query.Where(x => x.InterfaceId == input.InterfaceId!.Value);
        if (input.IsEnabled.HasValue)
            query = query.Where(x => x.IsEnabled == input.IsEnabled!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(maxResult));

        var dtos = await BuildDtosAsync(entities);
        return new PagedResultDto<SupplierInterfaceMappingDto>(totalCount, dtos);
    }

    public async Task<SupplierInterfaceMappingDto> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        return (await BuildDtosAsync(new[] { entity }))[0];
    }

    public async Task<SupplierInterfaceMappingDto> CreateAsync(CreateSupplierInterfaceMappingDto input)
    {
        var existsQuery = await Repository.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(
            existsQuery.Where(x => x.SupplierId == input.SupplierId && x.InterfaceId == input.InterfaceId));
        if (exists)
        {
            throw new Exception("该供应商已配置此接口映射，请勿重复添加");
        }

        var entity = input.ToEntity();
        await Repository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return (await BuildDtosAsync(new[] { entity }))[0];
    }

    public async Task<SupplierInterfaceMappingDto> UpdateAsync(Guid id, UpdateSupplierInterfaceMappingDto input)
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

    /// <summary>加载供应商编码与接口编码，组装展示 DTO</summary>
    private async Task<List<SupplierInterfaceMappingDto>> BuildDtosAsync(IEnumerable<SupplierInterfaceMappingEntity> entities)
    {
        var list = entities.ToList();
        var supplierIds = list.Select(x => x.SupplierId).Distinct().ToList();
        var interfaceIds = list.Select(x => x.InterfaceId).Distinct().ToList();

        var supplierQuery = await _supplierRepo.GetQueryableAsync();
        var supplierCodes = await AsyncExecuter.ToListAsync(
            supplierQuery.Where(x => supplierIds.Contains(x.Id)).Select(x => new { x.Id, x.Code }));

        var interfaceQuery = await _interfaceRepo.GetQueryableAsync();
        var interfaceCodes = await AsyncExecuter.ToListAsync(
            interfaceQuery.Where(x => interfaceIds.Contains(x.Id)).Select(x => new { x.Id, x.Code }));

        var supplierMap = supplierCodes.ToDictionary(x => x.Id, x => x.Code);
        var interfaceMap = interfaceCodes.ToDictionary(x => x.Id, x => x.Code);

        return list.Select(e => e.ToDto(
            supplierMap.GetValueOrDefault(e.SupplierId, string.Empty),
            interfaceMap.GetValueOrDefault(e.InterfaceId, string.Empty))).ToList();
    }
}