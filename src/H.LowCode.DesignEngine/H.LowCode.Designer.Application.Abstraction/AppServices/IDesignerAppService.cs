using System;

namespace H.LowCode.Designer.Application.Abstraction.AppServices
{
    public interface IDesignerAppService
    {
        void SaveMetadata(string jsonSchema);
    }
}