using H.Abp.Application.Contracts;
using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IMenuAppService : IAppService
{
    Task<IList<MenuSchema>> GetListAsync(string appId);

    Task<MenuSchema> GetByIdAsync(string appId, string menuId);

    Task<bool> SaveAsync(MenuSchema menuSchema);

    Task<bool> DeleteAsync(string appId, string menuId);
}