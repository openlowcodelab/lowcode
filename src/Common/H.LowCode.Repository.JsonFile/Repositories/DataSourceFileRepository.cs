using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Model;
using H.LowCode.Domain.Repositories;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;

namespace H.LowCode.Repository.JsonFile;

public class DataSourceFileRepository : FileRepositoryBase, IDataSourceRepository
{
    private static string dataSourceName_Format = @"{0}\{1}\datasource\{2}.json";

    public DataSourceFileRepository(IOptions<MetaOption> metaOption) : base(metaOption)
    {
    }

    public async Task<IList<DataSourceSchema>> GetListAsync(string appId)
    {
        await Task.Delay(1);
        List<DataSourceSchema> list = [];

        var dataSourceFolder = Path.Combine(_metaBaseDir, appId, "datasource");
        if (!Directory.Exists(dataSourceFolder))
            return list;

        var files = Directory.GetFiles(dataSourceFolder);
        foreach (var fileName in files)
        {
            var dataSourceSchemaJson = ReadAllText(fileName);
            var dataSourceSchema = dataSourceSchemaJson.FromJson<DataSourceSchema>();

            list.Add(dataSourceSchema);
        }

        //排序
        list = list.OrderBy(t => t.Order).ToList();

        return list;
    }

    public async Task<DataSourceSchema> GetAsync(string appId, string id)
    {
        await Task.Delay(1);
        string fileName = string.Format(dataSourceName_Format, _metaBaseDir, appId, id);

        var dataSourceSchemaJson = ReadAllText(fileName);
        return dataSourceSchemaJson.FromJson<DataSourceSchema>();
    }

    public async Task SaveAsync(string appId, DataSourceSchema dataSourceSchema)
    {
        ArgumentNullException.ThrowIfNull(dataSourceSchema);
        ArgumentException.ThrowIfNullOrEmpty(dataSourceSchema.Id);

        dataSourceSchema.ModifiedTime = DateTime.UtcNow;

        await Task.Delay(1);
        string fileName = string.Format(dataSourceName_Format, _metaBaseDir, appId, dataSourceSchema.Id);

        string fileDirectory = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        File.WriteAllText(fileName, dataSourceSchema.ToJson(), Encoding.UTF8);
    }

    public async Task DeleteAsync(string appId, string id)
    {
        await Task.Delay(1);

        string fileName = string.Format(dataSourceName_Format, _metaBaseDir, appId, id);
        if (!File.Exists(fileName))
            return;

        var dataSourceFolder = Path.Combine(_metaBaseDir, appId, "datasource");
        if (!Directory.Exists(dataSourceFolder))
            return;

        File.Delete(fileName);
    }
}
