using H.LowCode.Configuration;
using H.LowCode.Domain.Repositories;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;

namespace H.LowCode.Repository.JsonFile;

public class MenuFileRepository : FileRepositoryBase, IMenuRepository
{
    private static string menuFileName_Format = @"{0}\{1}\menu\{2}.json";

    public MenuFileRepository(IOptions<MetaOption> metaOption) : base(metaOption)
    {

    }

    public async Task<MenuSchema> GetAsync(string appId, string menuId)
    {
        await Task.Delay(1);

        var fileName = string.Format(menuFileName_Format, _metaBaseDir, appId, menuId);
        if (!File.Exists(fileName))
            return null;

        var menuSchemaJson = ReadAllText(fileName);
        var menuSchema = menuSchemaJson.FromJson<MenuSchema>();

        return menuSchema;
    }

    public async Task<IList<MenuSchema>> GetListAsync(string appId)
    {
        await Task.Delay(1);
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

        return list;
    }

    public async Task SaveAsync(MenuSchema menuSchema)
    {
        ArgumentNullException.ThrowIfNull(menuSchema);
        ArgumentException.ThrowIfNullOrEmpty(menuSchema.Id);

        menuSchema.ModifiedTime = DateTime.UtcNow;

        await Task.Delay(1);
        string fileName = string.Format(menuFileName_Format, _metaBaseDir, menuSchema.AppId, menuSchema.Id);

        string fileDirectory = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        File.WriteAllText(fileName, menuSchema.ToJson(), Encoding.UTF8);
    }

    public async Task DeleteAsync(string appId, string menuId)
    {
        await Task.Delay(1);

        var fileName = string.Format(menuFileName_Format, _metaBaseDir, appId, menuId);
        if (!File.Exists(fileName))
            return;

        IList<MenuSchema> list = [];

        var menuFolder = Path.Combine(_metaBaseDir, appId, "menu");
        if (!Directory.Exists(menuFolder))
            return;

        var files = Directory.GetFiles(menuFolder);
        foreach (var file in files)
        {
            var menuSchemaJson = ReadAllText(file);
            var menuSchema = menuSchemaJson.FromJson<MenuSchema>();

            list.Add(menuSchema);
        }

        if (list.Any(t => t.ParentId == menuId))
            throw new InvalidOperationException("存在子节点, 不允许删除!");

        File.Delete(fileName);
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
