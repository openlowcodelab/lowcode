using H.LowCode.MetaSchema;
using Volo.Abp.Application.Services;

namespace H.LowCode.RenderEngine.Application.Contracts;

public interface IFormDataAppService : IApplicationService
{
    Task<bool> Save(FormCreateOrUpdateDTO dto);

    Task<FormDataDTO> Get<TKey>(TKey id);

    Task<bool> Delete<TKey>(TKey id);
}