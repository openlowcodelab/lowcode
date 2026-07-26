using H.Abstractions;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IPageAppService : IAppService
{
    Task<List<PageListModel>> GetListAsync(string appId);

    Task<PagePartsSchema> GetByIdAsync(string appId, string pageId);

    Task<PagePartsSchema> GetByIdWithDefineAsync(string appId, string pageId);

    Task<bool> SaveAsync(PagePartsSchema pageSchema);

    Task<bool> DeleteAsync(string appId, string pageId);

    Task<ComponentPartsSchema> GetPageComponentAsync(string appId, string pageId, string componentId);
}