using H.Abp.Application.Contracts;
using H.LowCode.MetaSchema;
using H.Util.Base;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IMenuAppService : IAppService
{
    Task<BaseOutput<IList<MenuSchema>>> GetListAsync(string appId);

    Task<BaseOutput<MenuSchema>> GetByIdAsync(string appId, string menuId);

    Task<BaseOutput<bool>> SaveAsync(MenuSchema menuSchema);

    Task<BaseOutput<bool>> DeleteAsync(string appId, string menuId);
}