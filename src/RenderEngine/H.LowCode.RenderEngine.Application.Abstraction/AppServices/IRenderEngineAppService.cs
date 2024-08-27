using H.LowCode.MetaSchema;

namespace H.LowCode.RenderEngine.Application.Abstraction
{
    public interface IRenderEngineAppService
    {
        Task<IList<MenuSchema>> GetMenusAsync(string appId);

        Task<string> GetPageAsync(string appId, string pageId);
    }
}