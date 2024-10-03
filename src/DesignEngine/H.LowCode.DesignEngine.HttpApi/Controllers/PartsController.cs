using H.Extensions.System;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.MetaSchema;
using H.LowCode.Model;
using H.LowCode.PartsMetaSchema;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.DesignEngine.HttpApi;

public class PartsController : DesignEngineControllerBase
{
    public PartsController()
    {
    }

    #region AppParts
    [HttpGet]
    public async Task<IList<AppPartsSchema>> GetAppsAsync()
    {
        await Task.Delay(500);
        return [];
    }
    #endregion

    #region PageParts
    [HttpGet]
    public async Task<IList<PagePartsSchema>> GetPagesAsync()
    {
        await Task.Delay(500);
        return [];
    }

    [HttpPost]
    public async Task SavePageAsync(PagePartsSchema partsSchema)
    {
        await Task.CompletedTask;
    }

    [HttpGet]
    public async Task<PagePartsSchema> GetPageAsync(string partsId)
    {
        await Task.Delay(500);
        return new();
    }

    [HttpGet]
    public async Task DeletePageAsync(string partsId)
    {
        await Task.CompletedTask;
    }
    #endregion

    #region ComponentParts
    [HttpGet]
    public async Task<IList<ComponentPartsListModel>> GetComponentsAsync()
    {
        await Task.Delay(500);
        return [];
    }

    [HttpPost]
    public async Task SaveComponentAsync(ComponentPartsSchema partsSchema)
    {
        await Task.CompletedTask;
    }

    [HttpGet]
    public async Task<ComponentPartsSchema> GetComponentAsync(string partsId)
    {
        await Task.Delay(500);
        return new();
    }

    [HttpGet]
    public async Task DeleteComponentAsync(string partsId)
    {
        await Task.CompletedTask;
    }
    #endregion
}
