using H.LowCode.MetaSchema;

namespace H.LowCode.Domain.Repositories;

public interface IMenuRepository
{
    Task<MenuSchema> GetAsync(string appId, string menuId);

    Task<IList<MenuSchema>> GetListAsync(string appId);

    Task SaveAsync(MenuSchema menuSchema);

    Task DeleteAsync(string appId, string menuId);
}