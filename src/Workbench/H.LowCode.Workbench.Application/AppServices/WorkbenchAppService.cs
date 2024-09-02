using H.Extensions.System;
using H.LowCode.MetaSchema;
using H.LowCode.Workbench.Application.Contracts;
using Microsoft.Extensions.Options;
using System.Text;

namespace H.LowCode.Workbench.Application;

public class WorkbenchAppService : IWorkbenchAppService
{
    private static string metaBaseDir;
    private static string appFileName_Format = @"{0}\{1}\{2}.json";

    public WorkbenchAppService(IOptions<MetaOption> metaOption)
    {
        metaBaseDir = metaOption.Value.FileBasePath;
    }

    public async Task<IList<AppSchema>> GetAppsAsync()
    {
        await Task.Delay(1);
        List<AppSchema> appSchemas = [];

        if (Directory.Exists(metaBaseDir) == false)
            return appSchemas;

        var directories = Directory.GetDirectories(metaBaseDir);
        foreach (var directory in directories)
        {
            DirectoryInfo dirInfo = new(directory);
            var fileName = string.Format(appFileName_Format, metaBaseDir, dirInfo.Name, dirInfo.Name);

            if (!File.Exists(fileName))
                continue;

            var appSchemaJson = ReadAllText(fileName);
            var appSchema = appSchemaJson.FromJson<AppSchema>();
            appSchemas.Add(appSchema);
        }

        return appSchemas;
    }

    public async Task SaveAppAsync(AppSchema app)
    {
        await Task.Delay(1);
        string fileName = string.Format(appFileName_Format, metaBaseDir, app.Id, app.Id);

        string fileDirectory = Path.GetDirectoryName(fileName);
        if (Directory.Exists(fileDirectory))
            throw new InvalidOperationException("应用已存在！");
        else
            Directory.CreateDirectory(fileDirectory);

        File.WriteAllText(fileName, app.ToJson(), Encoding.UTF8);
    }

    private static string ReadAllText(string fileName)
    {
        if (!File.Exists(fileName))
            throw new FileNotFoundException(fileName);

        return File.ReadAllText(fileName, Encoding.UTF8);
    }
}
