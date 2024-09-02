using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IDesignEngineAppService
{
    Task<MenuSchema> GetMenuAsync(string appId, string menuId);

    Task<IList<MenuSchema>> GetMenusAsync(string appId);

    Task SaveMenuAsync(MenuSchema menuSchema);

    Task DeleteMenuAsync(string appId, string menuId);

    Task<List<PageListModel>> GetPagesAsync(string appId);

    Task<string> GetPageAsync(string appId, string pageId);

    Task SavePageAsync(PageSchema pageSchema);
}