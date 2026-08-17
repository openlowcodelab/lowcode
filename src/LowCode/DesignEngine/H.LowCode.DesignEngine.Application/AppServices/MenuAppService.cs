using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.MetaSchema;
using H.Util.Base;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class MenuAppService : ApplicationService, IMenuAppService
{
    private IMenuRepository _repository => LazyServiceProvider.GetRequiredService<IMenuRepository>();

    public async Task<BaseOutput<IList<MenuSchema>>> GetListAsync(string appId)
    {
        return new(await _repository.GetListAsync(appId));
    }

    public async Task<BaseOutput<MenuSchema>> GetByIdAsync(string appId, string menuId)
    {
        return new(await _repository.GetAsync(appId, menuId));
    }

    public async Task<BaseOutput<bool>> SaveAsync(MenuSchema menuSchema)
    {
        ArgumentNullException.ThrowIfNull(menuSchema);
        ArgumentException.ThrowIfNullOrEmpty(menuSchema.Id);

        await _repository.SaveAsync(menuSchema);
        return new(true);
    }

    public async Task<BaseOutput<bool>> DeleteAsync(string appId, string menuId)
    {
        await _repository.DeleteAsync(appId, menuId);
        return new(true);
    }
}
