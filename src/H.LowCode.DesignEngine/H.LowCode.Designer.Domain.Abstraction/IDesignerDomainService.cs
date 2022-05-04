using Newtonsoft.Json.Schema;

namespace H.LowCode.Designer.Repository
{
    public interface IDesignerDomainService
    {
        void SaveMetadata(string jsonSchema);

        void SaveMetadata2(JSchema jsonSchema);
    }
}