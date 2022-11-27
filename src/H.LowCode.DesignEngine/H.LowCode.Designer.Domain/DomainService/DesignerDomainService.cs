using H.LowCode.Designer.Repository;
using H.LowCode.Designer.Repository.Abstraction;

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
    }
}