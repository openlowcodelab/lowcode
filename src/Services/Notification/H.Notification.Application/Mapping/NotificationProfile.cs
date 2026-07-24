using AutoMapper;
using H.Notification.Application.Contracts;
using H.Notification.EntityFrameworkCore;

namespace H.Notification.Application;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        // 渠道
        CreateMap<NotificationChannelEntity, NotificationChannelDto>();
        CreateMap<CreateNotificationChannelDto, NotificationChannelEntity>();
    }
}
