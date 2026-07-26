namespace H.Abp.HttpClientProxy;

/// <summary>
/// 远程服务配置项
/// </summary>
public class RemoteServiceConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
}

/// <summary>
/// 远程服务配置集合，从 appsettings.json 的 "RemoteServices" 节点读取
/// </summary>
public class RemoteServiceOptions
{
    private readonly Dictionary<string, RemoteServiceConfiguration> _services = new(StringComparer.OrdinalIgnoreCase);

    public RemoteServiceConfiguration this[string name]
    {
        get => _services.TryGetValue(name, out var config) ? config : new RemoteServiceConfiguration();
        set => _services[name] = value;
    }

    public void Configure(string name, string baseUrl)
    {
        _services[name] = new RemoteServiceConfiguration { BaseUrl = baseUrl.TrimEnd('/') };
    }

    public string GetBaseUrl(string serviceName)
    {
        return _services.TryGetValue(serviceName, out var config) ? config.BaseUrl : string.Empty;
    }
}
