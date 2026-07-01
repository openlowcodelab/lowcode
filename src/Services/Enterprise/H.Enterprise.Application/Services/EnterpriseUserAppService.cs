using H.Enterprise.Application.Contracts.Dtos;
using H.Enterprise.Application.Contracts.Services;
using H.Enterprise.EntityFrameworkCore;
using H.Enterprise.EntityFrameworkCore.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Volo.Abp.Application.Services;

namespace H.Enterprise.Application.Services;

/// <summary>
/// 企业用户管理服务实现
/// </summary>
public class EnterpriseUserAppService : ApplicationService, IEnterpriseUserAppService
{
    private readonly EnterpriseDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EnterpriseUserAppService(
        EnterpriseDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<EnterpriseUserDto>> GetEnterpriseUsersAsync(Guid enterpriseId)
    {
        var users = await _context.EnterpriseUsers
            .Include(eu => eu.Enterprise)
            .Where(eu => eu.EnterpriseId == enterpriseId)
            .OrderByDescending(eu => eu.Role == "Owner")
            .ThenByDescending(eu => eu.JoinedAt)
            .ToListAsync();

        return users.Select(u => new EnterpriseUserDto
        {
            Id = u.Id,
            EnterpriseId = u.EnterpriseId,
            EnterpriseName = u.Enterprise?.Name ?? "",
            UserId = u.UserId,
            UserName = u.UserName,
            Role = u.Role,
            IsDefault = u.IsDefault,
            JoinedAt = u.JoinedAt
        }).ToList();
    }

    public async Task AddUserAsync(AddEnterpriseUserDto input)
    {
        // 检查是否已存在
        var exists = await _context.EnterpriseUsers
            .AnyAsync(eu => eu.EnterpriseId == input.EnterpriseId && eu.UserId == input.UserId);

        if (exists)
            throw new Exception("该用户已是企业成员");

        var entity = new EnterpriseUserEntity
        {
            Id = Guid.NewGuid(),
            EnterpriseId = input.EnterpriseId,
            UserId = input.UserId,
            UserName = input.UserId.ToString(), // 由调用方更新用户名
            Role = input.Role,
            IsDefault = false,
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = GetCurrentUserId()
        };

        _context.EnterpriseUsers.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveUserAsync(Guid enterpriseId, Guid userId)
    {
        var entity = await _context.EnterpriseUsers
            .FirstOrDefaultAsync(eu => eu.EnterpriseId == enterpriseId && eu.UserId == userId)
            ?? throw new Exception("用户不在该企业中");

        // 不允许移除 Owner
        if (entity.Role == "Owner")
            throw new Exception("不能移除企业所有者");

        _context.EnterpriseUsers.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task SetDefaultEnterpriseAsync(Guid enterpriseId)
    {
        var userId = GetCurrentUserId() ?? throw new Exception("未登录");

        // 清除该用户的所有默认企业标记
        var userEnterprises = await _context.EnterpriseUsers
            .Where(eu => eu.UserId == userId)
            .ToListAsync();

        foreach (var eu in userEnterprises)
        {
            eu.IsDefault = eu.EnterpriseId == enterpriseId;
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserRoleAsync(Guid enterpriseId, Guid userId, string role)
    {
        var entity = await _context.EnterpriseUsers
            .FirstOrDefaultAsync(eu => eu.EnterpriseId == enterpriseId && eu.UserId == userId)
            ?? throw new Exception("用户不在该企业中");

        // 不允许修改 Owner 角色
        if (entity.Role == "Owner")
            throw new Exception("不能修改企业所有者的角色");

        entity.Role = role;
        await _context.SaveChangesAsync();
    }

    private Guid? GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userIdClaim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}
