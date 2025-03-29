﻿using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.MetaSchema;
using Volo.Abp.Domain.Services;

namespace H.LowCode.DesignEngine.Domain;

public class MenuDomainService : DomainService, IMenuDomainService
{
    private readonly IMenuRepository _repository;

    public MenuDomainService(IMenuRepository repository)
    {
        _repository = repository;
    }

    public async Task<IList<MenuSchema>> GetListAsync(string appId)
    {
        return await _repository.GetListAsync(appId);
    }

    public async Task<MenuSchema> GetAsync(string appId, string menuId)
    {
        return await _repository.GetAsync(appId, menuId);
    }

    public async Task SaveAsync(MenuSchema menuSchema)
    {
        await _repository.SaveAsync(menuSchema);
    }

    public async Task DeleteAsync(string appId, string menuId)
    {
        await _repository.DeleteAsync(appId, menuId);
    }
}