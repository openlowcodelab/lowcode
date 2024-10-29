using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;

namespace H.LowCode.Domain;

public interface IPageDomainService
{
    Task<List<PageListModel>> GetListAsync(string appId);

    Task<PageSchema> GetAsync(string appId, string pageId);

    Task SaveAsync(PageSchema pageSchema);

    Task DeleteAsync(string appId, string pageId);
}