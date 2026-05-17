using AutoMapper;
using H.SystemManagement.Application.Contracts.Dtos;
using H.SystemManagement.EntityFrameworkCore.Entities;

namespace H.SystemManagement.Application.Mapping;

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
