using H.Abstractions;

namespace H.LowCode.Application.Contracts;

public interface IFormDataAppService : IAppService
{
    Task<bool> SaveAsync(FormDataDto dto);

    Task<FormDataDto> GetAsync(string appId, string pageId, string id);

    Task<bool> DeleteAsync(string appId, string pageId, string id);
}