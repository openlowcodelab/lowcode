using AutoMapper;
using H.SystemManagement.Application.Contracts;
using H.SystemManagement.EntityFrameworkCore;

namespace H.SystemManagement.Application;

public class SystemManagementProfile : Profile
{
    public SystemManagementProfile()
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
