using System;

namespace H.LowCode.Designer.Repository
{
    public interface IDesignerDomainService
    {
        void SaveMetadata(string jsonSchema);
    }
}