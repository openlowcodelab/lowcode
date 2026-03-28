using AutoMapper;
using H.LowCode.Entity;
using H.LowCode.Application.Contracts;

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