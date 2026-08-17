using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Base;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Validation;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class PageTemplateAppService : ApplicationService, IPageTemplateAppService
{
    private IPageTemplateRepository _repository => LazyServiceProvider.GetRequiredService<IPageTemplateRepository>();

    public async Task<BaseOutput<List<PageTemplateListModel>>> GetListAsync()
    {
        return new(await _repository.GetListAsync());
    }

    public async Task<BaseOutput<PageTemplateSchema>> GetByIdAsync(string templateId)
    {
        return new(await _repository.GetByIdAsync(templateId));
    }

    [DisableValidation]
    public async Task<BaseOutput<bool>> SaveAsync(PageTemplateSchema pageTemplate)
    {
        ArgumentNullException.ThrowIfNull(pageTemplate);
        ArgumentException.ThrowIfNullOrEmpty(pageTemplate.TemplateId);

        return new(await _repository.SaveAsync(pageTemplate));
    }

    public async Task<BaseOutput<bool>> DeleteAsync(string templateId)
    {
        return new(await _repository.DeleteAsync(templateId));
    }
}
