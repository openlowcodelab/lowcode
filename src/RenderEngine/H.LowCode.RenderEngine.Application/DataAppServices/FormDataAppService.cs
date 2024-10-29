using H.LowCode.Domain;
using H.LowCode.RenderEngine.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine.Application;

public class FormDataAppService : IFormDataAppService
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
