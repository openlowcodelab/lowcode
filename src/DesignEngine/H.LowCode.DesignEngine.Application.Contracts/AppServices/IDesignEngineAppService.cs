using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IDesignEngineAppService
{
    Task<IList<AppSchema>> GetAppsAsync();

    Task SaveAppAsync(AppSchema appSchema);

    Task<AppSchema> GetAppAsync(string appId);

    Task<MenuSchema> GetMenuAsync(string appId, string menuId);

    Task<IList<MenuSchema>> GetMenusAsync(string appId);

    Task SaveMenuAsync(MenuSchema menuSchema);

    Task DeleteMenuAsync(string appId, string menuId);

    Task<List<PageListModel>> GetPagesAsync(string appId);

    Task<PageSchema> GetPageAsync(string appId, string pageId);

    Task SavePageAsync(PageSchema pageSchema);

    Task DeletePageAsync(string appId, string pageId);
}