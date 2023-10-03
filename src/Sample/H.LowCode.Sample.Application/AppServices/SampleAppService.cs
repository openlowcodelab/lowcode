using H.LowCode.Sample.Application.Abstraction.AppServices;
using System.Text;

namespace H.LowCode.Sample.Application.AppServices
{
    public class SampleAppService : ISampleAppService
    {
        public SampleAppService()
        {
        }

        public void SaveMetadata(string jsonSchema)
        {
            string filePath = @"D:\temp.json";
            File.WriteAllText(filePath, jsonSchema, Encoding.UTF8);
        }
    }
}
