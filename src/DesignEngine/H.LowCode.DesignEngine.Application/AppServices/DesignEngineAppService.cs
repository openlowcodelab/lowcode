using H.Extensions.System;
using H.LowCode.DesignEngine.Application.Abstraction;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;
using System.Collections.Generic;
using System.Text;

namespace H.LowCode.DesignEngine.Application
{
    public class DesignEngineAppService : IDesignEngineAppService
    {
        private static string baseDir = @"D:\H\code\my\H.LowCode\meta";
        private static string appFileName_Format = @"{0}\{1}\{2}.json";
        private static string menuFileName_Format = @"{0}\{1}\menu\{2}.json";
        private static string pageFileName_Format = @"{0}\{1}\page\{2}.json";

        public DesignEngineAppService()
        {

        }

        public async Task<IList<AppSchema>> GetAppsAsync()
        {
            await Task.Delay(1);
            List<AppSchema> appSchemas = [];

            if (Directory.Exists(baseDir) == false)
                return appSchemas;

            var directories = Directory.GetDirectories(baseDir);
            foreach (var directory in directories)
            {
                DirectoryInfo dirInfo = new(directory);
                var fileName = string.Format(appFileName_Format, baseDir, dirInfo.Name, dirInfo.Name);

                if (!File.Exists(fileName))
                    continue;

                var appSchemaJson = ReadAllText(fileName);
                var appSchema = appSchemaJson.FromJson<AppSchema>();
                appSchemas.Add(appSchema);
            }

            return appSchemas;
        }

        public async Task SaveAppAsync(AppSchema app)
        {
            await Task.Delay(1);
            string fileName = string.Format(appFileName_Format, baseDir, app.Id, app.Id);

            string fileDirectory = Path.GetDirectoryName(fileName);
            if (Directory.Exists(fileDirectory))
                throw new InvalidOperationException("应用已存在！");
            else
                Directory.CreateDirectory(fileDirectory);

            File.WriteAllText(fileName, app.ToJson(), Encoding.UTF8);
        }

        public async Task<MenuSchema> GetMenuAsync(string appId, string menuId)
        {
            await Task.Delay(1);

            var fileName = Path.Combine(baseDir, appId, "menu", $"{menuId}.json");
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

            var menuFilePath = Path.Combine(baseDir, appId, "menu");
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
            await Task.Delay(1);
            string fileName = string.Format(menuFileName_Format, baseDir, menuSchema.AppId, menuSchema.Id);

            string fileDirectory = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(fileDirectory))
                Directory.CreateDirectory(fileDirectory);

            File.WriteAllText(fileName, menuSchema.ToJson(), Encoding.UTF8);
        }

        public async Task DeleteMenuAsync(string appId, string menuId)
        {
            await Task.Delay(1);

            var fileName = Path.Combine(baseDir, appId, "menu", $"{menuId}.json");
            if (!File.Exists(fileName))
                return;

            IList<MenuSchema> list = [];

            var menuFilePath = Path.Combine(baseDir, appId, "menu");
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

            var pageFilePath = Path.Combine(baseDir, appId, "page");
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
                    PageName = pageSchema.Name
                };

                list.Add(model);
            }

            return list;
        }

        public async Task<string> GetPageAsync(string appId, string pageId)
        {
            await Task.Delay(1);
            string fileName = string.Format(pageFileName_Format, baseDir, appId, pageId);

            return ReadAllText(fileName);
        }

        public async Task SavePageAsync(PageSchema pageSchema)
        {
            await Task.Delay(1);
            string fileName = string.Format(pageFileName_Format, baseDir, pageSchema.AppId, pageSchema.Id);

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
}
