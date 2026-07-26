using H.LowCode.MetaSchema.DesignEngine;

namespace H.LowCode.DesignEngine.Domain.Repositories;

public interface IAppPublishRepository
{
    Task<List<AppPublishRecordSchema>> GetListAsync(string appId);

    Task SaveAsync(AppPublishRecordSchema record);
}
