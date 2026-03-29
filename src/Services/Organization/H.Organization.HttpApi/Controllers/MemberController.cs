using H.Organization.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace H.Organization.HttpApi.Controllers;

/// <summary>
/// 成员管理接口
/// </summary>
[ApiController]
[Route("api/members")]
public class MemberController : ControllerBase
{
    private readonly IMemberService _memberService;

    public MemberController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    /// <summary>
    /// 获取成员列表
    /// </summary>
    [HttpGet]
    public async Task<PagedResult<MemberDto>> GetListAsync([FromQuery] MemberQueryParams queryParams)
    {
        return await _memberService.GetListAsync(queryParams);
    }

    /// <summary>
    /// 获取成员详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<MemberDto?> GetByIdAsync(Guid id)
    {
        return await _memberService.GetByIdAsync(id);
    }

    /// <summary>
    /// 添加成员
    /// </summary>
    [HttpPost]
    public async Task<MemberDto> AddAsync([FromBody] AddMemberDto input)
    {
        return await _memberService.AddAsync(input);
    }

    /// <summary>
    /// 更新成员
    /// </summary>
    [HttpPut("{id}")]
    public async Task<MemberDto> UpdateAsync(Guid id, [FromBody] UpdateMemberDto input)
    {
        return await _memberService.UpdateAsync(id, input);
    }

    /// <summary>
    /// 删除成员
    /// </summary>
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await _memberService.DeleteAsync(id);
    }

    /// <summary>
    /// 批量删除成员
    /// </summary>
    [HttpPost("batch-delete")]
    public async Task BatchDeleteAsync([FromBody] List<Guid> ids)
    {
        await _memberService.BatchDeleteAsync(ids);
    }

    /// <summary>
    /// 获取部门下所有成员
    /// </summary>
    [HttpGet("by-organization/{organizationId}")]
    public async Task<List<MemberDto>> GetMembersByOrganizationIdAsync(Guid organizationId)
    {
        return await _memberService.GetMembersByOrganizationIdAsync(organizationId);
    }

    /// <summary>
    /// 检查用户是否已是部门成员
    /// </summary>
    [HttpGet("exists")]
    public async Task<bool> ExistsAsync(Guid organizationId, Guid userId)
    {
        return await _memberService.ExistsAsync(organizationId, userId);
    }
}
