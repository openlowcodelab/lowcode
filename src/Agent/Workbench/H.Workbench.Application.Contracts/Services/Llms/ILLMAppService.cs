using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Workbench.Application.Contracts;

/// <summary>
/// LLM 配置服务接口
/// </summary>
public interface ILLMAppService : IAppService
{
    /// <summary>
    /// 获取所有配置
    /// </summary>
    Task<BaseOutput<List<LLMDto>>> GetAllAsync();

    /// <summary>
    /// 获取指定 ID 的配置
    /// </summary>
    Task<BaseOutput<LLMDto?>> GetAsync(Guid id);

    /// <summary>
    /// 获取指定 Provider 配置
    /// </summary>
    Task<BaseOutput<LLMDto?>> GetConfigAsync(string providerName, CancellationToken ct = default);

    /// <summary>
    /// 获取默认 Provider 配置
    /// </summary>
    Task<BaseOutput<LLMDto?>> GetDefaultConfigAsync(CancellationToken ct = default);

    /// <summary>
    /// 创建配置
    /// </summary>
    Task<BaseOutput<LLMDto>> CreateAsync(CreateLLMDto input);

    /// <summary>
    /// 更新配置
    /// </summary>
    Task<BaseOutput<LLMDto>> UpdateAsync(Guid id, UpdateLLMDto input);

    /// <summary>
    /// 删除配置
    /// </summary>
    Task<BaseOutput> DeleteAsync(Guid id);

    /// <summary>
    /// 设置为默认 Provider
    /// </summary>
    Task<BaseOutput> SetDefaultAsync(string providerName);

    /// <summary>
    /// 获取含真实密钥的配置（仅限服务端进程内使用，不对外暴露 HTTP 端点）
    /// </summary>
    Task<BaseOutput<LLMDto?>> GetCredentialAsync(Guid id);

    /// <summary>
    /// 按 Provider 名称获取含真实密钥的配置（仅限服务端进程内使用）
    /// </summary>
    Task<BaseOutput<LLMDto?>> GetCredentialByProviderAsync(string providerName, CancellationToken ct = default);

    /// <summary>
    /// 获取默认配置的明文凭据（仅限服务端进程内使用）
    /// </summary>
    Task<BaseOutput<LLMDto?>> GetDefaultCredentialAsync(CancellationToken ct = default);
}
