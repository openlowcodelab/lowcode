using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;

namespace H.LowCode.DesignEngine.Domain.Repositories;

public interface IPageRepository
{
    Task<List<PageListModel>> GetListAsync(string appId);

    Task SaveAsync(PagePartsSchema pageSchema);

    Task<PagePartsSchema> GetByIdAsync(string appId, string pageId);

    Task DeleteAsync(string appId, string pageId);
}