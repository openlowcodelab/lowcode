using H.Extensions.System;
using H.LowCode.MetaSchema;
using H.LowCode.RenderEngine.Application.Abstraction;
using System.Text;

namespace H.LowCode.RenderEngine.Application
{
    public class RenderEngineAppService : IRenderEngineAppService
    {
        private static string baseDir = @"D:\H\code\my\H.LowCode\meta";
        private static string appFileName_Format = @"{0}\{1}\{2}.json";
        private static string menuFileName_Format = @"{0}\{1}\menu\{2}.json";
        private static string pageFileName_Format = @"{0}\{1}\page\{2}.json";

        public RenderEngineAppService()
        {

        }

        public async Task<IList<MenuSchema>> GetMenusAsync(string appId)
        {
            await Task.Delay(1);
            List<MenuSchema> list = [];

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
    }
}
