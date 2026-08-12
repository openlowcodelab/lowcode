using H.LowCode.Configuration;
using H.LowCode.MetaSchema;
using H.LowCode.RenderEngine.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace H.LowCode.RenderEngine.Repository.RemoteService;

public class MenuRemoteServiceRepository : RemoteServiceRepositoryBase, IMenuRepository
{
    public MenuRemoteServiceRepository(IOptions<MetaOption> metaOption)
    {

    }

    public async Task<MenuSchema> GetAsync(string appId, string menuId)
    {
        throw new NotImplementedException();
    }

    public async Task<IList<MenuSchema>> GetListAsync(string appId)
    {
        throw new NotImplementedException();
    }
}
