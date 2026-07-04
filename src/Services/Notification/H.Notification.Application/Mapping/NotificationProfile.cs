using AutoMapper;
using H.Notification.Application.Contracts;
using H.Notification.EntityFrameworkCore;

namespace H.Notification.Application;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        // NotificationBusiness 映射
        CreateMap<NotificationBusinessEntity, NotificationBusinessDto>();
        CreateMap<CreateNotificationBusinessDto, NotificationBusinessEntity>();
        CreateMap<UpdateNotificationBusinessDto, NotificationBusinessEntity>();

        // NotificationMethodConfig 映射
        CreateMap<NotificationMethodConfigEntity, NotificationMethodConfigDto>();
        CreateMap<CreateNotificationMethodConfigDto, NotificationMethodConfigEntity>();
    }
}
