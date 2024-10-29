using H.LowCode.MetaSchema;

namespace H.LowCode.RenderEngine.Application.Contracts;

public interface IFormDataAppService
{
    Task<bool> Save(FormCreateOrUpdateDTO dto);

    Task<FormDTO> Get<TKey>(TKey id);

    Task<bool> Delete<TKey>(TKey id);
}