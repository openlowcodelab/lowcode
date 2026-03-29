using H.Organization.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace H.Organization.HttpApi.Controllers;

/// <summary>
/// 部门管理接口
/// </summary>
[ApiController]
[Route("api/organizations")]
public class OrganizationController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    /// <summary>
    /// 获取所有部门（树形结构）
    /// </summary>
    [HttpGet("tree")]
    public async Task<List<OrganizationTreeDto>> GetAllAsTreeAsync()
    {
        return await _organizationService.GetAllAsTreeAsync();
    }

    /// <summary>
    /// 获取部门列表
    /// </summary>
    [HttpGet]
    public async Task<PagedResult<OrganizationDto>> GetListAsync([FromQuery] OrganizationQueryParams queryParams)
    {
        return await _organizationService.GetListAsync(queryParams);
    }

    /// <summary>
    /// 获取部门详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<OrganizationDto?> GetByIdAsync(Guid id)
    {
        return await _organizationService.GetByIdAsync(id);
    }

    /// <summary>
    /// 创建部门
    /// </summary>
    [HttpPost]
    public async Task<OrganizationDto> CreateAsync([FromBody] CreateOrganizationDto input)
    {
        return await _organizationService.CreateAsync(input);
    }

    /// <summary>
    /// 更新部门
    /// </summary>
    [HttpPut("{id}")]
    public async Task<OrganizationDto> UpdateAsync(Guid id, [FromBody] UpdateOrganizationDto input)
    {
        return await _organizationService.UpdateAsync(id, input);
    }

    /// <summary>
    /// 删除部门
    /// </summary>
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await _organizationService.DeleteAsync(id);
    }

    /// <summary>
    /// 批量删除部门
    /// </summary>
    [HttpPost("batch-delete")]
    public async Task BatchDeleteAsync([FromBody] List<Guid> ids)
    {
        await _organizationService.BatchDeleteAsync(ids);
    }

    /// <summary>
    /// 获取部门用户（包含子部门用户）
    /// </summary>
    [HttpGet("{id}/users")]
    public async Task<List<Guid>> GetOrganizationUserIdsAsync(Guid id, [FromQuery] bool includeChildren = true)
    {
        return await _organizationService.GetOrganizationUserIdsAsync(id, includeChildren);
    }
}
