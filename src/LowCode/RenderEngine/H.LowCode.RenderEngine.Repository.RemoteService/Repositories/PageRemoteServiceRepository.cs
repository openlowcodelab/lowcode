using H.LowCode.Configuration;
using H.LowCode.MetaSchema.RenderEngine;
using H.LowCode.RenderEngine.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace H.LowCode.RenderEngine.Repository.RemoteService;

public class PageRemoteServiceRepository : RemoteServiceRepositoryBase, IPageRepository
{
    public PageRemoteServiceRepository(IOptions<MetaOption> metaOption)
    {

    }

    public Task<PageSchema> GetAsync(string appId, string pageId)
    {
        throw new NotImplementedException();
    }
}
