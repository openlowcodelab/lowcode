using H.LowCode.Configuration;
using Microsoft.Extensions.Options;
using System.Text;

namespace H.LowCode.DesignEngine.Repository.JsonFile;

public abstract class FileRepositoryBase
{
    public bool? IsChangeTrackingEnabled { get; set; }

    protected static string _metaBaseDir;

    public FileRepositoryBase(IOptions<MetaOption> metaOption)
    {
        _metaBaseDir = metaOption.Value.AppsFilePath;
        IsChangeTrackingEnabled = false;
    }

    protected static string ReadAllText(string fileName)
    {
        if (!File.Exists(fileName))
            throw new FileNotFoundException(fileName);

        return File.ReadAllText(fileName, Encoding.UTF8);
    }
}
