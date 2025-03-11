using H.LowCode.RenderEngine.Domain.Repositories;
using H.LowCode.MetaSchema.RenderEngine;
using Volo.Abp.Domain.Services;

namespace H.LowCode.RenderEngine.Domain;

public class PageDomainService : DomainService, IPageDomainService
{
    private readonly IPageRepository _repository;

    public PageDomainService(IPageRepository repository)
    {
        _repository = repository;
    }

    async Task<PageSchema> IPageDomainService.GetAsync(string appId, string pageId)
    {
        return await _repository.GetAsync(appId, pageId);
    }
}