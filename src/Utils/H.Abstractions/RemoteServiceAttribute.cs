namespace H.Abstractions;

/// <summary>
/// 标记远程服务名称，用于指定接口对应的远程服务配置
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false)]
public class RemoteServiceAttribute : Attribute
{
    public string? Name { get; set; }

    public RemoteServiceAttribute()
    {
    }

    public RemoteServiceAttribute(string name)
    {
        Name = name;
    }
}
