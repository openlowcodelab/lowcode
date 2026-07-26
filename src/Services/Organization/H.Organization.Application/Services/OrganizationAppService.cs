using H.Organization.Application.Contracts;
using H.Organization.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Services;

namespace H.Organization.Application;

/// <summary>
/// 部门服务实现
/// </summary>
public class OrganizationAppService : ApplicationService, IOrganizationAppService
{
    private readonly OrganizationDbContext _context;

    public OrganizationAppService(OrganizationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取所有部门（树形结构）
    /// </summary>
    public async Task<List<OrganizationTreeDto>> GetAllAsTreeAsync()
    {
        var organizations = await _context.Organizations
            .Where(x => x.IsEnabled)
            .OrderBy(x => x.Sort)
            .ToListAsync();

        return BuildTree(organizations);
    }

    private List<OrganizationTreeDto> BuildTree(List<OrganizationEntity> organizations)
    {
        var lookup = organizations.ToLookup(x => x.ParentId);
        var roots = lookup[null];

        return roots.Select(x => new OrganizationTreeDto
        {
            Id = x.Id,
            ParentId = x.ParentId,
            Name = x.Name,
            Sort = x.Sort,
            Children = GetChildren(lookup, x.Id)
        }).ToList();
    }

    private List<OrganizationTreeDto> GetChildren(ILookup<Guid?, OrganizationEntity> lookup, Guid parentId)
    {
        return lookup[parentId]
            .Select(x => new OrganizationTreeDto
            {
                Id = x.Id,
                ParentId = x.ParentId,
                Name = x.Name,
                Sort = x.Sort,
                Children = GetChildren(lookup, x.Id)
            })
            .ToList();
    }

    /// <summary>
    /// 获取部门列表
    /// </summary>
    public async Task<PagedResult<OrganizationDto>> GetListAsync(OrganizationQueryParams queryParams)
    {
        var query = _context.Organizations.AsQueryable();

        if (queryParams.ParentId.HasValue)
        {
            query = query.Where(x => x.ParentId == queryParams.ParentId);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Keyword))
        {
            query = query.Where(x => x.Name.Contains(queryParams.Keyword) ||
                (x.Code != null && x.Code.Contains(queryParams.Keyword)));
        }

        if (queryParams.IsEnabled.HasValue)
        {
            query = query.Where(x => x.IsEnabled == queryParams.IsEnabled);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Sort)
            .Skip((queryParams.PageIndex - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(x => new OrganizationDto
            {
                Id = x.Id,
                ParentId = x.ParentId,
                Name = x.Name,
                Code = x.Code,
                Sort = x.Sort,
                LeaderId = x.LeaderId,
                Phone = x.Phone,
                Email = x.Email,
                IsEnabled = x.IsEnabled,
                CreatedAt = x.CreatedAt,
                Remark = x.Remark,
                ChildrenCount = x.Children.Count,
                MembersCount = x.Members.Count
            })
            .ToListAsync();

        return new PagedResult<OrganizationDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = queryParams.PageIndex,
            PageSize = queryParams.PageSize
        };
    }

    /// <summary>
    /// 获取部门详情
    /// </summary>
    public async Task<OrganizationDto?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Organizations
            .Include(x => x.Children)
            .Include(x => x.Members)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null) return null;

        return new OrganizationDto
        {
            Id = entity.Id,
            ParentId = entity.ParentId,
            Name = entity.Name,
            Code = entity.Code,
            Sort = entity.Sort,
            LeaderId = entity.LeaderId,
            Phone = entity.Phone,
            Email = entity.Email,
            IsEnabled = entity.IsEnabled,
            CreatedAt = entity.CreatedAt,
            Remark = entity.Remark,
            ChildrenCount = entity.Children.Count,
            MembersCount = entity.Members.Count
        };
    }

    /// <summary>
    /// 创建部门
    /// </summary>
    public async Task<OrganizationDto> CreateAsync(CreateOrganizationDto input)
    {
        var entity = new OrganizationEntity(Guid.NewGuid())
        {
            ParentId = input.ParentId,
            Name = input.Name,
            Code = input.Code,
            Sort = input.Sort,
            LeaderId = input.LeaderId,
            Phone = input.Phone,
            Email = input.Email,
            Remark = input.Remark,
            CreatedAt = DateTime.UtcNow
        };

        _context.Organizations.Add(entity);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(entity.Id) ?? throw new Exception("创建失败");
    }

    /// <summary>
    /// 更新部门
    /// </summary>
    public async Task<OrganizationDto> UpdateAsync(Guid id, UpdateOrganizationDto input)
    {
        var entity = await _context.Organizations.FindAsync(id);
        if (entity == null)
            throw new Exception("部门不存在");

        entity.ParentId = input.ParentId;
        entity.Name = input.Name;
        entity.Code = input.Code;
        entity.Sort = input.Sort;
        entity.LeaderId = input.LeaderId;
        entity.Phone = input.Phone;
        entity.Email = input.Email;
        entity.IsEnabled = input.IsEnabled;
        entity.Remark = input.Remark;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id) ?? throw new Exception("更新失败");
    }

    /// <summary>
    /// 删除部门
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Organizations.FindAsync(id);
        if (entity == null)
            throw new Exception("部门不存在");

        // 检查是否有子部门
        var hasChildren = await _context.Organizations.AnyAsync(x => x.ParentId == id);
        if (hasChildren)
            throw new Exception("请先删除子部门");

        // 检查是否有成员
        var hasMembers = await _context.Members.AnyAsync(x => x.OrganizationId == id);
        if (hasMembers)
            throw new Exception("请先移除部门成员");

        _context.Organizations.Remove(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 批量删除部门
    /// </summary>
    public async Task BatchDeleteAsync(List<Guid> ids)
    {
        foreach (var id in ids)
        {
            await DeleteAsync(id);
        }
    }

    /// <summary>
    /// 获取部门用户（包含子部门用户）
    /// </summary>
    public async Task<List<Guid>> GetOrganizationUserIdsAsync(Guid organizationId, bool includeChildren = true)
    {
        var userIds = new List<Guid>();

        if (includeChildren)
        {
            // 获取所有子部门ID
            var allOrgIds = await GetAllChildOrganizationIdsAsync(organizationId);
            userIds = await _context.Members
                .Where(x => allOrgIds.Contains(x.OrganizationId))
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync();
        }
        else
        {
            userIds = await _context.Members
                .Where(x => x.OrganizationId == organizationId)
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync();
        }

        return userIds;
    }

    private async Task<List<Guid>> GetAllChildOrganizationIdsAsync(Guid parentId)
    {
        var result = new List<Guid> { parentId };
        var children = await _context.Organizations
            .Where(x => x.ParentId == parentId)
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var childId in children)
        {
            result.AddRange(await GetAllChildOrganizationIdsAsync(childId));
        }

        return result;
    }
}
