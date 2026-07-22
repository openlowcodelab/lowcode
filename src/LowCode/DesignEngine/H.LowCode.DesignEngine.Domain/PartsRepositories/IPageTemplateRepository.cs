using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;

namespace H.LowCode.DesignEngine.Domain.Repositories;

public interface IPageTemplateRepository
{
    Task<List<PageTemplateListModel>> GetListAsync();

    Task<PageTemplateSchema> GetByIdAsync(string templateId);

    Task<bool> SaveAsync(PageTemplateSchema pageTemplate);

    Task<bool> DeleteAsync(string templateId);
}
