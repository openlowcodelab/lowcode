using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class DataSourceAppService : ApplicationService, IDataSourceAppService
{
    private IDataSourceRepository _repository => LazyServiceProvider.GetRequiredService<IDataSourceRepository>();

    public async Task<IList<DataSourceListModel>> GetListAsync(string appId, DataSourceInput input)
    {
        var dataSources = await _repository.GetListAsync(appId);

        List<DataSourceListModel> list = new List<DataSourceListModel>();
        foreach (var dataSourceSchema in dataSources)
        {
            DataSourceListModel model = new()
            {
                Id = dataSourceSchema.Id,
                Name = dataSourceSchema.Name,
                DisplayName = dataSourceSchema.DisplayName,
                Extra = dataSourceSchema.DataSourceType == ComponentDataSourceTypeEnum.API ?
                        $"{dataSourceSchema.API.Method} {dataSourceSchema.API.Path}" : string.Empty,
                Order = dataSourceSchema.Order,
                DataSourceType = dataSourceSchema.DataSourceType,
                PublishStatus = dataSourceSchema.PublishStatus,
                ModifiedTime = dataSourceSchema.ModifiedTime
            };
            list.Add(model);
        }

        return list.Where(t => t.DataSourceType == input.DataSourceType).OrderBy(t => t.Order).ToList();
    }

    public async Task<DataSourceSchema> GetByIdAsync(string appId, string id)
    {
        return await _repository.GetAsync(appId, id);
    }

    public async Task<bool> SaveAsync(string appId, DataSourceSchema dataSourceSchema)
    {
        ArgumentNullException.ThrowIfNull(dataSourceSchema);
        ArgumentException.ThrowIfNullOrEmpty(dataSourceSchema.Id);

        await _repository.SaveAsync(appId, dataSourceSchema);
        return true;
    }

    public async Task<bool> DeleteAsync(string appId, string id)
    {
        await _repository.DeleteAsync(appId, id);
        return true;
    }
}
