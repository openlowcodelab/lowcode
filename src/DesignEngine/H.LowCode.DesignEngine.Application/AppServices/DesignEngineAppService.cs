using H.LowCode.DesignEngine.Application.Abstraction.AppServices;
using System.Text;

namespace H.LowCode.DesignEngine.Application.AppServices
{
    public class DesignEngineAppService : IDesignEngineAppService
    {
        private string BaseDir = @"D:\H\code\OpenMetaApp\H.LowCode\meta\{0}\{1}.json";

        public DesignEngineAppService()
        {
        }

        public string GetPageSchema(string appId, string pageId)
        {
            string fileName = string.Format(BaseDir, appId, pageId);
            if (!File.Exists(fileName))
                throw new FileNotFoundException(fileName);

            return File.ReadAllText(fileName, Encoding.UTF8);
        }

        public void SavePageSchema(string appId, string pageId, string jsonSchema)
        {
            string fileName = string.Format(BaseDir, appId, pageId);

            string filePath = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            File.WriteAllText(fileName, jsonSchema, Encoding.UTF8);
        }
    }
}
