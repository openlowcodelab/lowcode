using H.SupplyChain.Application.Contracts;

namespace H.SupplyChain.Application.Services;

/// <summary>
/// 供应商接口调用工厂：按协议枚举返回对应的 <see cref="ISupplierApiInvoker"/> 实现。
/// 新增协议时只需在 DI 中注册新的 ISupplierApiInvoker 实现并暴露对应 Protocol。
/// </summary>
public class SupplierApiInvokerFactory : ISupplierApiInvokerFactory
{
    private readonly IEnumerable<ISupplierApiInvoker> _invokers;

    public SupplierApiInvokerFactory(IEnumerable<ISupplierApiInvoker> invokers)
    {
        _invokers = invokers;
    }

    public ISupplierApiInvoker Get(SupplierProtocolEnum protocol)
    {
        var invoker = _invokers.FirstOrDefault(c => c.Protocol == protocol);
        if (invoker is null)
        {
            throw new InvalidOperationException($"未注册供应商协议 {protocol} 的 ISupplierApiInvoker 实现");
        }
        return invoker;
    }
}