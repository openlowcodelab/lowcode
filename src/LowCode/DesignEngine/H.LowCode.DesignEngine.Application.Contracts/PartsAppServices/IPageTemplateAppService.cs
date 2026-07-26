using H.Abstractions;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IPageTemplateAppService : IAppService
{
    Task<List<PageTemplateListModel>> GetListAsync();

    Task<PageTemplateSchema> GetByIdAsync(string templateId);

    Task<bool> SaveAsync(PageTemplateSchema pageTemplate);

    Task<bool> DeleteAsync(string templateId);
}
