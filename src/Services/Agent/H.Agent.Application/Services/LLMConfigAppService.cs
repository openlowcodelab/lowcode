using AutoMapper;
using H.Agent.Application.Contracts;
using H.Agent.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Agent.Application;

/// <summary>
/// LLM 配置服务实现
/// </summary>
public class LLMConfigAppService : ApplicationService, ILLMConfigAppService
{
    private readonly IRepository<LLMConfigEntity, Guid> _repository;
    private readonly IMapper _objectMapper;
    
    public LLMConfigAppService(IRepository<LLMConfigEntity, Guid> repository, IMapper objectMapper)
    {
        _repository = repository;
        _objectMapper = objectMapper;
    }
    
    public async Task<List<LLMConfigDto>> GetAllAsync()
    {
        var entities = await AsyncExecuter.ToListAsync(
            (await _repository.GetQueryableAsync()).OrderBy(x => x.ProviderName)
        );
        return _objectMapper.Map<List<LLMConfigEntity>, List<LLMConfigDto>>(entities);
    }
    
    public async Task<LLMConfigDto?> GetConfigAsync(string providerName, CancellationToken ct = default)
    {
        var entity = await AsyncExecuter.FirstOrDefaultAsync(
            (await _repository.GetQueryableAsync()).Where(x => x.ProviderName == providerName),
            ct
        );
        return entity == null ? null : _objectMapper.Map<LLMConfigEntity, LLMConfigDto>(entity);
    }
    
    public async Task<LLMConfigDto?> GetDefaultConfigAsync(CancellationToken ct = default)
    {
        var entity = await AsyncExecuter.FirstOrDefaultAsync(
            (await _repository.GetQueryableAsync()).Where(x => x.IsDefault),
            ct
        );
        return entity == null ? null : _objectMapper.Map<LLMConfigEntity, LLMConfigDto>(entity);
    }
    
    public async Task<LLMConfigDto> CreateAsync(CreateLLMConfigDto input)
    {
        // 如果设置为默认，取消其他默认
        if (input.IsEnabled)
        {
            await ClearDefaultAsync();
        }
        
        var entity = _objectMapper.Map<CreateLLMConfigDto, LLMConfigEntity>(input);
        entity.IsDefault = input.IsEnabled;
        
        await _repository.InsertAsync(entity);
        
        return _objectMapper.Map<LLMConfigEntity, LLMConfigDto>(entity);
    }
    
    public async Task<LLMConfigDto> UpdateAsync(Guid id, UpdateLLMConfigDto input)
    {
        var entity = await _repository.GetAsync(id);
        
        if (!string.IsNullOrEmpty(input.ApiKey))
            entity.ApiKey = input.ApiKey;
        if (input.ApiSecret != null)
            entity.ApiSecret = input.ApiSecret;
        if (input.BaseUrl != null)
            entity.BaseUrl = input.BaseUrl;
        
        entity.ProviderDisplayName = input.ProviderDisplayName;
        entity.Model = input.Model;
        entity.IsEnabled = input.IsEnabled;
        entity.MaxTokens = input.MaxTokens;
        entity.Temperature = input.Temperature;
        entity.TimeoutSeconds = input.TimeoutSeconds;
        entity.ExtraConfig = input.ExtraConfig;
        
        await _repository.UpdateAsync(entity);
        
        return _objectMapper.Map<LLMConfigEntity, LLMConfigDto>(entity);
    }
    
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
    
    public async Task SetDefaultAsync(string providerName)
    {
        await ClearDefaultAsync();
        
        var entity = await AsyncExecuter.FirstOrDefaultAsync(
            (await _repository.GetQueryableAsync()).Where(x => x.ProviderName == providerName)
        );
        
        if (entity != null)
        {
            entity.IsDefault = true;
            await _repository.UpdateAsync(entity);
        }
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
}
