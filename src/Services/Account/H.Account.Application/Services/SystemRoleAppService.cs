using H.Account.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace H.Account.Application;

/// <summary>
/// 系统角色管理服务实现
/// </summary>
public class SystemRoleAppService : ApplicationService, ISystemRoleAppService
{
    private readonly IdentityRoleManager _roleManager;
    private readonly IdentityUserManager _userManager;

    public SystemRoleAppService(
        IdentityRoleManager roleManager,
        IdentityUserManager userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<List<SystemRoleDto>> GetRolesAsync()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        var result = new List<SystemRoleDto>();

        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            result.Add(new SystemRoleDto
            {
                Id = role.Id,
                Name = role.Name!,
                IsBuiltIn = role.IsStatic,
                UserCount = usersInRole.Count
            });
        }

        return result;
    }

    public async Task<SystemRoleDto> CreateRoleAsync(CreateSystemRoleDto dto)
    {
        if (SystemRoleNames.IsBuiltIn(dto.Name))
            throw new Exception("不允许创建与内置角色同名的角色");

        // 检查角色名是否已存在
        var existing = await _roleManager.FindByNameAsync(dto.Name);
        if (existing != null)
            throw new Exception("角色名称已存在");

        var role = new Volo.Abp.Identity.IdentityRole(
            GuidGenerator.Create(),
            dto.Name);
        role.IsStatic = false;  // 自定义角色可删除

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        return new SystemRoleDto
        {
            Id = role.Id,
            Name = role.Name!,
            IsBuiltIn = false,
            UserCount = 0
        };
    }

    public async Task DeleteRoleAsync(Guid roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString())
            ?? throw new Exception("角色不存在");

        if (role.IsStatic)
            throw new Exception("内置角色不可删除");

        // 检查是否有用户关联
        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        if (usersInRole.Any())
            throw new Exception("请先移除该角色下的所有用户");

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
