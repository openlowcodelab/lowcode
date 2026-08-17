using DotNetCore.CAP;
using H.Abp.Application.Contracts;
using H.Order.Application.Contracts;
using H.Order.Application.Mapping;
using H.Order.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Order.Application.Services;

/// <summary>
/// 订单应用服务（对外开放）。
/// 列表查询仅返回核心字段，绝不关联扩展表；
/// 详情接口才会按订单ID单独查询扩展表，合并返回全部行业特有属性。
/// </summary>
public class OrderAppService
    : ApplicationService,
      IOrderAppService
{
    protected readonly IRepository<OrderEntity, Guid> Repository;
    private readonly IRepository<OrderExtensionEntity, Guid> _extensionRepo;
    private readonly IRepository<DispatchLogEntity, Guid> _dispatchLogRepo;
    private readonly IDispatchService _dispatchService;
    private readonly ICapPublisher _capPublisher;

    public OrderAppService(
        IRepository<OrderEntity, Guid> repository,
        IRepository<OrderExtensionEntity, Guid> extensionRepo,
        IRepository<DispatchLogEntity, Guid> dispatchLogRepo,
        IDispatchService dispatchService,
        ICapPublisher capPublisher)
    {
        Repository = repository;
        _extensionRepo = extensionRepo;
        _dispatchLogRepo = dispatchLogRepo;
        _dispatchService = dispatchService;
        _capPublisher = capPublisher;
    }

    /// <summary>
    /// 订单列表：仅核心字段，不关联扩展属性表
    /// </summary>
    public async Task<BaseOutput<PagedResultDto<OrderDto>>> GetListAsync(OrderQueryDto input)
    {
        var query = await Repository.GetQueryableAsync();
        query = ApplyFilters(query, input);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime)
                 .Skip(input.SkipCount)
                 .Take(maxResult));

        var dtos = entities.Select(e => e.ToDto()).ToList();
        return BaseOutput<PagedResultDto<OrderDto>>.Ok(new PagedResultDto<OrderDto>(totalCount, dtos));
    }

    protected virtual IQueryable<OrderEntity> ApplyFilters(IQueryable<OrderEntity> query, OrderQueryDto input)
    {
        if (!string.IsNullOrWhiteSpace(input.Filter))
            query = query.Where(x => x.OrderNo.Contains(input.Filter) || x.ProductName.Contains(input.Filter));
        if (!string.IsNullOrWhiteSpace(input.OrderNo))
            query = query.Where(x => x.OrderNo == input.OrderNo);
        if (!string.IsNullOrWhiteSpace(input.Industry))
            query = query.Where(x => x.Industry == input.Industry);
        if (!string.IsNullOrWhiteSpace(input.BuyerId))
            query = query.Where(x => x.BuyerId == input.BuyerId);
        if (input.Status.HasValue)
            query = query.Where(x => x.OrderStatus == (int)input.Status!.Value);
        if (input.MinAmount.HasValue)
            query = query.Where(x => x.TotalAmount >= input.MinAmount!.Value);
        if (input.MaxAmount.HasValue)
            query = query.Where(x => x.TotalAmount <= input.MaxAmount!.Value);
        if (input.CreateTimeStart.HasValue)
            query = query.Where(x => x.CreationTime >= input.CreateTimeStart!.Value);
        if (input.CreateTimeEnd.HasValue)
            query = query.Where(x => x.CreationTime <= input.CreateTimeEnd!.Value);
        return query;
    }

    public async Task<BaseOutput<OrderDto>> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        return BaseOutput<OrderDto>.Ok(entity.ToDto());
    }

    /// <summary>
    /// 订单详情：核心字段 + 行业扩展属性 + 最近下发状态
    /// </summary>
    public async Task<BaseOutput<OrderDetailDto>> GetDetailAsync(Guid id)
    {
        var order = await Repository.GetAsync(id);

        var extQueryable = await _extensionRepo.GetQueryableAsync();
        var ext = await AsyncExecuter.FirstOrDefaultAsync(extQueryable.Where(x => x.OrderId == id));

        var dto = order.ToDetailDto();
        dto.AttributesJson = ext?.AttributesJson;
        dto.DispatchStatus = await GetDispatchStatusInternalAsync(id);
        return BaseOutput<OrderDetailDto>.Ok(dto);
    }

    /// <summary>
    /// 创建订单：核心字段 + 扩展属同事务写入；进入待下发状态则发布 CAP 事件
    /// </summary>
    public async Task<BaseOutput<OrderDto>> CreateAsync(CreateOrderDto input)
    {
        var entity = input.ToEntity();
        entity.OrderNo = string.IsNullOrWhiteSpace(input.OrderNo)
            ? GenerateOrderNo()
            : input.OrderNo!.Trim();

        // 编码唯一性检查
        var existsQuery = await Repository.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(existsQuery.Where(x => x.OrderNo == entity.OrderNo));
        if (exists)
        {
            throw new Exception($"订单号 {entity.OrderNo} 已存在");
        }

        await Repository.InsertAsync(entity);
        // entity.Id 由 ABP IGuidGenerator 在 InsertAsync 时赋值

        if (!string.IsNullOrWhiteSpace(input.AttributesJson))
        {
            var ext = new OrderExtensionEntity
            {
                OrderId = entity.Id,
                AttributesJson = input.AttributesJson
            };
            await _extensionRepo.InsertAsync(ext);
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        if (input.OrderStatus == OrderStatusEnum.PendingDispatch)
        {
            await PublishPendingDispatchEventAsync(entity);
        }

        return BaseOutput<OrderDto>.Ok(entity.ToDto());
    }

    public async Task<BaseOutput<OrderDto>> UpdateAsync(Guid id, UpdateOrderDto input)
    {
        var entity = await Repository.GetAsync(id);
        input.Apply(entity);

        // 扩展属性同 upsert：仅当显式传入 AttributesJson（null 表示保持不变）
        if (input.AttributesJson is not null)
        {
            var extQueryable = await _extensionRepo.GetQueryableAsync();
            var ext = await AsyncExecuter.FirstOrDefaultAsync(extQueryable.Where(x => x.OrderId == id));
            if (ext is null)
            {
                var newExt = new OrderExtensionEntity
                {
                    OrderId = id,
                    AttributesJson = input.AttributesJson
                };
                await _extensionRepo.InsertAsync(newExt);
            }
            else
            {
                ext.AttributesJson = input.AttributesJson;
                await _extensionRepo.UpdateAsync(ext);
            }
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        if (input.OrderStatus == OrderStatusEnum.PendingDispatch)
        {
            await PublishPendingDispatchEventAsync(entity);
        }

        return BaseOutput<OrderDto>.Ok(entity.ToDto());
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        await Repository.DeleteAsync(entity);

        var extQueryable = await _extensionRepo.GetQueryableAsync();
        var ext = await AsyncExecuter.FirstOrDefaultAsync(extQueryable.Where(x => x.OrderId == id));
        if (ext is not null)
        {
            await _extensionRepo.DeleteAsync(ext);
        }
        return BaseOutput.Ok();
    }

    /// <summary>手动触发订单下发</summary>
    public async Task<BaseOutput<TriggerDispatchResultDto>> TriggerDispatchAsync(Guid id)
    {
        return BaseOutput<TriggerDispatchResultDto>.Ok(await _dispatchService.DispatchAsync(id));
    }

    /// <summary>查询订单最近一次下发状态</summary>
    public async Task<BaseOutput<DispatchStatusDto>> GetDispatchStatusAsync(Guid id)
    {
        return BaseOutput<DispatchStatusDto>.Ok(await GetDispatchStatusInternalAsync(id));
    }

    private async Task<DispatchStatusDto?> GetDispatchStatusInternalAsync(Guid id)
    {
        var queryable = await _dispatchLogRepo.GetQueryableAsync();
        var latest = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(x => x.OrderId == id).OrderByDescending(x => x.CreationTime));
        if (latest is null) return null;

        return new DispatchStatusDto
        {
            SupplierCode = latest.SupplierCode,
            Status = (DispatchStatusEnum)latest.Status,
            ErrorMessage = latest.ErrorMessage,
            RequestTime = latest.RequestTime
        };
    }

    private async Task PublishPendingDispatchEventAsync(OrderEntity entity)
    {
        var evt = new OrderPendingDispatchEvent
        {
            OrderId = entity.Id,
            OrderNo = entity.OrderNo,
            Industry = entity.Industry,
            ProductCategory = entity.ProductCategory
        };
        await _capPublisher.PublishAsync(OrderTopics.PendingDispatch, evt);
    }

    private static string GenerateOrderNo()
    {
        return "O" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")
             + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
    }
}