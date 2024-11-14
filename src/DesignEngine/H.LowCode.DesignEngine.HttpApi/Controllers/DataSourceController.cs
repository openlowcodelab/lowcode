using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.DesignEngine.HttpApi;

public class DataSourceController : DesignEngineControllerBase
{
    private IDataSourceAppService _appService;

    public DataSourceController(IDataSourceAppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    public async Task<IList<DataSourceListModel>> GetListAsync(string appId, [FromQuery] DataSourceInput input)
    {
        return await _appService.GetListAsync(appId, input);
    }

    [HttpGet]
    public async Task<DataSourceSchema> GetAsync(string appId, string id)
    {
        return await _appService.GetByIdAsync(appId, id);
    }

    [HttpPost]
    public async Task SaveAsync(string appId, DataSourceSchema dataSourceSchema)
    {
        await _appService.SaveAsync(appId, dataSourceSchema);
    }

    [HttpGet]
    public async Task DeleteAsync(string appId, string menuId)
    {
        await _appService.DeleteAsync(appId, menuId);
    }
}
