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

    public async Task<FormDataDTO> GetAsync(string appId, string pageId, string dataId)
    {
        var entity = await _formDataDomainService.GetAsync(appId, pageId, dataId);
        var dto = ObjectMapper.Map<FormEntity, FormDataDTO>(entity);
        return dto;
    }

    public async Task<bool> SaveAsync(FormCreateOrUpdateDTO dto)
    {
        return await _formDataDomainService.SaveAsync(null);
    }

    public async Task<bool> DeleteAsync(string appId, string pageId, string id)
    {
        return await _formDataDomainService.DeleteAsync(appId, pageId, id);
    }
}
