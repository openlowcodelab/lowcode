using System;
using System.Collections.Generic;

namespace H.Agent.Application.Contracts;

/// <summary>
/// LLM 配置 DTO
/// </summary>
public class LLMConfigDto
{
    public Guid Id { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string? ApiSecret { get; set; }
    public string? BaseUrl { get; set; }
    public string Model { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool IsDefault { get; set; }
    public int MaxTokens { get; set; }
    public float Temperature { get; set; }
    public int TimeoutSeconds { get; set; }
    public string? ExtraConfig { get; set; }
    public DateTime CreationTime { get; set; }
}

/// <summary>
/// 创建 LLM 配置 DTO
/// </summary>
public class CreateLLMConfigDto
{
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string? ApiSecret { get; set; }
    public string? BaseUrl { get; set; }
    public string Model { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int MaxTokens { get; set; } = 2000;
    public float Temperature { get; set; } = 0.7f;
    public int TimeoutSeconds { get; set; } = 30;
    public string? ExtraConfig { get; set; }
}

/// <summary>
/// 更新 LLM 配置 DTO
/// </summary>
public class UpdateLLMConfigDto
{
    public string ProviderDisplayName { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? BaseUrl { get; set; }
    public string Model { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public int MaxTokens { get; set; }
    public float Temperature { get; set; }
    public int TimeoutSeconds { get; set; }
    public string? ExtraConfig { get; set; }
}
