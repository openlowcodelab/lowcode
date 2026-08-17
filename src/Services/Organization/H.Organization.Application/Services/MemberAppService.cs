using H.Account.Application.Contracts;
using H.Organization.Application.Contracts;
using H.Organization.EntityFrameworkCore;
using H.Util.Base;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Services;

namespace H.Organization.Application;

/// <summary>
/// 成员服务实现
/// </summary>
public class MemberAppService : ApplicationService, IMemberAppService
{
    private readonly OrganizationDbContext _context;
    private readonly IAccountUserAppService _userService;

    public MemberAppService(OrganizationDbContext context, IAccountUserAppService userService)
    {
        _context = context;
        _userService = userService;
    }

    /// <summary>
    /// 获取成员列表
    /// </summary>
    public async Task<BaseOutput<H.Organization.Application.Contracts.PagedResult<MemberDto>>> GetListAsync(MemberQueryParams queryParams)
    {
        var query = _context.Members
            .Include(x => x.Organization)
            .AsQueryable();

        if (queryParams.OrganizationId.HasValue)
        {
            query = query.Where(x => x.OrganizationId == queryParams.OrganizationId);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Keyword))
        {
            query = query.Where(x => x.UserName.Contains(queryParams.Keyword));
        }

        if (queryParams.MemberType.HasValue)
        {
            query = query.Where(x => x.MemberType == queryParams.MemberType);
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
            .Select(x => new MemberDto
            {
                Id = x.Id,
                OrganizationId = x.OrganizationId,
                OrganizationName = x.Organization != null ? x.Organization.Name : string.Empty,
                UserId = x.UserId,
                UserName = x.UserName,
                MemberType = x.MemberType,
                MemberTypeName = GetMemberTypeName(x.MemberType),
                Sort = x.Sort,
                IsMain = x.IsMain,
                IsEnabled = x.IsEnabled,
                CreatedAt = x.CreatedAt,
                Remark = x.Remark,
                RoleNames = _context.RoleMembers
                    .Where(rm => rm.MemberId == x.Id && rm.Role != null)
                    .Select(rm => rm.Role!.Name)
                    .ToList()
            })
            .ToListAsync();

        // 尝试从 Account 服务获取用户的邮箱和手机号
        foreach (var item in items)
        {
            try
            {
                var user = (await _userService.GetUserDtoByIdAsync(item.UserId)).Data;
                if (user != null)
                {
                    item.Email = user.Email;
                    item.PhoneNumber = user.PhoneNumber;
                }
            }
            catch
            {
                // 忽略错误，使用本地数据
            }
        }

        return new(new H.Organization.Application.Contracts.PagedResult<MemberDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = queryParams.PageIndex,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// 获取成员详情
    /// </summary>
    public async Task<BaseOutput<MemberDto>> GetByIdAsync(Guid id)
    {
        var entity = await _context.Members
            .Include(x => x.Organization)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null) return new(null);

        var dto = new MemberDto
        {
            Id = entity.Id,
            OrganizationId = entity.OrganizationId,
            OrganizationName = entity.Organization?.Name ?? string.Empty,
            UserId = entity.UserId,
            UserName = entity.UserName,
            MemberType = entity.MemberType,
            MemberTypeName = GetMemberTypeName(entity.MemberType),
            Sort = entity.Sort,
            IsMain = entity.IsMain,
            IsEnabled = entity.IsEnabled,
            CreatedAt = entity.CreatedAt,
            Remark = entity.Remark
        };

        // 尝试从 Account 服务获取用户的邮箱和手机号
        try
        {
            var user = (await _userService.GetUserDtoByIdAsync(entity.UserId)).Data;
            if (user != null)
            {
                dto.Email = user.Email;
                dto.PhoneNumber = user.PhoneNumber;
            }
        }
        catch
        {
            // 忽略错误，使用本地数据
        }

        return new(dto);
    }

    /// <summary>
    /// 添加成员（从Account服务获取用户信息）
    /// </summary>
    public async Task<BaseOutput<MemberDto>> AddAsync(AddMemberDto input)
    {
        // 检查用户是否已是部门成员
        var exists = await _context.Members.AnyAsync(x =>
            x.OrganizationId == input.OrganizationId && x.UserId == input.UserId);
        if (exists)
            throw new Exception("该用户已是部门成员");

        // 如果设置为主部门，需要先将其他主部门取消
        if (input.IsMain)
        {
            var currentMain = await _context.Members
                .Where(x => x.UserId == input.UserId && x.IsMain)
                .ToListAsync();
            foreach (var member in currentMain)
            {
                member.IsMain = false;
            }
        }

        // 从 Account 服务获取用户信息
        string userName = string.Empty;
        try
        {
            var user = (await _userService.GetUserDtoByIdAsync(input.UserId)).Data;
            if (user != null)
            {
                userName = user.UserName;
            }
        }
        catch
        {
            throw new Exception("无法获取用户信息，请确认用户是否存在于Account服务中");
        }

        if (string.IsNullOrEmpty(userName))
        {
            throw new Exception("用户不存在");
        }

        var entity = new MemberEntity(Guid.NewGuid())
        {
            OrganizationId = input.OrganizationId,
            UserId = input.UserId,
            UserName = userName,
            MemberType = input.MemberType,
            Sort = input.Sort,
            IsMain = input.IsMain,
            Remark = input.Remark,
            CreatedAt = DateTime.UtcNow
        };

        _context.Members.Add(entity);
        await _context.SaveChangesAsync();

        var added = await GetByIdAsync(entity.Id);
        return new(added.Data ?? throw new Exception("添加成员失败"));
    }

    /// <summary>
    /// 批量添加成员（一个用户关联多个部门）
    /// </summary>
    public async Task<BaseOutput<List<MemberDto>>> AddBatchAsync(AddMemberBatchDto input)
    {
        if (input.OrganizationIds == null || input.OrganizationIds.Count == 0)
            throw new Exception("请选择至少一个部门");

        // 从 Account 服务获取用户信息
        string userName = string.Empty;
        try
        {
            var user = (await _userService.GetUserDtoByIdAsync(input.UserId)).Data;
            if (user != null)
            {
                userName = user.UserName;
            }
        }
        catch
        {
            throw new Exception("无法获取用户信息，请确认用户是否存在于Account服务中");
        }

        if (string.IsNullOrEmpty(userName))
            throw new Exception("用户不存在");

        // 如果设置为主部门，先取消该用户已有的主部门
        if (input.IsMain)
        {
            var currentMain = await _context.Members
                .Where(x => x.UserId == input.UserId && x.IsMain)
                .ToListAsync();
            foreach (var member in currentMain)
            {
                member.IsMain = false;
            }
        }

        var distinctOrgIds = input.OrganizationIds.Distinct().ToList();
        var createdIds = new List<Guid>();
        var isFirst = true;

        foreach (var orgId in distinctOrgIds)
        {
            // 跳过已存在的部门成员
            var exists = await _context.Members.AnyAsync(x =>
                x.OrganizationId == orgId && x.UserId == input.UserId);
            if (exists)
                continue;

            var entity = new MemberEntity(Guid.NewGuid())
            {
                OrganizationId = orgId,
                UserId = input.UserId,
                UserName = userName,
                MemberType = input.MemberType,
                Sort = input.Sort,
                // 主部门全局唯一，仅首个新增部门可设为主部门
                IsMain = input.IsMain && isFirst,
                Remark = input.Remark,
                CreatedAt = DateTime.UtcNow
            };

            _context.Members.Add(entity);
            createdIds.Add(entity.Id);
            isFirst = false;
        }

        await _context.SaveChangesAsync();

        var result = new List<MemberDto>();
        foreach (var id in createdIds)
        {
            var dto = (await GetByIdAsync(id)).Data;
            if (dto != null)
                result.Add(dto);
        }
        return new(result);
    }

    /// <summary>
    /// 搜索可分配用户（用于成员选择器）
    /// </summary>
    public async Task<BaseOutput<List<AssignableUserDto>>> SearchAssignableUsersAsync(string? keyword)
    {
        var result = await _userService.GetPagedUsersAsync(new H.Account.Application.Contracts.UserQueryParams
        {
            Keyword = keyword,
            PageIndex = 1,
            PageSize = 50
        });

        var users = result.Data?.Items ?? new List<H.Account.Application.Contracts.UserDto>();

        return new(users.Select(u => new AssignableUserDto
        {
            UserId = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber
        }).ToList());
    }

    /// <summary>
    /// 为成员分配角色（全量重建）
    /// </summary>
    public async Task<BaseOutput> AssignRolesAsync(Guid memberId, AssignMemberRolesDto input)
    {
        var member = await _context.Members.FindAsync(memberId);
        if (member == null)
            throw new Exception("成员不存在");

        // 删除现有关联
        var existing = await _context.RoleMembers
            .Where(x => x.MemberId == memberId)
            .ToListAsync();
        _context.RoleMembers.RemoveRange(existing);

        // 按 RoleIds 重建
        foreach (var roleId in input.RoleIds.Distinct())
        {
            _context.RoleMembers.Add(new RoleMember(Guid.NewGuid())
            {
                RoleId = roleId,
                MemberId = memberId,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return new();
    }

    /// <summary>
    /// 获取成员已授角色ID列表
    /// </summary>
    public async Task<BaseOutput<List<Guid>>> GetMemberRoleIdsAsync(Guid memberId)
    {
        var roleIds = await _context.RoleMembers
            .Where(x => x.MemberId == memberId)
            .Select(x => x.RoleId)
            .ToListAsync();
        return new(roleIds);
    }

    /// <summary>
    /// 更新成员
    /// </summary>
    public async Task<BaseOutput<MemberDto>> UpdateAsync(Guid id, UpdateMemberDto input)
    {
        var entity = await _context.Members.FindAsync(id);
        if (entity == null)
            throw new Exception("成员不存在");

        // 如果设置为主部门，需要先将其他主部门取消
        if (input.IsMain && !entity.IsMain)
        {
            var currentMain = await _context.Members
                .Where(x => x.UserId == entity.UserId && x.IsMain && x.Id != id)
                .ToListAsync();
            foreach (var member in currentMain)
            {
                member.IsMain = false;
            }
        }

        entity.MemberType = input.MemberType;
        entity.Sort = input.Sort;
        entity.IsMain = input.IsMain;
        entity.IsEnabled = input.IsEnabled;
        entity.Remark = input.Remark;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var updated = await GetByIdAsync(id);
        return new(updated.Data ?? throw new Exception("更新成员失败"));
    }

    /// <summary>
    /// 删除成员
    /// </summary>
    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        var entity = await _context.Members.FindAsync(id);
        if (entity == null)
            throw new Exception("成员不存在");

        _context.Members.Remove(entity);
        await _context.SaveChangesAsync();
        return new();
    }

    /// <summary>
    /// 批量删除成员
    /// </summary>
    public async Task<BaseOutput> BatchDeleteAsync(List<Guid> ids)
    {
        foreach (var id in ids)
        {
            await DeleteAsync(id);
        }
        return new();
    }

    /// <summary>
    /// 获取部门下所有成员
    /// </summary>
    public async Task<BaseOutput<List<MemberDto>>> GetMembersByOrganizationIdAsync(Guid organizationId)
    {
        var items = await _context.Members
            .Include(x => x.Organization)
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Sort)
            .Select(x => new MemberDto
            {
                Id = x.Id,
                OrganizationId = x.OrganizationId,
                OrganizationName = x.Organization != null ? x.Organization.Name : string.Empty,
                UserId = x.UserId,
                UserName = x.UserName,
                MemberType = x.MemberType,
                MemberTypeName = GetMemberTypeName(x.MemberType),
                Sort = x.Sort,
                IsMain = x.IsMain,
                IsEnabled = x.IsEnabled,
                CreatedAt = x.CreatedAt,
                Remark = x.Remark,
                RoleNames = _context.RoleMembers
                    .Where(rm => rm.MemberId == x.Id && rm.Role != null)
                    .Select(rm => rm.Role!.Name)
                    .ToList()
            })
            .ToListAsync();

        // 尝试从 Account 服务获取用户的邮箱和手机号
        foreach (var item in items)
        {
            try
            {
                var user = (await _userService.GetUserDtoByIdAsync(item.UserId)).Data;
                if (user != null)
                {
                    item.Email = user.Email;
                    item.PhoneNumber = user.PhoneNumber;
                }
            }
            catch
            {
                // 忽略错误，使用本地数据
            }
        }

        return new(items);
    }

    /// <summary>
    /// 检查用户是否已是部门成员
    /// </summary>
    public async Task<BaseOutput<bool>> ExistsAsync(Guid organizationId, Guid userId)
    {
        var exists = await _context.Members.AnyAsync(x =>
            x.OrganizationId == organizationId && x.UserId == userId);
        return new(exists);
    }

    private static string GetMemberTypeName(int memberType)
    {
        return memberType switch
        {
            1 => "普通成员",
            2 => "部门负责人",
            3 => "部门经理",
            _ => "未知"
        };
    }
}
