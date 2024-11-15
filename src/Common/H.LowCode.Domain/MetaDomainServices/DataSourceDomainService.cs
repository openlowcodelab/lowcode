using H.LowCode.DesignEngine.Model;
using H.LowCode.Domain.Repositories;
using H.LowCode.MetaSchema;
using Volo.Abp.DependencyInjection;

namespace H.LowCode.Domain;

public class DataSourceDomainService : IDataSourceDomainService, IScopedDependency
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
        return list.Where(t => t.DataSourceType == DataSourceTypeEnum.API).ToList();
    }

    public IEnumerable<DataSourceSchema> GetAllEntities(string appId)
    {
        var list = GetListAsync(appId).Result;
        return list.Where(t => t.DataSourceType == DataSourceTypeEnum.Table).ToList();
    }

    public async Task<DataSourceSchema> GetAsync(string appId, string id)
    {
        return await _repository.GetAsync(appId, id);
    }

    public async Task SaveAsync(string appId, DataSourceSchema dataSourceSchema)
    {
        await _repository.SaveAsync(appId, dataSourceSchema);
    }

    public async Task DeleteAsync(string appId, string id)
    {
        await _repository.DeleteAsync(appId, id);
    }
}