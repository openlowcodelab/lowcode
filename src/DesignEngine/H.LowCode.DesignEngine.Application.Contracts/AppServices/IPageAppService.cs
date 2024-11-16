using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IPageAppService : IApplicationService
{
    Task<List<PageListModel>> GetListAsync(string appId);

    Task<PageSchema> GetByIdAsync(string appId, string pageId);

    Task<bool> SaveAsync(PageSchema pageSchema);

    Task<bool> DeleteAsync(string appId, string pageId);
}