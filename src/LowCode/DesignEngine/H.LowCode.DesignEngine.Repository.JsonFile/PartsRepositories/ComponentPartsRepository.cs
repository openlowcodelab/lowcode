using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.Extensions.Options;
using System.Text;

namespace H.LowCode.DesignEngine.Repository.JsonFile;

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

        var files = Directory.GetFiles(componentPartsFolder, "*.json");
        foreach (var fileName in files)
        {
            if (fileName.EndsWith($"{libraryId}.json"))
                continue;

            var componentPartsSchemaJson = ReadAllText(fileName) ?? throw new FileNotFoundException(fileName);
            if (string.IsNullOrWhiteSpace(componentPartsSchemaJson))
                continue;

            var componentPartsSchema = componentPartsSchemaJson.FromJson<ComponentPartsSchema>();
            if (componentPartsSchema == null)
                continue;

            ComponentPartsListModel model = new()
            {
                LibraryId = componentPartsSchema.LibraryId,
                ComponentId = componentPartsSchema.PartsId,
                ComponentType = componentPartsSchema.ComponentType,
                IsContainer = componentPartsSchema.IsContainer,
                IsSupportDataSource = componentPartsSchema.IsSupportDataSource,
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

        var files = Directory.GetFiles(componentPartsFolder, "*.json");
        foreach (var fileName in files)
        {
            if (fileName.EndsWith($"{libraryId}.json"))
                continue;

            var componentPartsSchemaJson = ReadAllText(fileName) ?? throw new FileNotFoundException(fileName);
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

    public async Task<ComponentPartsSchema> GetByIdAsync(string libraryId, string partsId)
    {
        string fileName = string.Format(componentPartsFileName_Format, _metaBaseDir, libraryId, partsId);

        var componentPartsSchemaJson = ReadAllText(fileName) ?? throw new FileNotFoundException(fileName);
        var componentParts = componentPartsSchemaJson.FromJson<ComponentPartsSchema>();
        return await Task.FromResult(componentParts);
    }

    public async Task<bool> SaveAsync(ComponentPartsSchema componentParts)
    {
        ArgumentNullException.ThrowIfNull(componentParts);
        ArgumentException.ThrowIfNullOrEmpty(componentParts.Id);

        componentParts.ModifiedTime = DateTime.UtcNow;

        // 组件定义中使用 DefaultTypeName 即可，无需保存 TypeName
        // 组件保存 json 文件时，强制设置 TypeName 为 null
        if (componentParts.Fragment != null)
            componentParts.Fragment.TypeName = null;

        string fileName = string.Format(componentPartsFileName_Format, _metaBaseDir, componentParts.LibraryId, componentParts.PartsId);

        string fileDirectory = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        File.WriteAllText(fileName, componentParts.ToJson(), Encoding.UTF8);
        return await Task.FromResult(true);
    }

    public async Task<bool> DeleteAsync(string libraryId, string partsId)
    {
        string fileName = string.Format(componentPartsFileName_Format, _metaBaseDir, libraryId, partsId);
        if (!File.Exists(fileName))
            return false;

        File.Delete(fileName);
        return await Task.FromResult(true);
    }
}
