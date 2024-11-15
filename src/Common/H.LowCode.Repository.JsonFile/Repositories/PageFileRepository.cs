using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Model;
using H.LowCode.Domain.Repositories;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.DependencyInjection;

namespace H.LowCode.Repository.JsonFile;

public class PageFileRepository : FileRepositoryBase, IPageRepository, IScopedDependency
{
    private static string pageFileName_Format = @"{0}\{1}\page\{2}.json";

    public PageFileRepository(IOptions<MetaOption> metaOption) : base(metaOption)
    {

    }

    public async Task<List<PageListModel>> GetListAsync(string appId)
    {
        await Task.Delay(1);
        List<PageListModel> list = [];

        var pageFolder = Path.Combine(_metaBaseDir, appId, "page");
        if (!Directory.Exists(pageFolder))
            return list;

        var files = Directory.GetFiles(pageFolder);
        foreach (var fileName in files)
        {
            var pageSchemaJson = ReadAllText(fileName);
            var pageSchema = pageSchemaJson.FromJson<PageSchema>();

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

        return list;
    }

    public async Task<PageSchema> GetAsync(string appId, string pageId)
    {
        await Task.Delay(1);
        string fileName = string.Format(pageFileName_Format, _metaBaseDir, appId, pageId);

        var pageSchemaJson = ReadAllText(fileName);
        return pageSchemaJson.FromJson<PageSchema>();
    }

    public async Task SaveAsync(PageSchema pageSchema)
    {
        ArgumentNullException.ThrowIfNull(pageSchema);
        ArgumentException.ThrowIfNullOrEmpty(pageSchema.Id);

        pageSchema.ModifiedTime = DateTime.UtcNow;

        await Task.Delay(1);
        string fileName = string.Format(pageFileName_Format, _metaBaseDir, pageSchema.AppId, pageSchema.Id);

        string fileDirectory = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        File.WriteAllText(fileName, pageSchema.ToJson(), Encoding.UTF8);
    }

    public async Task DeleteAsync(string appId, string pageId)
    {
        await Task.Delay(1);

        string fileName = string.Format(pageFileName_Format, _metaBaseDir, appId, pageId);
        if (!File.Exists(fileName))
            return;

        var pageFolder = Path.Combine(_metaBaseDir, appId, "page");
        if (!Directory.Exists(pageFolder))
            return;

        File.Delete(fileName);
    }
}
