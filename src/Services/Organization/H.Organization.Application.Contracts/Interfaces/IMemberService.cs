using System;

namespace H.Organization.Application.Contracts;

/// <summary>
/// 成员服务接口
/// </summary>
public interface IMemberService
{
    /// <summary>
    /// 获取成员列表
    /// </summary>
    Task<PagedResult<MemberDto>> GetListAsync(MemberQueryParams queryParams);

    /// <summary>
    /// 获取成员详情
    /// </summary>
    Task<MemberDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// 添加成员（从Account服务获取用户信息）
    /// </summary>
    Task<MemberDto> AddAsync(AddMemberDto input);

    /// <summary>
    /// 更新成员
    /// </summary>
    Task<MemberDto> UpdateAsync(Guid id, UpdateMemberDto input);

    /// <summary>
    /// 删除成员
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// 批量删除成员
    /// </summary>
    Task BatchDeleteAsync(List<Guid> ids);

    /// <summary>
    /// 获取部门下所有成员
    /// </summary>
    Task<List<MemberDto>> GetMembersByOrganizationIdAsync(Guid organizationId);

    /// <summary>
    /// 检查用户是否已是部门成员
    /// </summary>
    Task<bool> ExistsAsync(Guid organizationId, Guid userId);
}
