using Newtonsoft.Json.Schema;

namespace H.LowCode.Designer.Application.Abstraction.AppServices
{
    public interface IDesignerAppService
    {
        void SaveMetadata(string jsonSchema);

        void SaveMetadata2(JSchema jsonSchema);
    }
}