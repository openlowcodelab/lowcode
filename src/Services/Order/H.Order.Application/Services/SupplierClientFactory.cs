using H.Order.Application.Contracts;

namespace H.Order.Application.Services;

/// <summary>
/// 供应商客户端工厂：按协议枚举返回对应的 <see cref="ISupplierClient"/> 实现。
/// 新增协议时只需在 DI 中注册新的 ISupplierClient 实现并暴露对应 Protocol。
/// </summary>
public class SupplierClientFactory : ISupplierClientFactory
{
    private readonly IEnumerable<ISupplierClient> _clients;

    public SupplierClientFactory(IEnumerable<ISupplierClient> clients)
    {
        _clients = clients;
    }

    public ISupplierClient Get(SupplierProtocolEnum protocol)
    {
        var client = _clients.FirstOrDefault(c => c.Protocol == protocol);
        if (client is null)
        {
            throw new InvalidOperationException($"未注册供应商协议 {protocol} 的 ISupplierClient 实现");
        }
        return client;
    }
}