using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Model;
using H.LowCode.Domain.Repositories;
using H.LowCode.MetaSchema;
using H.LowCode.PartsMetaSchema;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Repository.JsonFile;

public class ComponentLibraryRepository : PartsFileRepositoryBase, IComponentLibraryRepository
{
    private static string componentLibraryFileName_Format = @"{0}\componentParts\{1}\{2}.json";

    public ComponentLibraryRepository(IOptions<MetaOption> metaOption) : base(metaOption)
    {

    }

    public async Task<List<ComponentLibrarySchema>> GetListAsync()
    {
        List<ComponentLibrarySchema> list = [];
        var componentPartsFolder = Path.Combine(_metaBaseDir, "componentParts");
        if (!Directory.Exists(componentPartsFolder))
            return list;

        var directories = Directory.GetDirectories(componentPartsFolder);
        foreach (var directory in directories)
        {
            DirectoryInfo dirInfo = new(directory);
            var fileName = string.Format(componentLibraryFileName_Format, _metaBaseDir, dirInfo.Name, dirInfo.Name);

            if (!File.Exists(fileName))
                continue;

            var librarySchemaJson = ReadAllText(fileName);
            var librarySchema = librarySchemaJson.FromJson<ComponentLibrarySchema>();
            list.Add(librarySchema);
        }
        return await Task.FromResult(list);
    }

    public async Task<ComponentLibrarySchema> GetByIdAsync(string libraryId)
    {
        string fileName = string.Format(componentLibraryFileName_Format, _metaBaseDir, libraryId, libraryId);

        var componentLibrarySchemaJson = ReadAllText(fileName);
        var componentLibrary = componentLibrarySchemaJson.FromJson<ComponentLibrarySchema>();
        return await Task.FromResult(componentLibrary);
    }

    public async Task<bool> SaveAsync(ComponentLibrarySchema componentLibrary)
    {
        ArgumentNullException.ThrowIfNull(componentLibrary);
        ArgumentException.ThrowIfNullOrEmpty(componentLibrary.LibraryId);

        componentLibrary.ModifiedTime = DateTime.UtcNow;

        string fileName = string.Format(componentLibraryFileName_Format, _metaBaseDir, componentLibrary.LibraryId, componentLibrary.LibraryId);

        string fileDirectory = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        File.WriteAllText(fileName, componentLibrary.ToJson(), Encoding.UTF8);
        return await Task.FromResult(true);
    }

    public async Task<bool> DeleteAsync(string libraryId)
    {
        string fileName = string.Format(componentLibraryFileName_Format, _metaBaseDir, libraryId, libraryId);
        if (!File.Exists(fileName))
            return false;

        File.Delete(fileName);
        return await Task.FromResult(true);
    }
}
