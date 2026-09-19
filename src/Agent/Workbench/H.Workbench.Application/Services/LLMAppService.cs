using AutoMapper;
using H.Workbench.Application.Contracts;
using H.Workbench.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Workbench.Application;

/// <summary>
/// LLM 配置服务实现。
/// 约定：所有对外方法输出的 ApiKey/ApiSecret 均为掩码；
/// 真实凭据只能经 GetCredentialAsync 系列（已禁用远程暴露）在服务端进程内获取。
/// </summary>
public class LLMAppService : ApplicationService, ILLMAppService
{
    private readonly IRepository<LLMEntity, Guid> _repository;
    private readonly IMapper _objectMapper;

    public LLMAppService(IRepository<LLMEntity, Guid> repository, IMapper objectMapper)
    {
        _repository = repository;
        _objectMapper = objectMapper;
    }

    public async Task<BaseOutput<List<LLMDto>>> GetAllAsync()
    {
        var entities = await AsyncExecuter.ToListAsync(
            (await _repository.GetQueryableAsync()).OrderBy(x => x.ProviderName)
        );
        return new(entities.Select(Masked).ToList());
    }

    public async Task<BaseOutput<LLMDto?>> GetConfigAsync(string providerName, CancellationToken ct = default)
    {
        var entity = await FindByProviderAsync(providerName, ct);
        return new(entity == null ? null : Masked(entity));
    }

    public async Task<BaseOutput<LLMDto?>> GetAsync(Guid id)
    {
        var entity = await _repository.FindAsync(id);
        return new(entity == null ? null : Masked(entity));
    }

    public async Task<BaseOutput<LLMDto?>> GetDefaultConfigAsync(CancellationToken ct = default)
    {
        var entity = await AsyncExecuter.FirstOrDefaultAsync(
            (await _repository.GetQueryableAsync()).Where(x => x.IsDefault),
            ct
        );
        return new(entity == null ? null : Masked(entity));
    }

    [RemoteService(IsEnabled = false)]
    public Task<BaseOutput<LLMDto?>> GetCredentialAsync(Guid id)
    {
        return GetRawAsync(id);
    }

    [RemoteService(IsEnabled = false)]
    public async Task<BaseOutput<LLMDto?>> GetCredentialByProviderAsync(string providerName, CancellationToken ct = default)
    {
        var entity = await FindByProviderAsync(providerName, ct);
        return new(entity == null ? null : _objectMapper.Map<LLMEntity, LLMDto>(entity));
    }

    [RemoteService(IsEnabled = false)]
    public async Task<BaseOutput<LLMDto?>> GetDefaultCredentialAsync(CancellationToken ct = default)
    {
        var entity = await AsyncExecuter.FirstOrDefaultAsync(
            (await _repository.GetQueryableAsync()).Where(x => x.IsDefault),
            ct
        );
        return new(entity == null ? null : _objectMapper.Map<LLMEntity, LLMDto>(entity));
    }

    private async Task<BaseOutput<LLMDto?>> GetRawAsync(Guid id)
    {
        var entity = await _repository.FindAsync(id);
        return new(entity == null ? null : _objectMapper.Map<LLMEntity, LLMDto>(entity));
    }

    public async Task<BaseOutput<LLMDto>> CreateAsync(CreateLLMDto input)
    {
        // 如果设置为默认，取消其他默认
        if (input.IsEnabled)
        {
            await ClearDefaultAsync();
        }

        var entity = _objectMapper.Map<CreateLLMDto, LLMEntity>(input);
        entity.IsDefault = input.IsEnabled;

        entity = await _repository.InsertAsync(entity);

        return new(Masked(entity));
    }

    public async Task<BaseOutput<LLMDto>> UpdateAsync(Guid id, UpdateLLMDto input)
    {
        var entity = await _repository.GetAsync(id);

        if (IsRealSecretSubmitted(input.ApiKey))
            entity.ApiKey = input.ApiKey;
        if (IsRealSecretSubmitted(input.ApiSecret))
            entity.ApiSecret = input.ApiSecret;
        else if (input.ApiSecret != null && input.ApiSecret.Length == 0)
            entity.ApiSecret = null;
        if (input.BaseUrl != null)
            entity.BaseUrl = input.BaseUrl;

        entity.ProviderDisplayName = input.ProviderDisplayName;
        entity.Model = input.Model;
        entity.IsEnabled = input.IsEnabled;
        entity.MaxTokens = input.MaxTokens;
        entity.Temperature = input.Temperature;
        entity.TimeoutSeconds = input.TimeoutSeconds;
        entity.ExtraConfig = input.ExtraConfig;

        entity = await _repository.UpdateAsync(entity);

        return new(Masked(entity));
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
        return new();
    }

    public async Task<BaseOutput> SetDefaultAsync(string providerName)
    {
        await ClearDefaultAsync();

        var entity = await FindByProviderAsync(providerName);

        if (entity != null)
        {
            entity.IsDefault = true;
            await _repository.UpdateAsync(entity);
        }

        return new();
    }

    private async Task<LLMEntity?> FindByProviderAsync(string providerName, CancellationToken ct = default)
    {
        return await AsyncExecuter.FirstOrDefaultAsync(
            (await _repository.GetQueryableAsync()).Where(x => x.ProviderName == providerName),
            ct
        );
    }

    private async Task ClearDefaultAsync()
    {
        var queryable = await _repository.GetQueryableAsync();
        var defaultConfigs = await AsyncExecuter.ToListAsync(queryable.Where(x => x.IsDefault));

        foreach (var config in defaultConfigs)
        {
            config.IsDefault = false;
            await _repository.UpdateAsync(config);
        }
    }

    private LLMDto Masked(LLMEntity entity)
    {
        var dto = _objectMapper.Map<LLMEntity, LLMDto>(entity);
        dto.ApiKeyConfigured = !string.IsNullOrEmpty(entity.ApiKey);
        dto.ApiKey = MaskSecret(dto.ApiKey);
        dto.ApiSecret = MaskSecret(dto.ApiSecret);
        return dto;
    }

    /// <summary>
    /// 掩码规则：保留末 4 位便于用户辨认是哪把钥匙
    /// </summary>
    internal static string? MaskSecret(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return "****" + (value.Length > 4 ? value[^4..] : "");
    }

    /// <summary>
    /// 前端回传的字符串只有"非空且不是掩码"才算真实修改了密钥
    /// </summary>
    internal static bool IsRealSecretSubmitted(string? value) =>
        !string.IsNullOrEmpty(value) && !value.Contains("****");
}
