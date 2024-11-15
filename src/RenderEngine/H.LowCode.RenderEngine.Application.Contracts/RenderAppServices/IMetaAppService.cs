using H.LowCode.MetaSchema;
using Volo.Abp.Application.Services;

namespace H.LowCode.RenderEngine.Application.Contracts;

public interface IMetaAppService : IApplicationService
{
    Task<IList<MenuSchema>> GetMenusAsync(string appId);

    Task<PageSchema> GetPageAsync(string appId, string pageId);
}