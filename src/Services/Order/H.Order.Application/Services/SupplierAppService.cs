using H.Abp.Application.Contracts;
using H.Order.Application.Contracts;
using H.Order.Application.Mapping;
using H.Order.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Order.Application.Services;

/// <summary>
/// 供应商管理：CRUD
/// </summary>
public class SupplierAppService
    : ApplicationService,
      ISupplierAppService
{
    protected readonly IRepository<SupplierEntity, Guid> Repository;

    public SupplierAppService(IRepository<SupplierEntity, Guid> repository) { Repository = repository; }

    public async Task<BaseOutput<SupplierDto>> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        return new(entity.ToDto());
    }

    public async Task<BaseOutput<PagedResultDto<SupplierDto>>> GetListAsync(SupplierQueryDto input)
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
        return new(new PagedResultDto<SupplierDto>(totalCount, dtos));
    }

    public async Task<BaseOutput<SupplierDto>> CreateAsync(CreateSupplierDto input)
    {
        var existsQuery = await Repository.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(existsQuery.Where(x => x.Code == input.Code));
        if (exists)
        {
            throw new Exception($"供应商编码 {input.Code} 已存在");
        }

        var entity = input.ToEntity();
        entity = await Repository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return new(entity.ToDto());
    }

    public async Task<BaseOutput<SupplierDto>> UpdateAsync(Guid id, UpdateSupplierDto input)
    {
        var entity = await Repository.GetAsync(id);
        input.Apply(entity);
        entity = await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return new(entity.ToDto());
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        await Repository.DeleteAsync(id);
        return new();
    }
}

/// <summary>
/// 路由规则管理：CRUD
/// </summary>
public class RouteRuleAppService
    : ApplicationService,
      IRouteRuleAppService
{
    protected readonly IRepository<RouteRuleEntity, Guid> Repository;

    public RouteRuleAppService(IRepository<RouteRuleEntity, Guid> repository) { Repository = repository; }

    public async Task<BaseOutput<RouteRuleDto>> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        return new(entity.ToDto());
    }

    public async Task<BaseOutput<PagedResultDto<RouteRuleDto>>> GetListAsync(RouteRuleQueryDto input)
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
        return new(new PagedResultDto<RouteRuleDto>(totalCount, dtos));
    }

    public async Task<BaseOutput<RouteRuleDto>> CreateAsync(CreateRouteRuleDto input)
    {
        var entity = input.ToEntity();
        entity = await Repository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return new(entity.ToDto());
    }

    public async Task<BaseOutput<RouteRuleDto>> UpdateAsync(Guid id, UpdateRouteRuleDto input)
    {
        var entity = await Repository.GetAsync(id);
        input.Apply(entity);
        entity = await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return new(entity.ToDto());
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        await Repository.DeleteAsync(id);
        return new();
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

    public async Task<BaseOutput<PagedResultDto<DispatchLogDto>>> GetListAsync(DispatchLogQueryDto input)
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
        return new(new PagedResultDto<DispatchLogDto>(totalCount, dtos));
    }

    public async Task<BaseOutput<DispatchLogDto>> GetLatestByOrderIdAsync(Guid orderId)
    {
        var query = await _logRepo.GetQueryableAsync();
        var latest = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(x => x.OrderId == orderId).OrderByDescending(x => x.CreationTime));
        return new(latest is null ? null : latest.ToDto());
    }

    public async Task<BaseOutput<TriggerDispatchResultDto>> RetryAsync(Guid logId)
    {
        var log = await _logRepo.GetAsync(logId);
        return new(await _dispatchService.DispatchAsync(log.OrderId));
    }
}