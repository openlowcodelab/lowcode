using H.Organization.Application.Contracts;
using H.Organization.EntityFrameworkCore;
using H.Util.Base;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Services;

namespace H.Organization.Application;

/// <summary>
/// 角色服务实现
/// </summary>
public class RoleAppService : ApplicationService, IRoleAppService
{
    private readonly OrganizationDbContext _context;

    public RoleAppService(OrganizationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取角色列表
    /// </summary>
    public async Task<BaseOutput<PagedResult<RoleDto>>> GetListAsync(RoleQueryParams queryParams)
    {
        var query = _context.Roles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.Keyword))
        {
            query = query.Where(x => x.Name.Contains(queryParams.Keyword) ||
                (x.Code != null && x.Code.Contains(queryParams.Keyword)));
        }

        if (queryParams.RoleType.HasValue)
        {
            query = query.Where(x => x.RoleType == queryParams.RoleType);
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
            .Select(x => new RoleDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                RoleType = x.RoleType,
                RoleTypeName = GetRoleTypeName(x.RoleType),
                Sort = x.Sort,
                DataScope = x.DataScope,
                DataScopeName = GetDataScopeName(x.DataScope),
                IsEnabled = x.IsEnabled,
                CreatedAt = x.CreatedAt,
                Remark = x.Remark,
                MembersCount = x.RoleMembers.Count
            })
            .ToListAsync();

        return BaseOutput<PagedResult<RoleDto>>.Ok(new PagedResult<RoleDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = queryParams.PageIndex,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// 获取角色详情
    /// </summary>
    public async Task<BaseOutput<RoleDto>> GetByIdAsync(Guid id)
    {
        var entity = await _context.Roles
            .Include(x => x.RoleMembers)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null) return BaseOutput<RoleDto>.Ok(null);

        return BaseOutput<RoleDto>.Ok(new RoleDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            RoleType = entity.RoleType,
            RoleTypeName = GetRoleTypeName(entity.RoleType),
            Sort = entity.Sort,
            DataScope = entity.DataScope,
            DataScopeName = GetDataScopeName(entity.DataScope),
            IsEnabled = entity.IsEnabled,
            CreatedAt = entity.CreatedAt,
            Remark = entity.Remark,
            MembersCount = entity.RoleMembers.Count
        });
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    public async Task<BaseOutput<RoleDto>> CreateAsync(CreateRoleDto input)
    {
        // 检查编码是否已存在
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            var exists = await _context.Roles.AnyAsync(x => x.Code == input.Code);
            if (exists)
                throw new Exception("角色编码已存在");
        }

        var entity = new RoleEntity(Guid.NewGuid())
        {
            Name = input.Name,
            Code = input.Code,
            RoleType = input.RoleType,
            Sort = input.Sort,
            DataScope = input.DataScope,
            Remark = input.Remark,
            CreatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(entity);
        await _context.SaveChangesAsync();

        var created = await GetByIdAsync(entity.Id);
        return BaseOutput<RoleDto>.Ok(created.Data ?? throw new Exception("创建失败"));
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    public async Task<BaseOutput<RoleDto>> UpdateAsync(Guid id, UpdateRoleDto input)
    {
        var entity = await _context.Roles.FindAsync(id);
        if (entity == null)
            throw new Exception("角色不存在");

        // 检查编码是否已存在（排除自身）
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            var exists = await _context.Roles.AnyAsync(x => x.Code == input.Code && x.Id != id);
            if (exists)
                throw new Exception("角色编码已存在");
        }

        entity.Name = input.Name;
        entity.Code = input.Code;
        entity.RoleType = input.RoleType;
        entity.Sort = input.Sort;
        entity.DataScope = input.DataScope;
        entity.IsEnabled = input.IsEnabled;
        entity.Remark = input.Remark;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var updated = await GetByIdAsync(id);
        return BaseOutput<RoleDto>.Ok(updated.Data ?? throw new Exception("更新失败"));
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        var entity = await _context.Roles.FindAsync(id);
        if (entity == null)
            throw new Exception("角色不存在");

        // 检查是否有成员
        var hasMembers = await _context.RoleMembers.AnyAsync(x => x.RoleId == id);
        if (hasMembers)
            throw new Exception("请先移除角色成员");

        _context.Roles.Remove(entity);
        await _context.SaveChangesAsync();
        return BaseOutput.Ok();
    }

    /// <summary>
    /// 批量删除角色
    /// </summary>
    public async Task<BaseOutput> BatchDeleteAsync(List<Guid> ids)
    {
        foreach (var id in ids)
        {
            await DeleteAsync(id);
        }
        return BaseOutput.Ok();
    }

    /// <summary>
    /// 获取所有启用的角色
    /// </summary>
    public async Task<BaseOutput<List<RoleDto>>> GetAllEnabledAsync()
    {
        var roles = await _context.Roles
            .Where(x => x.IsEnabled)
            .OrderBy(x => x.Sort)
            .Select(x => new RoleDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                RoleType = x.RoleType,
                RoleTypeName = GetRoleTypeName(x.RoleType),
                Sort = x.Sort,
                DataScope = x.DataScope,
                DataScopeName = GetDataScopeName(x.DataScope),
                IsEnabled = x.IsEnabled,
                CreatedAt = x.CreatedAt,
                Remark = x.Remark,
                MembersCount = x.RoleMembers.Count
            })
            .ToListAsync();
        return BaseOutput<List<RoleDto>>.Ok(roles);
    }

    /// <summary>
    /// 获取角色已分配成员
    /// </summary>
    public async Task<BaseOutput<List<RoleMemberDto>>> GetRoleMembersAsync(Guid roleId)
    {
        var members = await _context.RoleMembers
            .Where(x => x.RoleId == roleId && x.Member != null)
            .OrderBy(x => x.Member!.Sort)
            .Select(x => new RoleMemberDto
            {
                MemberId = x.MemberId,
                UserId = x.Member!.UserId,
                UserName = x.Member!.UserName,
                OrganizationName = x.Member!.Organization != null ? x.Member!.Organization.Name : string.Empty
            })
            .ToListAsync();
        return BaseOutput<List<RoleMemberDto>>.Ok(members);
    }

    private static string GetRoleTypeName(int roleType)
    {
        return roleType switch
        {
            1 => "系统角色",
            2 => "自定义角色",
            _ => "未知"
        };
    }

    private static string GetDataScopeName(int dataScope)
    {
        return dataScope switch
        {
            1 => "全部数据",
            2 => "本部门数据",
            3 => "仅本人数据",
            _ => "未知"
        };
    }
}
