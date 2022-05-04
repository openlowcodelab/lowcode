using H.LowCode.Designer.Repository.Abstraction;
using Newtonsoft.Json.Schema;

namespace H.LowCode.Designer.Repository.MongoDB.Repositories
{
    public class DesignerMongoDBRepository : IDesignerRepository
    {
        public void SaveMetadata(string jsonSchema)
        {
            throw new NotImplementedException();
        }

        public void SaveMetadata2(JSchema jsonSchema)
        {
            throw new NotImplementedException();
        }
    }
}