using H.Abp.Application.Contracts;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Base;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IAppPublishAppService : IAppService
{
    Task<BaseOutput<List<AppPublishRecordSchema>>> GetRecordsAsync(string appId);

    Task<BaseOutput<AppPublishRecordSchema>> PublishAsync(string appId, string version, string description);

    Task<BaseOutput<bool>> RollbackAsync(string appId, string recordId);
}
