using H.Abp.Application.Contracts;
using H.Notification.Application.Contracts;
using H.Notification.EntityFrameworkCore;
using H.Util.Base;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Notification.Application;

public class ContactAppService : ApplicationService, IContactAppService
{
    private readonly IRepository<ContactEntity, Guid> _contactRepository;
    private readonly IRepository<ContactGroupMemberEntity, Guid> _memberRepository;

    public ContactAppService(
        IRepository<ContactEntity, Guid> contactRepository,
        IRepository<ContactGroupMemberEntity, Guid> memberRepository)
    {
        _contactRepository = contactRepository;
        _memberRepository = memberRepository;
    }

    public async Task<BaseOutput<PagedResultDto<ContactDto>>> GetListAsync(ContactQueryDto input)
    {
        var query = (await _contactRepository.GetQueryableAsync())
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => x.Name.Contains(input.Filter!) || (x.Email != null && x.Email.Contains(input.Filter!)));

        if (input.GroupId.HasValue)
        {
            var memberQuery = await _memberRepository.GetQueryableAsync();
            var contactIds = memberQuery.Where(m => m.GroupId == input.GroupId).Select(m => m.ContactId);
            query = query.Where(x => contactIds.Contains(x.Id));
        }

        var totalCount = await AsyncExecuter.CountAsync(query);
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount));

        var ids = entities.Select(e => e.Id).ToList();
        var members = await AsyncExecuter.ToListAsync(
            (await _memberRepository.GetQueryableAsync()).Where(m => ids.Contains(m.ContactId)));
        var groupMap = members.GroupBy(m => m.ContactId).ToDictionary(g => g.Key, g => g.Select(x => x.GroupId).ToList());

        var dtos = entities.Select(e => MapToDto(e, groupMap.TryGetValue(e.Id, out var gs) ? gs : new List<long>())).ToList();
        return new(new PagedResultDto<ContactDto>(totalCount, dtos));
    }

    public async Task<BaseOutput<List<ContactDto>>> GetAllEnabledAsync()
    {
        var entities = await AsyncExecuter.ToListAsync((await _contactRepository.GetQueryableAsync()).Where(x => x.IsEnabled));
        return new(entities.Select(e => MapToDto(e, new List<long>())).ToList());
    }

    public async Task<BaseOutput<ContactDto>> GetAsync(Guid id)
    {
        var entity = await _contactRepository.GetAsync(id);
        var groups = await AsyncExecuter.ToListAsync(
            (await _memberRepository.GetQueryableAsync()).Where(m => m.ContactId == id).Select(m => m.GroupId));
        return new(MapToDto(entity, groups));
    }

    public async Task<BaseOutput<ContactDto>> CreateAsync(CreateContactDto input)
    {
        var entity = new ContactEntity(GuidGenerator.Create())
        {
            Name = input.Name,
            Description = input.Description,
            IsEnabled = input.IsEnabled,
            InAppUserId = input.InAppUserId,
            Email = input.Email,
            Phone = input.Phone,
            WebhookUrl = input.WebhookUrl
        };
        entity = await _contactRepository.InsertAsync(entity, autoSave: true);
        return new(MapToDto(entity, new List<long>()));
    }

    public async Task<BaseOutput<ContactDto>> UpdateAsync(Guid id, UpdateContactDto input)
    {
        var entity = await _contactRepository.GetAsync(id);
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.IsEnabled = input.IsEnabled;
        entity.InAppUserId = input.InAppUserId;
        entity.Email = input.Email;
        entity.Phone = input.Phone;
        entity.WebhookUrl = input.WebhookUrl;
        entity = await _contactRepository.UpdateAsync(entity, autoSave: true);

        var groups = await (await _memberRepository.GetQueryableAsync())
            .Where(m => m.ContactId == id).Select(m => m.GroupId).ToListAsync();
        return new(MapToDto(entity, groups));
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        await _contactRepository.DeleteAsync(id);
        return new();
    }

    private static ContactDto MapToDto(ContactEntity e, List<long> groupIds) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        IsEnabled = e.IsEnabled,
        InAppUserId = e.InAppUserId,
        Email = e.Email,
        Phone = e.Phone,
        WebhookUrl = e.WebhookUrl,
        CreationTime = e.CreationTime,
        GroupIds = groupIds
    };
}
