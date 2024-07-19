using H.LowCode.Admin.DTO;
using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.Application.Abstraction.AppServices
{
    public interface IDesignEngineAppService
    {
        IList<AppSchema> GetApps();

        void SaveApp(AppSchema app);

        IList<MenuSchema> GetMenus(string appId);

        void SaveMenu(MenuSchema menuSchema);

        List<PageListModel> GetPages(string appId);

        string GetPage(string appId, string pageId);

        void SavePage(PageSchema pageSchema);
    }
}