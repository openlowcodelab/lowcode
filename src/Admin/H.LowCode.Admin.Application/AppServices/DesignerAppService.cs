using H.LowCode.Admin.Application.Abstraction.AppServices;
using System.Text;

namespace H.LowCode.Admin.Application.AppServices
{
    public class DesignerAppService : IDesignerAppService
    {
        public DesignerAppService()
        {
        }

        public void SaveMetadata(string jsonSchema)
        {
            string filePath = @"D:\temp.json";
            File.WriteAllText(filePath, jsonSchema, Encoding.UTF8);
        }
    }
}
