using H.Extensions.System;
using H.LowCode.MetaSchema;
using H.LowCode.RenderEngine.Application.Contracts;
using Microsoft.Extensions.Options;
using System.Text;
using System.Xml.Linq;

namespace H.LowCode.RenderEngine.Application;

public class RenderEngineAppService : IRenderEngineAppService
{
    private static string metaBaseDir;
    private static string appFileName_Format = @"{0}\{1}\{2}.json";
    private static string menuFileName_Format = @"{0}\{1}\menu\{2}.json";
    private static string pageFileName_Format = @"{0}\{1}\page\{2}.json";

    public RenderEngineAppService(IOptions<MetaOption> metaOption)
    {
        metaBaseDir = metaOption.Value.FileBasePath;
    }

    public async Task<IList<MenuSchema>> GetMenusAsync(string appId)
    {
        await Task.Delay(1);
        IList<MenuSchema> list = [];

        var menuFilePath = Path.Combine(metaBaseDir, appId, "menu");
        if (!Directory.Exists(menuFilePath))
            return list;

        var files = Directory.GetFiles(menuFilePath);
        foreach (var fileName in files)
        {
            var menuSchemaJson = ReadAllText(fileName);
            var menuSchema = menuSchemaJson.FromJson<MenuSchema>();

            list.Add(menuSchema);
        }

        list = BuildTreeMenus(list);

        return list;
    }

    public async Task<string> GetPageAsync(string appId, string pageId)
    {
        await Task.Delay(1);
        string fileName = string.Format(pageFileName_Format, metaBaseDir, appId, pageId);

        return ReadAllText(fileName);
    }

    private static string ReadAllText(string fileName)
    {
        if (!File.Exists(fileName))
            throw new FileNotFoundException(fileName);

        return File.ReadAllText(fileName, Encoding.UTF8);
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
