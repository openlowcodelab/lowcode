using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Organization.Application.Contracts;

/// <summary>
/// 成员服务接口
/// </summary>
public interface IMemberAppService : IAppService
{
    /// <summary>
    /// 获取成员列表
    /// </summary>
    Task<BaseOutput<PagedResult<MemberDto>>> GetListAsync(MemberQueryParams queryParams);

    /// <summary>
    /// 获取成员详情
    /// </summary>
    Task<BaseOutput<MemberDto>> GetByIdAsync(Guid id);

    /// <summary>
    /// 添加成员（从Account服务获取用户信息）
    /// </summary>
    Task<BaseOutput<MemberDto>> AddAsync(AddMemberDto input);

    /// <summary>
    /// 批量添加成员（一个用户关联多个部门）
    /// </summary>
    Task<BaseOutput<List<MemberDto>>> AddBatchAsync(AddMemberBatchDto input);

    /// <summary>
    /// 搜索可分配用户（用于成员选择器）
    /// </summary>
    Task<BaseOutput<List<AssignableUserDto>>> SearchAssignableUsersAsync(string? keyword);

    /// <summary>
    /// 为成员分配角色（全量重建）
    /// </summary>
    Task<BaseOutput> AssignRolesAsync(Guid memberId, AssignMemberRolesDto input);

    /// <summary>
    /// 获取成员已授角色ID列表
    /// </summary>
    Task<BaseOutput<List<Guid>>> GetMemberRoleIdsAsync(Guid memberId);

    /// <summary>
    /// 更新成员
    /// </summary>
    Task<BaseOutput<MemberDto>> UpdateAsync(Guid id, UpdateMemberDto input);

    /// <summary>
    /// 删除成员
    /// </summary>
    Task<BaseOutput> DeleteAsync(Guid id);

    /// <summary>
    /// 批量删除成员
    /// </summary>
    Task<BaseOutput> BatchDeleteAsync(List<Guid> ids);

    /// <summary>
    /// 获取部门下所有成员
    /// </summary>
    Task<BaseOutput<List<MemberDto>>> GetMembersByOrganizationIdAsync(Guid organizationId);

    /// <summary>
    /// 检查用户是否已是部门成员
    /// </summary>
    Task<BaseOutput<bool>> ExistsAsync(Guid organizationId, Guid userId);
}
