using Microsoft.EntityFrameworkCore;
using H.Notification.Application.Contracts;
using H.Notification.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Notification.Application;

public class ContactGroupAppService : ApplicationService, IContactGroupAppService
{
    private readonly IRepository<ContactGroupEntity, long> _groupRepository;
    private readonly IRepository<ContactGroupMemberEntity, Guid> _memberRepository;

    public ContactGroupAppService(
        IRepository<ContactGroupEntity, long> groupRepository,
        IRepository<ContactGroupMemberEntity, Guid> memberRepository)
    {
        _groupRepository = groupRepository;
        _memberRepository = memberRepository;
    }

    public async Task<PagedResultDto<ContactGroupDto>> GetListAsync(ContactGroupQueryDto input)
    {
        var query = (await _groupRepository.GetQueryableAsync())
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Name.Contains(input.Filter!));

        var totalCount = await AsyncExecuter.CountAsync(query);
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount));

        var ids = entities.Select(e => e.Id).ToList();
        var memberQuery = await _memberRepository.GetQueryableAsync();
        var counts = await AsyncExecuter.ToListAsync(
            memberQuery.Where(m => ids.Contains(m.GroupId))
                       .GroupBy(m => m.GroupId).Select(g => new { GroupId = g.Key, Count = g.Count() }));
        var countMap = counts.ToDictionary(x => x.GroupId, x => x.Count);

        var dtos = entities.Select(e => MapToDto(e, countMap.TryGetValue(e.Id, out var n) ? n : 0)).ToList();
        return new PagedResultDto<ContactGroupDto>(totalCount, dtos);
    }

    public async Task<List<ContactGroupDto>> GetAllEnabledAsync()
    {
        var entities = await AsyncExecuter.ToListAsync((await _groupRepository.GetQueryableAsync()).Where(x => x.IsEnabled));
        return entities.Select(e => MapToDto(e, 0)).ToList();
    }

    public async Task<ContactGroupDto> GetAsync(long id)
    {
        var entity = await _groupRepository.GetAsync(id);
        var dto = MapToDto(entity, 0);
        var members = await (await _memberRepository.GetQueryableAsync())
            .Where(m => m.GroupId == id).Select(m => m.ContactId).ToListAsync();
        dto.ContactIds = members;
        dto.ContactCount = members.Count;
        return dto;
    }

    public async Task<ContactGroupDto> CreateAsync(CreateContactGroupDto input)
    {
        var entity = new ContactGroupEntity
        {
            Name = input.Name,
            Description = input.Description,
            IsEnabled = input.IsEnabled
        };
        await _groupRepository.InsertAsync(entity, autoSave: true);
        await ReplaceMembersAsync(entity.Id, input.ContactIds);
        var dto = MapToDto(entity, input.ContactIds.Distinct().Count());
        dto.ContactIds = input.ContactIds.Distinct().ToList();
        return dto;
    }

    public async Task<ContactGroupDto> UpdateAsync(long id, UpdateContactGroupDto input)
    {
        var entity = await _groupRepository.GetAsync(id);
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.IsEnabled = input.IsEnabled;
        await _groupRepository.UpdateAsync(entity, autoSave: true);
        await ReplaceMembersAsync(id, input.ContactIds);
        var dto = MapToDto(entity, input.ContactIds.Distinct().Count());
        dto.ContactIds = input.ContactIds.Distinct().ToList();
        return dto;
    }

    private async Task ReplaceMembersAsync(long groupId, List<Guid> contactIds)
    {
        var existing = await (await _memberRepository.GetQueryableAsync())
            .Where(m => m.GroupId == groupId).ToListAsync();
        await _memberRepository.DeleteManyAsync(existing);

        var members = contactIds.Distinct().Select(cid => new ContactGroupMemberEntity(GuidGenerator.Create())
        {
            GroupId = groupId,
            ContactId = cid
        }).ToList();
        await _memberRepository.InsertManyAsync(members, autoSave: true);
    }

    public async Task DeleteAsync(long id)
    {
        // 先删除分组成员关系
        var members = await AsyncExecuter.ToListAsync(
            (await _memberRepository.GetQueryableAsync()).Where(m => m.GroupId == id));
        await _memberRepository.DeleteManyAsync(members);
        await _groupRepository.DeleteAsync(id);
    }

    private static ContactGroupDto MapToDto(ContactGroupEntity e, int count) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        IsEnabled = e.IsEnabled,
        CreationTime = e.CreationTime,
        ContactCount = count
    };
}
