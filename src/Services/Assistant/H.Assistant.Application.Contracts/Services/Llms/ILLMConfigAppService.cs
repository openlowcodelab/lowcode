using System;
using Volo.Abp.Application.Services;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// LLM 配置服务接口
/// </summary>
public interface ILLMConfigAppService : IApplicationService
{
    /// <summary>
    /// 获取所有配置
    /// </summary>
    Task<List<LLMConfigDto>> GetAllAsync();
    
    /// <summary>
    /// 获取指定 ID 的配置
    /// </summary>
    Task<LLMConfigDto?> GetAsync(Guid id);
    
    /// <summary>
    /// 获取指定 Provider 配置
    /// </summary>
    Task<LLMConfigDto?> GetConfigAsync(string providerName, CancellationToken ct = default);
    
    /// <summary>
    /// 获取默认 Provider 配置
    /// </summary>
    Task<LLMConfigDto?> GetDefaultConfigAsync(CancellationToken ct = default);
    
    /// <summary>
    /// 创建配置
    /// </summary>
    Task<LLMConfigDto> CreateAsync(CreateLLMConfigDto input);
    
    /// <summary>
    /// 更新配置
    /// </summary>
    Task<LLMConfigDto> UpdateAsync(Guid id, UpdateLLMConfigDto input);
    
    /// <summary>
    /// 删除配置
    /// </summary>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// 设置为默认 Provider
    /// </summary>
    Task SetDefaultAsync(string providerName);
}
