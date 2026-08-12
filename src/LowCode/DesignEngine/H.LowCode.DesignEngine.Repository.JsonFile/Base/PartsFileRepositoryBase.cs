using H.LowCode.Configuration;
using Microsoft.Extensions.Options;
using System.Text;

namespace H.LowCode.DesignEngine.Repository.JsonFile;

public abstract class PartsFileRepositoryBase
{
    public bool? IsChangeTrackingEnabled { get; set; }

    protected static string? _metaBaseDir;

    public PartsFileRepositoryBase(IOptions<MetaOption> metaOption)
    {
        _metaBaseDir = metaOption.Value.PartsFilePath;
        IsChangeTrackingEnabled = false;
    }

    protected static string? ReadAllText(string fileName)
    {
        if (!File.Exists(fileName))
            return null;

        return File.ReadAllText(fileName, Encoding.UTF8);
    }
}
