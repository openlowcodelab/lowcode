using H.LowCode.MetaSchema;

namespace H.LowCode.RenderEngine.Application.Contracts;

public interface IRenderEngineAppService
{
    Task<IList<MenuSchema>> GetMenusAsync(string appId);

    Task<PageSchema> GetPageAsync(string appId, string pageId);
}