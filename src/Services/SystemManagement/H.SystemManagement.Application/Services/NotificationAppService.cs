using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using System.Linq.Dynamic.Core;
using H.SystemManagement.Application.Contracts;
using H.SystemManagement.EntityFrameworkCore;

namespace H.SystemManagement.Application;

public class NotificationBusinessAppService
    : CrudAppService<NotificationBusinessEntity, NotificationBusinessDto, Guid, NotificationBusinessQueryDto, CreateNotificationBusinessDto, UpdateNotificationBusinessDto>,
    INotificationBusinessAppService
{
    public NotificationBusinessAppService(
        IRepository<NotificationBusinessEntity, Guid> repository)
        : base(repository)
    {
    }

    public override async Task<NotificationBusinessDto> GetAsync(Guid id)
    {
        var entity = await Repository
            .WithDetails()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(NotificationBusinessEntity), id);
        }

        var dto = ObjectMapper.Map<NotificationBusinessEntity, NotificationBusinessDto>(entity);
        dto.Methods = ObjectMapper.Map<List<NotificationMethodConfigEntity>, List<NotificationMethodConfigDto>>(entity.Methods.ToList());
        return dto;
    }

    public override async Task<PagedResultDto<NotificationBusinessDto>> GetListAsync(NotificationBusinessQueryDto input)
    {
        var query = Repository
            .WithDetails()
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => x.BusinessName.Contains(input.Filter!) || x.BusinessCode.Contains(input.Filter!));

        var totalCount = await AsyncExecuter.CountAsync(query);

        var entities = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        var dtos = ObjectMapper.Map<List<NotificationBusinessEntity>, List<NotificationBusinessDto>>(entities);
        foreach (var dto in dtos)
        {
            var entity = entities.First(e => e.Id == dto.Id);
            dto.Methods = ObjectMapper.Map<List<NotificationMethodConfigEntity>, List<NotificationMethodConfigDto>>(entity.Methods.ToList());
        }

        return new PagedResultDto<NotificationBusinessDto>(totalCount, dtos);
    }
}

public class NotificationMethodConfigAppService
    : ApplicationService,
    INotificationMethodConfigAppService
{
    private readonly IRepository<NotificationMethodConfigEntity, Guid> _repository;

    public NotificationMethodConfigAppService(IRepository<NotificationMethodConfigEntity, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<List<NotificationMethodConfigDto>> GetByBusinessIdAsync(Guid businessId)
    {
        var queryable = await _repository.GetQueryableAsync();
        var entities = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.BusinessId == businessId)
        );
        return ObjectMapper.Map<List<NotificationMethodConfigEntity>, List<NotificationMethodConfigDto>>(entities);
    }

    public async Task<NotificationMethodConfigDto> CreateAsync(Guid businessId, CreateNotificationMethodConfigDto input)
    {
        var entity = ObjectMapper.Map<CreateNotificationMethodConfigDto, NotificationMethodConfigEntity>(input);
        entity.BusinessId = businessId;

        await _repository.InsertAsync(entity);

        return ObjectMapper.Map<NotificationMethodConfigEntity, NotificationMethodConfigDto>(entity);
    }

    public async Task<NotificationMethodConfigDto> UpdateAsync(Guid id, CreateNotificationMethodConfigDto input)
    {
        var entity = await _repository.GetAsync(id);

        entity.MethodType = (int)input.MethodType;
        entity.IsEnabled = input.IsEnabled;
        entity.WebhookUrl = input.WebhookUrl;
        entity.SmsTemplateId = input.SmsTemplateId;
        entity.EmailAddress = input.EmailAddress;

        await _repository.UpdateAsync(entity);

        return ObjectMapper.Map<NotificationMethodConfigEntity, NotificationMethodConfigDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
