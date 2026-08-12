using H.LowCode.Configuration;
using Microsoft.Extensions.Options;
using System.Text;

namespace H.LowCode.RenderEngine.Repository.JsonFile;

public abstract class FileRepositoryBase
{
    public bool? IsChangeTrackingEnabled { get; set; }

    protected readonly string _metaBaseDir;

    public FileRepositoryBase(IOptions<MetaOption> metaOption)
    {
        // 获取绝对路径
        _metaBaseDir = Path.GetFullPath(metaOption.Value.AppsFilePath);
        IsChangeTrackingEnabled = false;
    }

    protected string ReadAllText(string fileName)
    {
        if (!File.Exists(fileName))
            throw new FileNotFoundException(fileName);

        return File.ReadAllText(fileName, Encoding.UTF8);
    }
}
