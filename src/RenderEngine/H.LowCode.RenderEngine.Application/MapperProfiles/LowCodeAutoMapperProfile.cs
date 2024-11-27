using AutoMapper;
using H.LowCode.Domain;
using H.LowCode.RenderEngine.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine.Application;

public class LowCodeAutoMapperProfile : Profile
{
    public LowCodeAutoMapperProfile()
    {
        CreateMap<FormEntity, FormDataDTO>();
    }
}