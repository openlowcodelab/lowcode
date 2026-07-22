using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.Extensions.Options;
using System.Text;

namespace H.LowCode.DesignEngine.Repository.JsonFile;

public class AppTemplateFileRepository : PartsFileRepositoryBase, IAppTemplateRepository
{
    private static string appTemplateFileName_Format = @"{0}\appTemplates\{1}.json";
    private const string appTemplateFolderName = "appTemplates";

    public AppTemplateFileRepository(IOptions<MetaOption> metaOption) : base(metaOption)
    {

    }

    public async Task<List<AppTemplateListModel>> GetListAsync()
    {
        List<AppTemplateListModel> list = [];

        var folder = Path.Combine(_metaBaseDir, appTemplateFolderName);
        if (!Directory.Exists(folder))
            return list;

        var files = Directory.GetFiles(folder, "*.json");
        foreach (var fileName in files)
        {
            var json = ReadAllText(fileName);
            if (string.IsNullOrWhiteSpace(json))
                continue;

            var schema = json.FromJson<AppTemplateSchema>();
            if (schema == null)
                continue;

            list.Add(new AppTemplateListModel
            {
                TemplateId = schema.TemplateId,
                Name = schema.Name,
                Description = schema.Description,
                Icon = schema.Icon,
                PageCount = schema.Pages?.Count ?? 0,
                Order = schema.Order,
                PublishStatus = schema.PublishStatus,
                ModifiedTime = schema.ModifiedTime
            });
        }

        list = list.OrderBy(t => t.Order).ToList();
        return await Task.FromResult(list);
    }

    public async Task<AppTemplateSchema> GetByIdAsync(string templateId)
    {
        string fileName = string.Format(appTemplateFileName_Format, _metaBaseDir, templateId);

        var json = ReadAllText(fileName);
        var schema = json?.FromJson<AppTemplateSchema>();
        return await Task.FromResult(schema);
    }

    public async Task<bool> SaveAsync(AppTemplateSchema appTemplate)
    {
        ArgumentNullException.ThrowIfNull(appTemplate);
        ArgumentException.ThrowIfNullOrEmpty(appTemplate.TemplateId);

        appTemplate.ModifiedTime = DateTime.UtcNow;

        string fileName = string.Format(appTemplateFileName_Format, _metaBaseDir, appTemplate.TemplateId);

        string fileDirectory = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        File.WriteAllText(fileName, appTemplate.ToJson(), Encoding.UTF8);
        return await Task.FromResult(true);
    }

    public async Task<bool> DeleteAsync(string templateId)
    {
        string fileName = string.Format(appTemplateFileName_Format, _metaBaseDir, templateId);
        if (!File.Exists(fileName))
            return false;

        File.Delete(fileName);
        return await Task.FromResult(true);
    }
}
