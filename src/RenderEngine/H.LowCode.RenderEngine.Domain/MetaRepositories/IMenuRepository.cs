using H.LowCode.MetaSchema;
using Volo.Abp.Domain.Repositories;

namespace H.LowCode.RenderEngine.Domain.Repositories;

public interface IMenuRepository
{
    Task<MenuSchema> GetAsync(string appId, string menuId);

    Task<IList<MenuSchema>> GetListAsync(string appId);
}