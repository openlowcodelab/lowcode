using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;

namespace H.LowCode.DesignEngine.Repository.JsonFile;

public class AppFileRepository : FileRepositoryBase, IAppRepository
{
    private static string appFileName_Format = @"{0}\{1}\{2}.json";

    public AppFileRepository(IOptions<MetaOption> metaOption) : base(metaOption)
    {
        
    }

    public async Task<IList<AppPartsSchema>> GetListAsync()
    {
        List<AppPartsSchema> appSchemas = [];

        if (Directory.Exists(_metaBaseDir) == false)
            return appSchemas;

        var directories = Directory.GetDirectories(_metaBaseDir);
        foreach (var directory in directories)
        {
            DirectoryInfo dirInfo = new(directory);
            var fileName = string.Format(appFileName_Format, _metaBaseDir, dirInfo.Name, dirInfo.Name);

            if (!File.Exists(fileName))
                continue;

            var appSchemaJson = ReadAllText(fileName);
            var appSchema = appSchemaJson.FromJson<AppPartsSchema>();
            appSchemas.Add(appSchema);
        }

        return await Task.FromResult(appSchemas);
    }

    public async Task SaveAsync(AppPartsSchema appSchema)
    {
        ArgumentNullException.ThrowIfNull(appSchema);
        ArgumentException.ThrowIfNullOrEmpty(appSchema.Id);

        appSchema.ModifiedTime = DateTime.UtcNow;

        string fileName = string.Format(appFileName_Format, _metaBaseDir, appSchema.Id, appSchema.Id);

        string fileDirectory = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        File.WriteAllText(fileName, appSchema.ToJson(), Encoding.UTF8);
        await Task.CompletedTask;
    }

    public async Task<AppPartsSchema> GetAsync(string appId)
    {
        string fileName = string.Format(appFileName_Format, _metaBaseDir, appId, appId);

        var appSchemaJson = ReadAllText(fileName);
        var appSchema = appSchemaJson.FromJson<AppPartsSchema>();
        return await Task.FromResult(appSchema);
    }
}
