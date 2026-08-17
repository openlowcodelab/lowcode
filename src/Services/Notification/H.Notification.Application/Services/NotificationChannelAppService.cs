using H.Abp.Application.Contracts;
using H.Notification.Application.Contracts;
using H.Notification.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Notification.Application;

public class NotificationChannelAppService
    : ApplicationService,
    INotificationChannelAppService
{
    private readonly IRepository<NotificationChannelEntity, Guid> _repository;

    public NotificationChannelAppService(IRepository<NotificationChannelEntity, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<BaseOutput<PagedResultDto<NotificationChannelDto>>> GetListAsync(NotificationChannelQueryDto input)
    {
        var query = (await _repository.GetQueryableAsync())
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => x.Name.Contains(input.Filter!) || x.Code.Contains(input.Filter!));

        var totalCount = await AsyncExecuter.CountAsync(query);
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime)
                 .Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = ObjectMapper.Map<List<NotificationChannelEntity>, List<NotificationChannelDto>>(entities);
        return BaseOutput<PagedResultDto<NotificationChannelDto>>.Ok(new PagedResultDto<NotificationChannelDto>(totalCount, dtos));
    }

    public async Task<BaseOutput<NotificationChannelDto>> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return BaseOutput<NotificationChannelDto>.Ok(ObjectMapper.Map<NotificationChannelEntity, NotificationChannelDto>(entity));
    }

    public async Task<BaseOutput<NotificationChannelDto>> CreateAsync(CreateNotificationChannelDto input)
    {
        var entity = ObjectMapper.Map<CreateNotificationChannelDto, NotificationChannelEntity>(input);
        await _repository.InsertAsync(entity, autoSave: true);
        return BaseOutput<NotificationChannelDto>.Ok(ObjectMapper.Map<NotificationChannelEntity, NotificationChannelDto>(entity));
    }

    public async Task<BaseOutput<NotificationChannelDto>> UpdateAsync(Guid id, UpdateNotificationChannelDto input)
    {
        var entity = await _repository.GetAsync(id);
        ObjectMapper.Map(input, entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return BaseOutput<NotificationChannelDto>.Ok(ObjectMapper.Map<NotificationChannelEntity, NotificationChannelDto>(entity));
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
        return BaseOutput.Ok();
    }

    public async Task<BaseOutput<List<NotificationChannelDto>>> GetAllEnabledAsync()
    {
        var query = (await _repository.GetQueryableAsync()).Where(x => x.IsEnabled);
        var entities = await AsyncExecuter.ToListAsync(query);
        return BaseOutput<List<NotificationChannelDto>>.Ok(ObjectMapper.Map<List<NotificationChannelEntity>, List<NotificationChannelDto>>(entities));
    }
}
