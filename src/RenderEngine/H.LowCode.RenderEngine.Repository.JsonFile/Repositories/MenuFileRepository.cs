using H.LowCode.Configuration;
using H.LowCode.RenderEngine.Domain.Repositories;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;

namespace H.LowCode.RenderEngine.Repository.JsonFile;

public class MenuFileRepository : FileRepositoryBase, IMenuRepository
{
    private static string menuFileName_Format = @"{0}\{1}\menu\{2}.json";

    public MenuFileRepository(IOptions<MetaOption> metaOption) : base(metaOption)
    {

    }

    public async Task<MenuSchema> GetAsync(string appId, string menuId)
    {
        var fileName = string.Format(menuFileName_Format, _metaBaseDir, appId, menuId);
        if (!File.Exists(fileName))
            return null;

        var menuSchemaJson = ReadAllText(fileName);
        var menuSchema = menuSchemaJson.FromJson<MenuSchema>();

        return await Task.FromResult(menuSchema);
    }

    public async Task<IList<MenuSchema>> GetListAsync(string appId)
    {
        IList<MenuSchema> list = [];

        var menuFolder = Path.Combine(_metaBaseDir, appId, "menu");
        if (!Directory.Exists(menuFolder))
            return list;

        var files = Directory.GetFiles(menuFolder);
        foreach (var fileName in files)
        {
            var menuSchemaJson = ReadAllText(fileName);
            var menuSchema = menuSchemaJson.FromJson<MenuSchema>();

            list.Add(menuSchema);
        }

        list = BuildTreeMenus(list);

        return await Task.FromResult(list);
    }

    private static IList<MenuSchema> BuildTreeMenus(IList<MenuSchema> menus)
    {
        var treeMenus = new List<MenuSchema>();

        var menuDic = new Dictionary<string, MenuSchema>();
        foreach (var m in menus)
        {
            if (!menuDic.ContainsKey(m.Id))
                menuDic[m.Id] = m;
        }

        foreach (var menu in menus)
        {
            if (menu.ParentId.IsNullOrEmpty())
                treeMenus.Add(menu);
            else
            {
                if (menuDic.TryGetValue(menu.ParentId, out var parentMenu))
                {
                    parentMenu.Childrens.Add(menu);
                    parentMenu.Childrens = parentMenu.Childrens.OrderBy(t => t.Order).ToList();
                }
                else
                    throw new KeyNotFoundException($"ParentId not found: {menu.ParentId}");
            }
        }

        //排序
        treeMenus = treeMenus.OrderBy(t => t.Order).ToList();

        return treeMenus;
    }
}
