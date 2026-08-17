using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.LowCode.Application.Contracts;

public interface IFormDataAppService : IAppService
{
    Task<BaseOutput<bool>> SaveAsync(FormDataDto dto);

    Task<BaseOutput<FormDataDto>> GetAsync(string appId, string pageId, string id);

    Task<BaseOutput<bool>> DeleteAsync(string appId, string pageId, string id);
}