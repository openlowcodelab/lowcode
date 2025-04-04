﻿using H.LowCode.RenderEngine.Domain.Repositories;
using H.LowCode.MetaSchema;
using Volo.Abp.Domain.Services;

namespace H.LowCode.RenderEngine.Domain;

public class DataSourceDomainService : DomainService, IDataSourceDomainService
{
    private readonly IDataSourceRepository _repository;

    public DataSourceDomainService(IDataSourceRepository repository)
    {
        _repository = repository;
    }

    public async Task<IList<DataSourceSchema>> GetListAsync(string appId)
    {
        return await _repository.GetListAsync(appId);
    }

    public async Task<IList<DataSourceSchema>> GetAllApisAsync(string appId)
    {
        var list = await GetListAsync(appId);
        return list.Where(t => t.DataSourceType == ComponentDataSourceTypeEnum.API).ToList();
    }

    public IEnumerable<DataSourceSchema> GetAllEntities(string appId)
    {
        var list = GetListAsync(appId).Result;
        return list.Where(t => t.DataSourceType == ComponentDataSourceTypeEnum.DB).ToList();
    }

    public async Task<DataSourceSchema> GetAsync(string appId, string id)
    {
        return await _repository.GetAsync(appId, id);
    }
}