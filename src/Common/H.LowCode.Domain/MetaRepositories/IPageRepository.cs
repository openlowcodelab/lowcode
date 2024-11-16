using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;
using Volo.Abp.Domain.Repositories;

namespace H.LowCode.Domain.Repositories;

public interface IPageRepository
{
    Task<List<PageListModel>> GetListAsync(string appId);

    Task<PageSchema> GetAsync(string appId, string pageId);

    Task SaveAsync(PageSchema pageSchema);

    Task DeleteAsync(string appId, string pageId);
}