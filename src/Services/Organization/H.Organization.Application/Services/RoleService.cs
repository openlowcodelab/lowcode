using H.Organization.Application.Contracts;
using H.Organization.Domain;
using H.Organization.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace H.Organization.Application.Services;

/// <summary>
/// 角色服务实现
/// </summary>
public class RoleService : IRoleService
{
    private readonly OrganizationDbContext _context;

    public RoleService(OrganizationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取角色列表
    /// </summary>
    public async Task<PagedResult<RoleDto>> GetListAsync(RoleQueryParams queryParams)
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

        return new PagedResult<RoleDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = queryParams.PageIndex,
            PageSize = queryParams.PageSize
        };
    }

    /// <summary>
    /// 获取角色详情
    /// </summary>
    public async Task<RoleDto?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Roles
            .Include(x => x.RoleMembers)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null) return null;

        return new RoleDto
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
        };
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    public async Task<RoleDto> CreateAsync(CreateRoleDto input)
    {
        // 检查编码是否已存在
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            var exists = await _context.Roles.AnyAsync(x => x.Code == input.Code);
            if (exists)
                throw new Exception("角色编码已存在");
        }

        var entity = new RoleEntity
        {
            Id = Guid.NewGuid(),
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

        return await GetByIdAsync(entity.Id) ?? throw new Exception("创建失败");
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    public async Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto input)
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

        return await GetByIdAsync(id) ?? throw new Exception("更新失败");
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    public async Task DeleteAsync(Guid id)
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
    }

    /// <summary>
    /// 批量删除角色
    /// </summary>
    public async Task BatchDeleteAsync(List<Guid> ids)
    {
        foreach (var id in ids)
        {
            await DeleteAsync(id);
        }
    }

    /// <summary>
    /// 获取所有启用的角色
    /// </summary>
    public async Task<List<RoleDto>> GetAllEnabledAsync()
    {
        return await _context.Roles
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
