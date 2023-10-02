using H.LowCode.Designer.Application.Abstraction.AppServices;
using System.Text;

namespace H.LowCode.Designer.Application.AppServices
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
