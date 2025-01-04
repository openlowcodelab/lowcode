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
        List<ComponentPartsListModel> list = [];

        var componentPartsFolder = Path.Combine(_metaBaseDir, "componentParts", libraryId);
        if (!Directory.Exists(componentPartsFolder))
            return list;

        var files = Directory.GetFiles(componentPartsFolder);
        foreach (var fileName in files)
        {
            if (fileName.EndsWith($"{libraryId}.json"))
                continue;

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

        return await Task.FromResult(list);
    }

    public async Task<List<ComponentPartsSchema>> GetAllComponentsAsync(string libraryId)
    {
        List<ComponentPartsSchema> list = [];

        var componentPartsFolder = Path.Combine(_metaBaseDir, "componentParts", libraryId);
        if (!Directory.Exists(componentPartsFolder))
            return list;

        var files = Directory.GetFiles(componentPartsFolder);
        foreach (var fileName in files)
        {
            if (fileName.EndsWith($"{libraryId}.json"))
                continue;

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

        return await Task.FromResult(list);
    }

    public async Task<ComponentPartsSchema> GetByIdAsync(string libraryId, string componentId)
    {
        string fileName = string.Format(componentPartsFileName_Format, _metaBaseDir, libraryId, componentId);

        var componentPartsSchemaJson = ReadAllText(fileName);
        var componentParts = componentPartsSchemaJson.FromJson<ComponentPartsSchema>();
        return await Task.FromResult(componentParts);
    }

    public async Task<bool> SaveAsync(ComponentPartsSchema componentParts)
    {
        ArgumentNullException.ThrowIfNull(componentParts);
        ArgumentException.ThrowIfNullOrEmpty(componentParts.Id);

        componentParts.ModifiedTime = DateTime.UtcNow;

        // 组件定义中使用 DefaultFullTypeName 即可，无需保存 FullTypeName
        // 组件保存 json 文件时，强制设置 FullTypeName 为 null
        if(componentParts.Fragment != null)
            componentParts.Fragment.FullTypeName = null;

        string fileName = string.Format(componentPartsFileName_Format, _metaBaseDir, componentParts.LibraryId, componentParts.ComponentId);

        string fileDirectory = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        File.WriteAllText(fileName, componentParts.ToJson(), Encoding.UTF8);
        return await Task.FromResult(true);
    }

    public async Task<bool> DeleteAsync(string libraryId, string componentId)
    {
        string fileName = string.Format(componentPartsFileName_Format, _metaBaseDir, libraryId, componentId);
        if (!File.Exists(fileName))
            return false;

        File.Delete(fileName);
        return await Task.FromResult(true);
    }
}
