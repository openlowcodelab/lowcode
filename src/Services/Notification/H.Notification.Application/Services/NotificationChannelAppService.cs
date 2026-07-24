using H.Notification.Application.Contracts;
using H.Notification.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Notification.Application;

public class NotificationChannelAppService
    : CrudAppService<NotificationChannelEntity, NotificationChannelDto, Guid, NotificationChannelQueryDto, CreateNotificationChannelDto, UpdateNotificationChannelDto>,
    INotificationChannelAppService
{
    public NotificationChannelAppService(IRepository<NotificationChannelEntity, Guid> repository)
        : base(repository)
    {
    }

    public override async Task<PagedResultDto<NotificationChannelDto>> GetListAsync(NotificationChannelQueryDto input)
    {
        var query = (await Repository.GetQueryableAsync())
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => x.Name.Contains(input.Filter!) || x.Code.Contains(input.Filter!));

        var totalCount = await AsyncExecuter.CountAsync(query);
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime)
                 .Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = ObjectMapper.Map<List<NotificationChannelEntity>, List<NotificationChannelDto>>(entities);
        return new PagedResultDto<NotificationChannelDto>(totalCount, dtos);
    }

    public async Task<List<NotificationChannelDto>> GetAllEnabledAsync()
    {
        var query = (await Repository.GetQueryableAsync()).Where(x => x.IsEnabled);
        var entities = await AsyncExecuter.ToListAsync(query);
        return ObjectMapper.Map<List<NotificationChannelEntity>, List<NotificationChannelDto>>(entities);
    }
}
