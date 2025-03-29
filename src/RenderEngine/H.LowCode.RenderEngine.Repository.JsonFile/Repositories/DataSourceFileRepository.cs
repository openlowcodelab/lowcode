using H.LowCode.Configuration;
using H.LowCode.RenderEngine.Domain.Repositories;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;

namespace H.LowCode.RenderEngine.Repository.JsonFile;

public class DataSourceFileRepository : FileRepositoryBase, IDataSourceRepository
{
    private static string dataSourceName_Format = @"{0}\{1}\datasource\{2}.json";

    public DataSourceFileRepository(IOptions<MetaOption> metaOption) : base(metaOption)
    {
    }

    public async Task<IList<DataSourceSchema>> GetListAsync(string appId)
    {
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

        return await Task.FromResult(list);
    }

    public async Task<DataSourceSchema> GetAsync(string appId, string id)
    {
        string fileName = string.Format(dataSourceName_Format, _metaBaseDir, appId, id);

        var dataSourceSchemaJson = ReadAllText(fileName);
        var dataSource = dataSourceSchemaJson.FromJson<DataSourceSchema>();
        return await Task.FromResult(dataSource);
    }
}
