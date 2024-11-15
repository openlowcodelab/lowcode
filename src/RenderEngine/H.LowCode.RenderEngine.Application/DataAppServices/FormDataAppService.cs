using H.LowCode.Domain;
using H.LowCode.RenderEngine.Application.Contracts;
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
    public FormDataAppService(IFormDataDomainService formDataDomainService)
    {

    }

    public Task<FormDTO> Get<TKey>(TKey id)
    {
        throw new NotImplementedException();
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
