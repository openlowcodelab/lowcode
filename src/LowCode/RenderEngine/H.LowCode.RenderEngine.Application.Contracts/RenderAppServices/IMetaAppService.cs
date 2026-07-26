using H.Abp.Application.Contracts;
using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.RenderEngine;

namespace H.LowCode.RenderEngine.Application.Contracts;

public interface IMetaAppService : IAppService
{
    Task<IList<MenuSchema>> GetMenusAsync(string appId);

    Task<PageSchema> GetPageAsync(string appId, string pageId);

    Task<PageSchema> GetPageWithDefineAsync(string appId, string pageId);
}