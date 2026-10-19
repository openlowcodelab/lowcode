using System.Net;

namespace H.Abp.HttpClientProxy;

/// <summary>
/// 远程服务返回非成功状态码时抛出的异常，Message 为服务端下发的可读错误信息
/// </summary>
public class RemoteServiceException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public RemoteServiceException(HttpStatusCode statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
