using H.LowCode.Configuration;
using H.LowCode.RenderEngine.Domain.Repositories;
using H.LowCode.MetaSchema.RenderEngine;
using Microsoft.Extensions.Options;
using System.Text;

namespace H.LowCode.RenderEngine.Repository.JsonFile;

public class PageFileRepository : FileRepositoryBase, IPageRepository
{
    private static string pageFileName_Format = @"{0}\{1}\page\{2}.json";

    public PageFileRepository(IOptions<MetaOption> metaOption) : base(metaOption)
    {

    }

    public Task<PageSchema> GetAsync(string appId, string pageId)
    {
        string fileName = string.Format(pageFileName_Format, _metaBaseDir, appId, pageId);

        var pageSchemaJson = ReadAllText(fileName);
        var pageSchema = pageSchemaJson.FromJson<PageSchema>();

        return Task.FromResult(pageSchema);
    }
}
