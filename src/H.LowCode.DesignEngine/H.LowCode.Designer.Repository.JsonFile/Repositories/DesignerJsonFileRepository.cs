using H.LowCode.Designer.Repository.Abstraction;
using System.Text;

namespace H.LowCode.Designer.Repository.JsonFile.Repositories
{
    public class DesignerJsonFileRepository : IDesignerRepository
    {
        public void SaveMetadata(string jsonSchema)
        {
            string filePath = @"D:\temp.json";
            File.WriteAllText(filePath, jsonSchema, Encoding.UTF8);
        }
    }
}