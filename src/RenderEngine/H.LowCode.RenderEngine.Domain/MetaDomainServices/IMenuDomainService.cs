using H.LowCode.MetaSchema;
using Volo.Abp.Domain.Services;

namespace H.LowCode.RenderEngine.Domain;

public interface IMenuDomainService : IDomainService
{
    Task<MenuSchema> GetAsync(string appId, string menuId);

    Task<IList<MenuSchema>> GetListAsync(string appId);
}