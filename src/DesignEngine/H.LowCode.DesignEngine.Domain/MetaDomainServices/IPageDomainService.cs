using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using Volo.Abp.Domain.Services;

namespace H.LowCode.DesignEngine.Domain;

public interface IPageDomainService : IDomainService
{
    Task<List<PageListModel>> GetListAsync(string appId);

    Task<PagePartsSchema> GetByIdAsync(string appId, string pageId);

    Task SaveAsync(PagePartsSchema pageSchema);

    Task DeleteAsync(string appId, string pageId);
}