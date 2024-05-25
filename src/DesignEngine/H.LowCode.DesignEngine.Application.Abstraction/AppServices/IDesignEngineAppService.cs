using System;

namespace H.LowCode.DesignEngine.Application.Abstraction.AppServices
{
    public interface IDesignEngineAppService
    {
        void SavePageSchema(string appId, string pageId, string pageSchema);

        string GetPageSchema(string appId, string pageId);
    }
}