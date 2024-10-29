using H.Extensions.System;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.Domain;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;

namespace H.LowCode.DesignEngine.Application;

public class MenuAppService : IMenuAppService
{
    private IMenuDomainService _domainService;

    public MenuAppService(IMenuDomainService domainService)
    {
        _domainService = domainService;
    }

    public async Task<IList<MenuSchema>> GetListAsync(string appId)
    {
        return await _domainService.GetListAsync(appId);
    }

    public async Task<MenuSchema> GetAsync(string appId, string menuId)
    {
        return await _domainService.GetAsync(appId, menuId);
    }

    public async Task SaveAsync(MenuSchema menuSchema)
    {
        ArgumentNullException.ThrowIfNull(menuSchema);
        ArgumentException.ThrowIfNullOrEmpty(menuSchema.Id);

        await _domainService.SaveAsync(menuSchema);
    }

    public async Task DeleteAsync(string appId, string menuId)
    {
        await _domainService.DeleteAsync(appId, menuId);
    }
}
