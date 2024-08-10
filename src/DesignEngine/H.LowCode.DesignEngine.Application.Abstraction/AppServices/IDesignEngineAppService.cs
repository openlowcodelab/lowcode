using H.LowCode.Admin.DTO;
using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.Application.Abstraction
{
    public interface IDesignEngineAppService
    {
        Task<IList<AppSchema>> GetAppsAsync();

        Task SaveAppAsync(AppSchema app);

        Task<MenuSchema> GetMenuAsync(string appId, string menuId);

        Task<IList<MenuSchema>> GetMenusAsync(string appId);

        Task SaveMenuAsync(MenuSchema menuSchema);

        Task<List<PageListModel>> GetPagesAsync(string appId);

        Task<string> GetPageAsync(string appId, string pageId);

        Task SavePageAsync(PageSchema pageSchema);
    }
}