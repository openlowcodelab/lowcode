using H.LowCode.MetaSchema;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IMenuAppService : IApplicationService
{
    Task<IList<MenuSchema>> GetListAsync(string appId);

    Task<MenuSchema> GetByIdAsync(string appId, string menuId);

    Task<bool> SaveAsync(MenuSchema menuSchema);

    Task<bool> DeleteAsync(string appId, string menuId);
}