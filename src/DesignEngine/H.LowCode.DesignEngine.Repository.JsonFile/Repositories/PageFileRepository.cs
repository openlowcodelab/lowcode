using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Model;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.Extensions.Options;
using System.Text;

namespace H.LowCode.DesignEngine.Repository.JsonFile;

public class PageFileRepository : FileRepositoryBase, IPageRepository
{
    private static string pageFileName_Format = @"{0}\{1}\page\{2}.json";

    public PageFileRepository(IOptions<MetaOption> metaOption) : base(metaOption)
    {

    }

    public async Task<List<PageListModel>> GetListAsync(string appId)
    {
        List<PageListModel> list = [];

        var pageFolder = Path.Combine(_metaBaseDir, appId, "page");
        if (!Directory.Exists(pageFolder))
            return list;

        var files = Directory.GetFiles(pageFolder);
        foreach (var fileName in files)
        {
            var pageSchemaJson = ReadAllText(fileName);
            var pageSchema = pageSchemaJson.FromJson<PagePartsSchema>();

            PageListModel model = new()
            {
                PageId = pageSchema.Id,
                PageName = pageSchema.Name,
                Order = pageSchema.Order,
                PageType = pageSchema.PageType,
                PublishStatus = pageSchema.PublishStatus,
                ModifiedTime = pageSchema.ModifiedTime
            };

            list.Add(model);
        }

        //排序
        list = list.OrderBy(t => t.Order).ToList();

        return await Task.FromResult(list);
    }

    public Task<PagePartsSchema> GetByIdAsync(string appId, string pageId)
    {
        string fileName = string.Format(pageFileName_Format, _metaBaseDir, appId, pageId);

        var pageSchemaJson = ReadAllText(fileName);
        var pageSchema = pageSchemaJson.FromJson<PagePartsSchema>();
        return Task.FromResult(pageSchema);
    }

    public Task SaveAsync(PagePartsSchema pageSchema)
    {
        ArgumentNullException.ThrowIfNull(pageSchema);
        ArgumentException.ThrowIfNullOrEmpty(pageSchema.Id);

        pageSchema.ModifiedTime = DateTime.UtcNow;

        string fileName = string.Format(pageFileName_Format, _metaBaseDir, pageSchema.AppId, pageSchema.Id);

        string fileDirectory = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        File.WriteAllText(fileName, pageSchema.ToJson(), Encoding.UTF8);
        return Task.CompletedTask;
    }

    public Task<PagePartsSchema> GetAsync(string appId, string pageId)
    {
        string fileName = string.Format(pageFileName_Format, _metaBaseDir, appId, pageId);

        var pageSchemaJson = ReadAllText(fileName);
        var pageSchema = pageSchemaJson.FromJson<PagePartsSchema>();

        return Task.FromResult(pageSchema);
    }

    public Task DeleteAsync(string appId, string pageId)
    {
        string fileName = string.Format(pageFileName_Format, _metaBaseDir, appId, pageId);
        if (!File.Exists(fileName))
            return Task.CompletedTask;

        File.Delete(fileName);
        return Task.CompletedTask;
    }
}
