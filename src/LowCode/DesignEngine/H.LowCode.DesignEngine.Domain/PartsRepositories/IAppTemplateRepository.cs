using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;

namespace H.LowCode.DesignEngine.Domain.Repositories;

public interface IAppTemplateRepository
{
    Task<List<AppTemplateListModel>> GetListAsync();

    Task<AppTemplateSchema> GetByIdAsync(string templateId);

    Task<bool> SaveAsync(AppTemplateSchema appTemplate);

    Task<bool> DeleteAsync(string templateId);
}
