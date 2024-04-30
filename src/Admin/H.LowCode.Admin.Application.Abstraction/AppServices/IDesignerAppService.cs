using System;

namespace H.LowCode.Admin.Application.Abstraction.AppServices
{
    public interface IDesignerAppService
    {
        void SaveMetadata(string jsonSchema);
    }
}