using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Validation;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class PageTemplateAppService : ApplicationService, IPageTemplateAppService
{
    private IPageTemplateRepository _repository => LazyServiceProvider.GetRequiredService<IPageTemplateRepository>();

    public async Task<List<PageTemplateListModel>> GetListAsync()
    {
        return await _repository.GetListAsync();
    }

    public async Task<PageTemplateSchema> GetByIdAsync(string templateId)
    {
        return await _repository.GetByIdAsync(templateId);
    }

    [DisableValidation]
    public async Task<bool> SaveAsync(PageTemplateSchema pageTemplate)
    {
        ArgumentNullException.ThrowIfNull(pageTemplate);
        ArgumentException.ThrowIfNullOrEmpty(pageTemplate.TemplateId);

        return await _repository.SaveAsync(pageTemplate);
    }

    public async Task<bool> DeleteAsync(string templateId)
    {
        return await _repository.DeleteAsync(templateId);
    }
}
