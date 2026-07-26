using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.Extensions.Options;
using System.Text;

namespace H.LowCode.DesignEngine.Repository.JsonFile;

public class AppPublishFileRepository : FileRepositoryBase, IAppPublishRepository
{
    private static readonly string publishFileName_Format = @"{0}\{1}\publish\{2}.json";

    public AppPublishFileRepository(IOptions<MetaOption> metaOption) : base(metaOption)
    {
    }

    public async Task<List<AppPublishRecordSchema>> GetListAsync(string appId)
    {
        List<AppPublishRecordSchema> list = [];

        var publishFolder = Path.Combine(_metaBaseDir, appId, "publish");
        if (!Directory.Exists(publishFolder))
            return list;

        var files = Directory.GetFiles(publishFolder, "*.json");
        foreach (var fileName in files)
        {
            var json = ReadAllText(fileName);
            var record = json.FromJson<AppPublishRecordSchema>();
            list.Add(record);
        }

        //按发布时间倒序
        list = list.OrderByDescending(t => t.PublishTime).ToList();

        return await Task.FromResult(list);
    }

    public Task SaveAsync(AppPublishRecordSchema record)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentException.ThrowIfNullOrEmpty(record.Id);
        ArgumentException.ThrowIfNullOrEmpty(record.AppId);

        string fileName = string.Format(publishFileName_Format, _metaBaseDir, record.AppId, record.Id);

        string fileDirectory = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        File.WriteAllText(fileName, record.ToJson(), Encoding.UTF8);
        return Task.CompletedTask;
    }
}
