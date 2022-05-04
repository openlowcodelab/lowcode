using H.LowCode.Designer.Repository;
using H.LowCode.Designer.Repository.Abstraction;
using Newtonsoft.Json.Schema;

namespace H.LowCode.Designer.Domain.DomainService
{
    public class DesignerDomainService : IDesignerDomainService
    {
        private IDesignerRepository _designerRepository;

        public DesignerDomainService(IDesignerRepository designerRepository)
        {
            _designerRepository = designerRepository;
        }

        public void SaveMetadata(string jsonSchema)
        {
            _designerRepository.SaveMetadata(jsonSchema);
        }

        public void SaveMetadata2(JSchema jsonSchema)
        {
            _designerRepository.SaveMetadata2(jsonSchema);
        }
    }
}