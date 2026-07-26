using H.Order.Application.Contracts;
using H.Order.Application.Mapping;
using H.Order.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Order.Application.Services;

/// <summary>
/// 供应商管理：CRUD
/// </summary>
public class SupplierAppService
    : CrudAppService<SupplierEntity, SupplierDto, Guid, SupplierQueryDto, CreateSupplierDto, UpdateSupplierDto>,
      ISupplierAppService
{
    public SupplierAppService(IRepository<SupplierEntity, Guid> repository) : base(repository) { }

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
/// 路由规则管理：CRUD
/// </summary>
public class RouteRuleAppService
    : CrudAppService<RouteRuleEntity, RouteRuleDto, Guid, RouteRuleQueryDto, CreateRouteRuleDto, UpdateRouteRuleDto>,
      IRouteRuleAppService
{
    public RouteRuleAppService(IRepository<RouteRuleEntity, Guid> repository) : base(repository) { }

    public override async Task<PagedResultDto<RouteRuleDto>> GetListAsync(RouteRuleQueryDto input)
    {
        var query = await Repository.GetQueryableAsync();
        if (!string.IsNullOrWhiteSpace(input.Filter))
            query = query.Where(x => x.Name.Contains(input.Filter) || x.SupplierCode.Contains(input.Filter));
        if (input.IsEnabled.HasValue)
            query = query.Where(x => x.IsEnabled == input.IsEnabled!.Value);
        if (!string.IsNullOrWhiteSpace(input.SupplierCode))
            query = query.Where(x => x.SupplierCode == input.SupplierCode);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.Priority).Skip(input.SkipCount).Take(maxResult));

        var dtos = entities.Select(e => e.ToDto()).ToList();
        return new PagedResultDto<RouteRuleDto>(totalCount, dtos);
    }

    public override async Task<RouteRuleDto> CreateAsync(CreateRouteRuleDto input)
    {
        var entity = input.ToEntity();
        await Repository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return entity.ToDto();
    }

    public override async Task<RouteRuleDto> UpdateAsync(Guid id, UpdateRouteRuleDto input)
    {
        var entity = await Repository.GetAsync(id);
        input.Apply(entity);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return entity.ToDto();
    }
}

/// <summary>
/// 下发日志查询与重试
/// </summary>
public class DispatchLogAppService
    : ApplicationService, IDispatchLogAppService
{
    private readonly IRepository<DispatchLogEntity, Guid> _logRepo;
    private readonly IDispatchService _dispatchService;

    public DispatchLogAppService(
        IRepository<DispatchLogEntity, Guid> logRepo,
        IDispatchService dispatchService)
    {
        _logRepo = logRepo;
        _dispatchService = dispatchService;
    }

    public async Task<PagedResultDto<DispatchLogDto>> GetListAsync(DispatchLogQueryDto input)
    {
        var query = await _logRepo.GetQueryableAsync();
        if (input.OrderId.HasValue)
            query = query.Where(x => x.OrderId == input.OrderId!.Value);
        if (!string.IsNullOrWhiteSpace(input.SupplierCode))
            query = query.Where(x => x.SupplierCode == input.SupplierCode);
        if (input.Status.HasValue)
            query = query.Where(x => x.Status == (int)input.Status!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(maxResult));

        var dtos = entities.Select(e => e.ToDto()).ToList();
        return new PagedResultDto<DispatchLogDto>(totalCount, dtos);
    }

    public async Task<DispatchLogDto?> GetLatestByOrderIdAsync(Guid orderId)
    {
        var query = await _logRepo.GetQueryableAsync();
        var latest = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(x => x.OrderId == orderId).OrderByDescending(x => x.CreationTime));
        return latest is null ? null : latest.ToDto();
    }

    public async Task<TriggerDispatchResultDto> RetryAsync(Guid logId)
    {
        var log = await _logRepo.GetAsync(logId);
        return await _dispatchService.DispatchAsync(log.OrderId);
    }
}