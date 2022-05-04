using Newtonsoft.Json.Schema;

namespace H.LowCode.Designer.Repository.Abstraction
{
    public interface IDesignerRepository
    {
        void SaveMetadata(string jsonSchema);

        void SaveMetadata2(JSchema jsonSchema);
    }
}