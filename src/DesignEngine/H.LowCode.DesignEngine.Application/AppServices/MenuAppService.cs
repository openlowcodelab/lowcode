using H.Extensions.System;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.Domain;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class MenuAppService : ApplicationService, IMenuAppService
{
    private IMenuDomainService _domainService => LazyServiceProvider.GetRequiredService<IMenuDomainService>();

    public async Task<IList<MenuSchema>> GetListAsync(string appId)
    {
        return await _domainService.GetListAsync(appId);
    }

    public async Task<MenuSchema> GetByIdAsync(string appId, string menuId)
    {
        return await _domainService.GetAsync(appId, menuId);
    }

    public async Task<bool> SaveAsync(MenuSchema menuSchema)
    {
        ArgumentNullException.ThrowIfNull(menuSchema);
        ArgumentException.ThrowIfNullOrEmpty(menuSchema.Id);

        await _domainService.SaveAsync(menuSchema);
        return true;
    }

    public async Task<bool> DeleteAsync(string appId, string menuId)
    {
        await _domainService.DeleteAsync(appId, menuId);
        return true;
    }
}
