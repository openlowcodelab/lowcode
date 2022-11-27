using H.LowCode.Designer.Application.Abstraction.AppServices;
using H.LowCode.Designer.Repository;
using System;

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
    }
}
