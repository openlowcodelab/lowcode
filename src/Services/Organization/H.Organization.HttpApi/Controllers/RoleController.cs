using H.Organization.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace H.Organization.HttpApi.Controllers;

/// <summary>
/// 角色管理接口
/// </summary>
[ApiController]
[Route("api/roles")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// 获取角色列表
    /// </summary>
    [HttpGet]
    public async Task<PagedResult<RoleDto>> GetListAsync([FromQuery] RoleQueryParams queryParams)
    {
        return await _roleService.GetListAsync(queryParams);
    }

    /// <summary>
    /// 获取角色详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<RoleDto?> GetByIdAsync(Guid id)
    {
        return await _roleService.GetByIdAsync(id);
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    [HttpPost]
    public async Task<RoleDto> CreateAsync([FromBody] CreateRoleDto input)
    {
        return await _roleService.CreateAsync(input);
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    [HttpPut("{id}")]
    public async Task<RoleDto> UpdateAsync(Guid id, [FromBody] UpdateRoleDto input)
    {
        return await _roleService.UpdateAsync(id, input);
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await _roleService.DeleteAsync(id);
    }

    /// <summary>
    /// 批量删除角色
    /// </summary>
    [HttpPost("batch-delete")]
    public async Task BatchDeleteAsync([FromBody] List<Guid> ids)
    {
        await _roleService.BatchDeleteAsync(ids);
    }

    /// <summary>
    /// 获取所有启用的角色
    /// </summary>
    [HttpGet("enabled")]
    public async Task<List<RoleDto>> GetAllEnabledAsync()
    {
        return await _roleService.GetAllEnabledAsync();
    }
}
