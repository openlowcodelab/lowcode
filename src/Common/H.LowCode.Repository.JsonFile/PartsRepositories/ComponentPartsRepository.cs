using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Model;
using H.LowCode.Domain.Repositories;
using H.LowCode.MetaSchema;
using H.LowCode.PartsMetaSchema;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Repository.JsonFile;

public class ComponentPartsRepository : PartsFileRepositoryBase, IComponentPartsRepository
{
    private static string componentPartsFileName_Format = @"{0}\componentParts\{1}\{2}.json";

    public ComponentPartsRepository(IOptions<MetaOption> metaOption) : base(metaOption)
    {

    }

    public async Task<List<ComponentPartsListModel>> GetListAsync(string libraryId)
    {
        await Task.Delay(1);
        List<ComponentPartsListModel> list = [];

        var componentPartsFolder = Path.Combine(_metaBaseDir, "componentParts", libraryId);
        if (!Directory.Exists(componentPartsFolder))
            return list;

        var files = Directory.GetFiles(componentPartsFolder);
        foreach (var fileName in files)
        {
            var componentPartsSchemaJson = ReadAllText(fileName);
            if (string.IsNullOrWhiteSpace(componentPartsSchemaJson))
                continue;

            var componentPartsSchema = componentPartsSchemaJson.FromJson<ComponentPartsSchema>();
            if (componentPartsSchema == null)
                continue;

            ComponentPartsListModel model = new()
            {
                LibraryId = componentPartsSchema.LibraryId,
                ComponentId = componentPartsSchema.ComponentId,
                ComponentName = componentPartsSchema.ComponentName,
                ComponentType = componentPartsSchema.ComponentType,
                IsContainer = componentPartsSchema.IsContainer,
                Label = componentPartsSchema.Label,
                Order = componentPartsSchema.Order,
                ModifiedTime = componentPartsSchema.ModifiedTime,
                PublishStatus = componentPartsSchema.PublishStatus
            };

            list.Add(model);
        }

        //排序
        list = list.OrderBy(t => t.Order).ToList();

        return list;
    }

    public async Task<List<ComponentPartsSchema>> GetAllComponentsAsync(string libraryId)
    {
        await Task.Delay(1);
        List<ComponentPartsSchema> list = [];

        var componentPartsFolder = Path.Combine(_metaBaseDir, "componentParts", libraryId);
        if (!Directory.Exists(componentPartsFolder))
            return list;

        var files = Directory.GetFiles(componentPartsFolder);
        foreach (var fileName in files)
        {
            var componentPartsSchemaJson = ReadAllText(fileName);
            if (string.IsNullOrWhiteSpace(componentPartsSchemaJson))
                continue;

            var componentPartsSchema = componentPartsSchemaJson.FromJson<ComponentPartsSchema>();

            if (componentPartsSchema == null || componentPartsSchema.PublishStatus != 1)
                continue;

            list.Add(componentPartsSchema);
        }

        //排序
        list = list.OrderBy(t => t.Order).ToList();

        return list;
    }

    public async Task<ComponentPartsSchema> GetByIdAsync(string libraryId, string componentId)
    {
        await Task.Delay(1);
        string fileName = string.Format(componentPartsFileName_Format, _metaBaseDir, libraryId, componentId);

        var componentPartsSchemaJson = ReadAllText(fileName);
        return componentPartsSchemaJson.FromJson<ComponentPartsSchema>();
    }

    public async Task<bool> SaveAsync(ComponentPartsSchema componentParts)
    {
        ArgumentNullException.ThrowIfNull(componentParts);
        ArgumentException.ThrowIfNullOrEmpty(componentParts.Id);

        componentParts.ModifiedTime = DateTime.UtcNow;

        await Task.Delay(1);
        string fileName = string.Format(componentPartsFileName_Format, _metaBaseDir, componentParts.LibraryId, componentParts.ComponentId);

        string fileDirectory = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        File.WriteAllText(fileName, componentParts.ToJson(), Encoding.UTF8);
        return true;
    }

    public async Task<bool> DeleteAsync(string libraryId, string componentId)
    {
        await Task.Delay(1);

        string fileName = string.Format(componentPartsFileName_Format, _metaBaseDir, libraryId, componentId);
        if (!File.Exists(fileName))
            return false;

        File.Delete(fileName);
        return true;
    }
}
