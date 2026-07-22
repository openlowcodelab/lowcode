using H.LowCode.MetaSchema.DesignEngine;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IAppPublishAppService : IApplicationService
{
    Task<List<AppPublishRecordSchema>> GetRecordsAsync(string appId);

    Task<AppPublishRecordSchema> PublishAsync(string appId, string version, string description);

    Task<bool> RollbackAsync(string appId, string recordId);
}
