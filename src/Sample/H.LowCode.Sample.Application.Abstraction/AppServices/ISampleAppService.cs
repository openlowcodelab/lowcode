using System;

namespace H.LowCode.Sample.Application.Abstraction.AppServices
{
    public interface ISampleAppService
    {
        void SaveMetadata(string jsonSchema);
    }
}