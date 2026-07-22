using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.Extensions.Options;
using System.Text;

namespace H.LowCode.DesignEngine.Repository.JsonFile;

public class PageTemplateRepository : PartsFileRepositoryBase, IPageTemplateRepository
{
    private static string pageTemplateFileName_Format = @"{0}\pageParts\{1}.json";
    private const string pageTemplateFolderName = "pageParts";

    public PageTemplateRepository(IOptions<MetaOption> metaOption) : base(metaOption)
    {

    }

    public async Task<List<PageTemplateListModel>> GetListAsync()
    {
        List<PageTemplateListModel> list = [];

        var folder = Path.Combine(_metaBaseDir, pageTemplateFolderName);
        if (!Directory.Exists(folder))
            return list;

        var files = Directory.GetFiles(folder, "*.json");
        foreach (var fileName in files)
        {
            var json = ReadAllText(fileName);
            if (string.IsNullOrWhiteSpace(json))
                continue;

            var schema = json.FromJson<PageTemplateSchema>();
            if (schema == null)
                continue;

            list.Add(new PageTemplateListModel
            {
                TemplateId = schema.TemplateId,
                Name = schema.Name,
                Category = schema.Category,
                PageType = schema.PageType,
                Description = schema.Description,
                Order = schema.Order,
                PublishStatus = schema.PublishStatus,
                ModifiedTime = schema.ModifiedTime
            });
        }

        list = list.OrderBy(t => t.Order).ToList();
        return await Task.FromResult(list);
    }

    public async Task<PageTemplateSchema> GetByIdAsync(string templateId)
    {
        string fileName = string.Format(pageTemplateFileName_Format, _metaBaseDir, templateId);

        var json = ReadAllText(fileName);
        var schema = json?.FromJson<PageTemplateSchema>();
        return await Task.FromResult(schema);
    }

    public async Task<bool> SaveAsync(PageTemplateSchema pageTemplate)
    {
        ArgumentNullException.ThrowIfNull(pageTemplate);
        ArgumentException.ThrowIfNullOrEmpty(pageTemplate.TemplateId);

        pageTemplate.ModifiedTime = DateTime.UtcNow;

        string fileName = string.Format(pageTemplateFileName_Format, _metaBaseDir, pageTemplate.TemplateId);

        string fileDirectory = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        File.WriteAllText(fileName, pageTemplate.ToJson(), Encoding.UTF8);
        return await Task.FromResult(true);
    }

    public async Task<bool> DeleteAsync(string templateId)
    {
        string fileName = string.Format(pageTemplateFileName_Format, _metaBaseDir, templateId);
        if (!File.Exists(fileName))
            return false;

        File.Delete(fileName);
        return await Task.FromResult(true);
    }
}
