using H.Extensions.Json.Schema;
using H.LowCode.Designer.Repository.Abstraction;
using Newtonsoft.Json.Schema;
using System.Text;

namespace H.LowCode.Designer.Repository.JsonFile.Repositories
{
    public class DesignerJsonFileRepository : IDesignerRepository
    {
        public void SaveMetadata(string jsonSchema)
        {
            string filePath = @"D:\H\H.LowCode\_metadata\temp.json";
            File.WriteAllText(filePath, jsonSchema, Encoding.UTF8);
        }

        public void SaveMetadata2(JSchema jsonSchema)
        {
            string filePath = @"D:\temp.json";
            File.WriteAllText(filePath, jsonSchema.ToJson(), Encoding.UTF8);
        }
    }
}