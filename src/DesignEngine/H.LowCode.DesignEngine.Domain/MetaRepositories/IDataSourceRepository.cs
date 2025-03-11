﻿using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;
using Volo.Abp.Domain.Repositories;

namespace H.LowCode.DesignEngine.Domain.Repositories;

public interface IDataSourceRepository
{
    Task<IList<DataSourceSchema>> GetListAsync(string appId);

    Task<DataSourceSchema> GetAsync(string appId, string id);

    Task SaveAsync(string appId, DataSourceSchema dataSourceSchema);

    Task DeleteAsync(string appId, string id);
}