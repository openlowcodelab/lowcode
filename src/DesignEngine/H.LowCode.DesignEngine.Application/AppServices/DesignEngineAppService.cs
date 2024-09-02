using H.Extensions.System;
using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;

namespace H.LowCode.DesignEngine.Application;

public class DesignEngineAppService : IDesignEngineAppService
{
    private static string metaBaseDir;
    private static string menuFileName_Format = @"{0}\{1}\menu\{2}.json";
    private static string pageFileName_Format = @"{0}\{1}\page\{2}.json";

    public DesignEngineAppService(IOptions<MetaOption> metaOption)
    {
        metaBaseDir = metaOption.Value.FileBasePath;
    }

    public async Task<MenuSchema> GetMenuAsync(string appId, string menuId)
    {
        await Task.Delay(1);

        var fileName = Path.Combine(metaBaseDir, appId, "menu", $"{menuId}.json");
        if (!File.Exists(fileName))
            return null;

        var menuSchemaJson = ReadAllText(fileName);
        var menuSchema = menuSchemaJson.FromJson<MenuSchema>();

        return menuSchema;
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

    public async Task SaveMenuAsync(MenuSchema menuSchema)
    {
        menuSchema.ModifiedTime = DateTime.UtcNow;

        await Task.Delay(1);
        string fileName = string.Format(menuFileName_Format, metaBaseDir, menuSchema.AppId, menuSchema.Id);

        string fileDirectory = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        File.WriteAllText(fileName, menuSchema.ToJson(), Encoding.UTF8);
    }

    public async Task DeleteMenuAsync(string appId, string menuId)
    {
        await Task.Delay(1);

        var fileName = Path.Combine(metaBaseDir, appId, "menu", $"{menuId}.json");
        if (!File.Exists(fileName))
            return;

        IList<MenuSchema> list = [];

        var menuFilePath = Path.Combine(metaBaseDir, appId, "menu");
        if (!Directory.Exists(menuFilePath))
            return;

        var files = Directory.GetFiles(menuFilePath);
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

    public async Task<List<PageListModel>> GetPagesAsync(string appId)
    {
        await Task.Delay(1);
        List<PageListModel> list = [];

        var pageFilePath = Path.Combine(metaBaseDir, appId, "page");
        if (!Directory.Exists(pageFilePath))
            return list;

        var files = Directory.GetFiles(pageFilePath);
        foreach (var fileName in files)
        {
            var pageSchemaJson = ReadAllText(fileName);
            var pageSchema = pageSchemaJson.FromJson<PageSchema>();

            PageListModel model = new()
            {
                PageId = pageSchema.Id,
                PageName = pageSchema.Name,
                Order = pageSchema.Order,
                PageType = pageSchema.PageType,
                PublishState = pageSchema.PublishState,
                ModifiedTime = pageSchema.ModifiedTime
            };

            list.Add(model);
        }

        //排序
        list = list.OrderBy(t => t.Order).ToList();

        return list;
    }

    public async Task<string> GetPageAsync(string appId, string pageId)
    {
        await Task.Delay(1);
        string fileName = string.Format(pageFileName_Format, metaBaseDir, appId, pageId);

        return ReadAllText(fileName);
    }

    public async Task SavePageAsync(PageSchema pageSchema)
    {
        pageSchema.ModifiedTime = DateTime.UtcNow;

        await Task.Delay(1);
        string fileName = string.Format(pageFileName_Format, metaBaseDir, pageSchema.AppId, pageSchema.Id);

        string fileDirectory = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        File.WriteAllText(fileName, pageSchema.ToJson(), Encoding.UTF8);
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
