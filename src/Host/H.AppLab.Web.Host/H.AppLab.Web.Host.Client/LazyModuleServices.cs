using Microsoft.Extensions.DependencyInjection;

namespace H.AppLab.Web.Host.Client;

/// <summary>
/// 懒加载模块服务注册表：模块程序集下载完成后，将其服务注册到独立的模块子容器，
/// 供 <see cref="CompositeServiceProvider"/> 回退解析。
/// （MEDI 根容器构建后不可追加注册，因此采用"根容器 + 模块子容器"组合解析）
/// </summary>
public sealed class LazyModuleRegistry : IDisposable
{
    private readonly List<ServiceProvider> _moduleProviders = [];
    private readonly HashSet<string> _registeredKeys = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>根容器（CompositeServiceProvider），供模块注册时转发基础服务</summary>
    public IServiceProvider? RootProvider { get; internal set; }

    /// <summary>
    /// 注册一个懒加载模块的服务（按 key 去重，重复调用直接忽略）。
    /// 每个模块构建独立子容器，后续模块加载不影响已有单例状态。
    /// </summary>
    public void RegisterModule(string key, Action<IServiceCollection, IServiceProvider> configure)
    {
        if (RootProvider is null)
        {
            throw new InvalidOperationException("LazyModuleRegistry.RootProvider 尚未初始化，无法注册懒加载模块服务");
        }

        if (!_registeredKeys.Add(key))
        {
            return;
        }

        var services = new ServiceCollection();
        configure(services, RootProvider);
        _moduleProviders.Add(services.BuildServiceProvider());
    }

    /// <summary>依次从各模块子容器解析服务，未命中返回 null</summary>
    public object? GetService(Type serviceType)
    {
        foreach (var provider in _moduleProviders)
        {
            var service = provider.GetService(serviceType);
            if (service is not null)
            {
                return service;
            }
        }
        return null;
    }

    public void Dispose()
    {
        foreach (var provider in _moduleProviders)
        {
            provider.Dispose();
        }
        _moduleProviders.Clear();
    }
}

/// <summary>
/// 组合式 ServiceProvider：优先从内部 MEDI 根容器解析，
/// 解析不到时回退到 <see cref="LazyModuleRegistry"/> 的模块子容器。
/// </summary>
internal sealed class CompositeServiceProvider(ServiceProvider inner, LazyModuleRegistry registry)
    : IServiceProvider, ISupportRequiredService, IServiceScopeFactory, IDisposable, IAsyncDisposable
{
    public object? GetService(Type serviceType)
    {
        // WebAssemblyHost 通过 IServiceScopeFactory 创建根作用域，
        // 必须拦截并返回组合工厂，否则作用域内解析将失去回退能力
        if (serviceType == typeof(IServiceScopeFactory))
        {
            return this;
        }
        return inner.GetService(serviceType) ?? registry.GetService(serviceType);
    }

    public object GetRequiredService(Type serviceType) =>
        GetService(serviceType)
        ?? throw new InvalidOperationException($"No service for type '{serviceType}' has been registered.");

    public IServiceScope CreateScope() => new CompositeServiceScope(inner.CreateScope(), registry);

    public void Dispose()
    {
        registry.Dispose();
        inner.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        registry.Dispose();
        await inner.DisposeAsync();
    }
}

/// <summary>
/// 组合式作用域：内部作用域解析不到的服务回退到模块子容器
/// </summary>
internal sealed class CompositeServiceScope(IServiceScope innerScope, LazyModuleRegistry registry)
    : IServiceScope, IServiceProvider, ISupportRequiredService, IServiceScopeFactory
{
    public IServiceProvider ServiceProvider => this;

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IServiceScopeFactory))
        {
            return this;
        }
        return innerScope.ServiceProvider.GetService(serviceType) ?? registry.GetService(serviceType);
    }

    public object GetRequiredService(Type serviceType) =>
        GetService(serviceType)
        ?? throw new InvalidOperationException($"No service for type '{serviceType}' has been registered.");

    IServiceScope IServiceScopeFactory.CreateScope() =>
        new CompositeServiceScope(
            innerScope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope(),
            registry);

    public void Dispose() => innerScope.Dispose();
}

/// <summary>
/// 容器工厂：构建内部 MEDI 容器并包装为 <see cref="CompositeServiceProvider"/>
/// </summary>
public sealed class CompositeServiceProviderFactory(LazyModuleRegistry registry) : IServiceProviderFactory<IServiceCollection>
{
    public IServiceCollection CreateBuilder(IServiceCollection services) => services;

    public IServiceProvider CreateServiceProvider(IServiceCollection services)
    {
        var inner = services.BuildServiceProvider();
        var composite = new CompositeServiceProvider(inner, registry);
        registry.RootProvider = composite;
        return composite;
    }
}
