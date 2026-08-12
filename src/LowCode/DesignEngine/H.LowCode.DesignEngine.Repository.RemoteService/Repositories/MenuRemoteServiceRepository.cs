using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.Options;

namespace H.LowCode.DesignEngine.Repository.RemoteService;

public class MenuRemoteServiceRepository : RemoteServiceRepositoryBase, IMenuRepository
{
    public MenuRemoteServiceRepository(IOptions<MetaOption> metaOption)
    {

    }

    public async Task DeleteAsync(string appId, string menuId)
    {
        throw new NotImplementedException();
    }

    public async Task<MenuSchema> GetAsync(string appId, string menuId)
    {
        throw new NotImplementedException();
    }

    public async Task<IList<MenuSchema>> GetListAsync(string appId)
    {
        throw new NotImplementedException();
    }

    public async Task SaveAsync(MenuSchema menuSchema)
    {
        throw new NotImplementedException();
    }
}
