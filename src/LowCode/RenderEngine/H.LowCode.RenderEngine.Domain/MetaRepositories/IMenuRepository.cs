using H.LowCode.MetaSchema;

namespace H.LowCode.RenderEngine.Domain.Repositories;

public interface IMenuRepository
{
    Task<MenuSchema> GetAsync(string appId, string menuId);

    Task<IList<MenuSchema>> GetListAsync(string appId);
}