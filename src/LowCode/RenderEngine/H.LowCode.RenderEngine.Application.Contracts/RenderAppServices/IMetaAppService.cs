using H.Abp.Application.Contracts;
using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.RenderEngine;
using H.Util.Base;

namespace H.LowCode.RenderEngine.Application.Contracts;

public interface IMetaAppService : IAppService
{
    Task<BaseOutput<IList<MenuSchema>>> GetMenusAsync(string appId);

    Task<BaseOutput<PageSchema>> GetPageAsync(string appId, string pageId);

    Task<BaseOutput<PageSchema>> GetPageWithDefineAsync(string appId, string pageId);
}