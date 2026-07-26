using H.Abp.Application.Contracts;

namespace H.Enterprise.Application.Contracts;

/// <summary>
/// 企业管理服务接口
/// </summary>
public interface IEnterpriseAppService : IAppService
{
    /// <summary>
    /// 获取企业列表（分页）
    /// </summary>
    Task<PagedResult<EnterpriseDto>> GetListAsync(EnterpriseQueryParams queryParams);

    /// <summary>
    /// 根据 ID 获取企业详情
    /// </summary>
    Task<EnterpriseDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// 创建企业（用户自行注册，Status=Pending）
    /// </summary>
    Task<EnterpriseDto> CreateAsync(CreateEnterpriseDto input);

    /// <summary>
    /// 更新企业信息
    /// </summary>
    Task<EnterpriseDto> UpdateAsync(Guid id, UpdateEnterpriseDto input);

    /// <summary>
    /// 更新当前企业信息（仅企业拥有者 Owner 可操作）
    /// </summary>
    Task<EnterpriseDto> UpdateCurrentEnterpriseAsync(UpdateEnterpriseDto input);

    /// <summary>
    /// 删除企业（仅 Pending/Disabled 可删除）
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// 激活企业（超管操作，设置数据库模式）
    /// </summary>
    Task ActivateAsync(Guid id, ActivateEnterpriseDto input);

    /// <summary>
    /// 启用企业
    /// </summary>
    Task EnableAsync(Guid id);

    /// <summary>
    /// 禁用企业
    /// </summary>
    Task DisableAsync(Guid id);

    /// <summary>
    /// 获取当前用户关联的所有企业列表
    /// </summary>
    Task<List<EnterpriseDto>> GetMyEnterprisesAsync(Guid userId);

    /// <summary>
    /// 选择企业（更新 Cookie Claims，设置 TenantId）
    /// </summary>
    Task SelectEnterpriseAsync(Guid enterpriseId);

    /// <summary>
    /// 登录后自动选择企业：
    /// 当用户仅有 1 个可用企业，或存在上一次登录（默认）企业时，直接选择并进入该企业；
    /// 否则不做选择，由调用方引导用户进入企业选择页。
    /// </summary>
    Task<EnterpriseAutoSelectResultDto> AutoSelectEnterpriseAsync();

    /// <summary>
    /// 获取当前选择的企业信息
    /// </summary>
    Task<EnterpriseDto?> GetCurrentEnterpriseAsync();

    /// <summary>
    /// 获取当前用户在当前企业中的角色（Owner/Admin/Member 等），未选择企业时返回 null
    /// </summary>
    Task<string?> GetCurrentEnterpriseRoleAsync();
}
