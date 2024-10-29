using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IPageAppService : IApplicationService
{
    Task<List<PageListModel>> GetListAsync(string appId);

    Task<PageSchema> GetAsync(string appId, string pageId);

    Task SaveAsync(PageSchema pageSchema);

    Task DeleteAsync(string appId, string pageId);
}