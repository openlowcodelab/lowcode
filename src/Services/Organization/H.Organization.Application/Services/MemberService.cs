using H.Account.Client;
using H.Organization.Application.Contracts;
using H.Organization.Domain;
using H.Organization.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace H.Organization.Application.Services;

/// <summary>
/// 成员服务实现
/// </summary>
public class MemberService : IMemberService
{
    private readonly OrganizationDbContext _context;
    private readonly AccountClient _accountClient;

    public MemberService(OrganizationDbContext context, AccountClient accountClient)
    {
        _context = context;
        _accountClient = accountClient;
    }

    /// <summary>
    /// 获取成员列表
    /// </summary>
    public async Task<PagedResult<MemberDto>> GetListAsync(MemberQueryParams queryParams)
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
                Remark = x.Remark
            })
            .ToListAsync();

        // 尝试从 Account 服务获取用户的邮箱和手机号
        foreach (var item in items)
        {
            try
            {
                var user = await _accountClient.GetUserAsync(item.UserId);
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

        return new PagedResult<MemberDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = queryParams.PageIndex,
            PageSize = queryParams.PageSize
        };
    }

    /// <summary>
    /// 获取成员详情
    /// </summary>
    public async Task<MemberDto?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Members
            .Include(x => x.Organization)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null) return null;

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
            var user = await _accountClient.GetUserAsync(entity.UserId);
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

        return dto;
    }

    /// <summary>
    /// 添加成员（从Account服务获取用户信息）
    /// </summary>
    public async Task<MemberDto> AddAsync(AddMemberDto input)
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
            var user = await _accountClient.GetUserAsync(input.UserId);
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

        var entity = new MemberEntity
        {
            Id = Guid.NewGuid(),
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

        return await GetByIdAsync(entity.Id) ?? throw new Exception("添加成员失败");
    }

    /// <summary>
    /// 更新成员
    /// </summary>
    public async Task<MemberDto> UpdateAsync(Guid id, UpdateMemberDto input)
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

        return await GetByIdAsync(id) ?? throw new Exception("更新成员失败");
    }

    /// <summary>
    /// 删除成员
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Members.FindAsync(id);
        if (entity == null)
            throw new Exception("成员不存在");

        _context.Members.Remove(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 批量删除成员
    /// </summary>
    public async Task BatchDeleteAsync(List<Guid> ids)
    {
        foreach (var id in ids)
        {
            await DeleteAsync(id);
        }
    }

    /// <summary>
    /// 获取部门下所有成员
    /// </summary>
    public async Task<List<MemberDto>> GetMembersByOrganizationIdAsync(Guid organizationId)
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
                Remark = x.Remark
            })
            .ToListAsync();

        // 尝试从 Account 服务获取用户的邮箱和手机号
        foreach (var item in items)
        {
            try
            {
                var user = await _accountClient.GetUserAsync(item.UserId);
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

        return items;
    }

    /// <summary>
    /// 检查用户是否已是部门成员
    /// </summary>
    public async Task<bool> ExistsAsync(Guid organizationId, Guid userId)
    {
        return await _context.Members.AnyAsync(x =>
            x.OrganizationId == organizationId && x.UserId == userId);
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
