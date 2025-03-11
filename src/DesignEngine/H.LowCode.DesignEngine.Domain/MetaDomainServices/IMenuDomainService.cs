using H.LowCode.MetaSchema;
using Volo.Abp.Domain.Services;

namespace H.LowCode.DesignEngine.Domain;

public interface IMenuDomainService : IDomainService
{
    Task<MenuSchema> GetAsync(string appId, string menuId);

    Task<IList<MenuSchema>> GetListAsync(string appId);

    Task SaveAsync(MenuSchema menuSchema);

    Task DeleteAsync(string appId, string menuId);
}