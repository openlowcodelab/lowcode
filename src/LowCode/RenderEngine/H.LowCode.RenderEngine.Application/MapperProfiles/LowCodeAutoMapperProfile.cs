using AutoMapper;
using H.LowCode.Application.Contracts;
using H.LowCode.Entity;

namespace H.LowCode.RenderEngine.Application;

public class LowCodeAutoMapperProfile : Profile
{
    public LowCodeAutoMapperProfile()
    {
        CreateMap<FormEntity, FormDataDto>();
        CreateMap<FormDataDto, FormEntity>();

        CreateMap<FormFieldEntity, FormFieldDto>();
        CreateMap<FormFieldDto, FormFieldEntity>();
    }
}