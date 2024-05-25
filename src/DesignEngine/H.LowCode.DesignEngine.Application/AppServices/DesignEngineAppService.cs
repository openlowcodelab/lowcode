using H.LowCode.DesignEngine.Application.Abstraction.AppServices;
using System.Text;

namespace H.LowCode.DesignEngine.Application.AppServices
{
    public class DesignEngineAppService : IDesignEngineAppService
    {
        public DesignEngineAppService()
        {
        }

        public void SavePageSchema(string appId, string pageId, string jsonSchema)
        {
            string filePath = $@"D:\{appId}\temp-{pageId}.json";
            File.WriteAllText(filePath, jsonSchema, Encoding.UTF8);
        }

        public string GetPageSchema(string appId, string pageId)
        {
            string filePath = $@"D:\{appId}\temp-{pageId}.json";
            return File.ReadAllText(filePath, Encoding.UTF8);
        }
    }
}
