using H.Enterprise.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;

namespace H.Enterprise.Application.Contracts.Services;

/// <summary>
/// 企业管理服务接口
/// </summary>
public interface IEnterpriseAppService : IApplicationService
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
    /// 获取当前选择的企业信息
    /// </summary>
    Task<EnterpriseDto?> GetCurrentEnterpriseAsync();
}
