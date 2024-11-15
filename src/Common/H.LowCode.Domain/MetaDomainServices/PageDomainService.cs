using H.LowCode.DesignEngine.Model;
using H.LowCode.Domain.Repositories;
using H.LowCode.MetaSchema;
using Volo.Abp.DependencyInjection;

namespace H.LowCode.Domain;

public class PageDomainService : IPageDomainService, IScopedDependency
{
    private readonly IPageRepository _repository;

    public PageDomainService(IPageRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PageListModel>> GetListAsync(string appId)
    {
        return await _repository.GetListAsync(appId);
    }

    public async Task<PageSchema> GetAsync(string appId, string pageId)
    {
        return await _repository.GetAsync(appId, pageId);
    }

    public async Task SaveAsync(PageSchema pageSchema)
    {
        await _repository.SaveAsync(pageSchema);
    }

    public async Task DeleteAsync(string appId, string pageId)
    {
        await _repository.DeleteAsync(appId, pageId);
    }
}