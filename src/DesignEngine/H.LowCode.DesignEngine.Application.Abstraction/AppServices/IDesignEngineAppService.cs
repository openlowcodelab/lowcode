using System;

namespace H.LowCode.DesignEngine.Application.Abstraction.AppServices
{
    public interface IDesignEngineAppService
    {
        string GetPageSchema(string appId, string pageId);

        void SavePageSchema(string appId, string pageId, string pageSchema);
    }
}