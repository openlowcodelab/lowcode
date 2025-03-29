﻿using H.LowCode.MetaSchema;
using Volo.Abp.Domain.Repositories;

namespace H.LowCode.DesignEngine.Domain.Repositories;

public interface IMenuRepository
{
    Task<MenuSchema> GetAsync(string appId, string menuId);

    Task<IList<MenuSchema>> GetListAsync(string appId);

    Task SaveAsync(MenuSchema menuSchema);

    Task DeleteAsync(string appId, string menuId);
}