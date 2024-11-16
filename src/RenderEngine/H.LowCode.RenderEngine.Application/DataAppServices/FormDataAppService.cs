using H.LowCode.Domain;
using H.LowCode.RenderEngine.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.RenderEngine.Application;

[RemoteService]
public class FormDataAppService : ApplicationService, IFormDataAppService
{
    private IFormDataDomainService _formDataDomainService => LazyServiceProvider.GetRequiredService<IFormDataDomainService>();

    public async Task<FormDTO> Get<TKey>(TKey id)
    {
        var entity = await _formDataDomainService.Get(id);
        var dto = ObjectMapper.Map<FormEntity, FormDTO>(entity);
        return dto;
    }

    public Task<bool> Save(FormCreateOrUpdateDTO dto)
    {
        throw new NotImplementedException();
    }
    public Task<bool> Delete<TKey>(TKey id)
    {
        throw new NotImplementedException();
    }
}
