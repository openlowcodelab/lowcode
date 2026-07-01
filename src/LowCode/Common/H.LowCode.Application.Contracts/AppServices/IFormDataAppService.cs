using Volo.Abp.Http;
using Volo.Abp.Application.Services;

namespace H.LowCode.Application.Contracts;

public interface IFormDataAppService : IApplicationService
{
    Task<bool> SaveAsync(FormDataDto dto);

    Task<FormDataDto> GetAsync(string appId, string pageId, string id);

    Task<bool> DeleteAsync(string appId, string pageId, string id);
}