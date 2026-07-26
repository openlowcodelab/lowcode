using H.Abstractions;
using H.LowCode.MetaSchema.DesignEngine;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IAppPublishAppService : IAppService
{
    Task<List<AppPublishRecordSchema>> GetRecordsAsync(string appId);

    Task<AppPublishRecordSchema> PublishAsync(string appId, string version, string description);

    Task<bool> RollbackAsync(string appId, string recordId);
}
