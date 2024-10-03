using H.Extensions.System;
using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;

namespace H.LowCode.DesignEngine.Application;

public class DataSourceAppService : IDataSourceAppService
{
    private static string metaBaseDir;
    private static string dataSourceName_Format = @"{0}\{1}\datasource\{2}.json";

    public DataSourceAppService(IOptions<MetaOption> metaOption)
    {
        metaBaseDir = metaOption.Value.AppsFilePath;
    }

    public async Task<IList<DataSourceListModel>> GetListAsync(string appId, DataSourceInput input)
    {
        await Task.Delay(1);
        List<DataSourceListModel> list = [];

        var dataSourceFolder = Path.Combine(metaBaseDir, appId, "datasource");
        if (!Directory.Exists(dataSourceFolder))
            return list;

        var files = Directory.GetFiles(dataSourceFolder);
        foreach (var fileName in files)
        {
            var dataSourceSchemaJson = ReadAllText(fileName);
            var dataSourceSchema = dataSourceSchemaJson.FromJson<DataSourceSchema>();

            if(dataSourceSchema.DataSourceType != input.DataSourceType)
                continue;

            DataSourceListModel model = new()
            {
                Id = dataSourceSchema.Id,
                Name = dataSourceSchema.Name,
                DisplayName = dataSourceSchema.DisplayName,
                Order = dataSourceSchema.Order,
                DataSourceType = dataSourceSchema.DataSourceType,
                PublishStatus = dataSourceSchema.PublishStatus,
                ModifiedTime = dataSourceSchema.ModifiedTime
            };

            list.Add(model);
        }

        //排序
        list = list.OrderBy(t => t.Order).ToList();

        return list;
    }

    public async Task<DataSourceSchema> GetAsync(string appId, string id)
    {
        await Task.Delay(1);
        string fileName = string.Format(dataSourceName_Format, metaBaseDir, appId, id);

        var dataSourceSchemaJson = ReadAllText(fileName);
        return dataSourceSchemaJson.FromJson<DataSourceSchema>();
    }

    public async Task SaveAsync(string appId, DataSourceSchema dataSourceSchema)
    {
        ArgumentNullException.ThrowIfNull(dataSourceSchema);
        ArgumentException.ThrowIfNullOrEmpty(dataSourceSchema.Id);

        dataSourceSchema.ModifiedTime = DateTime.UtcNow;

        await Task.Delay(1);
        string fileName = string.Format(dataSourceName_Format, metaBaseDir, appId, dataSourceSchema.Id);

        string fileDirectory = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        File.WriteAllText(fileName, dataSourceSchema.ToJson(), Encoding.UTF8);
    }

    public async Task DeleteAsync(string appId, string id)
    {
        await Task.Delay(1);

        string fileName = string.Format(dataSourceName_Format, metaBaseDir, appId, id);
        if (!File.Exists(fileName))
            return;

        var dataSourceFolder = Path.Combine(metaBaseDir, appId, "datasource");
        if (!Directory.Exists(dataSourceFolder))
            return;

        File.Delete(fileName);
    }

    private static string ReadAllText(string fileName)
    {
        if (!File.Exists(fileName))
            throw new FileNotFoundException(fileName);

        return File.ReadAllText(fileName, Encoding.UTF8);
    }
}
