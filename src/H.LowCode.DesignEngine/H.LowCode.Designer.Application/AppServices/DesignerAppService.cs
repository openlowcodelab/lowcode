using H.LowCode.Designer.Application.Abstraction.AppService;
using H.LowCode.Designer.Repository;
using Newtonsoft.Json.Schema;

namespace H.LowCode.Designer.Application.AppServices
{
    public class DesignerAppService : IDesignerAppService
    {
        private IDesignerDomainService _designerDomainService;

        public DesignerAppService(IDesignerDomainService designerDomainService)
        {
            _designerDomainService = designerDomainService;
        }

        public void SaveMetadata(string jsonSchema)
        {
            _designerDomainService.SaveMetadata(jsonSchema);
        }

        public void SaveMetadata2(JSchema jsonSchema)
        {
            _designerDomainService.SaveMetadata2(jsonSchema);
        }
    }
}
