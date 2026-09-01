using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// CI 集成服务接口：供流水线通过 HTTP 触发执行并轮询结果
/// </summary>
public interface ICiAppService : IAppService
{
    /// <summary>
    /// 触发一次执行（令牌校验通过后进入后台执行队列），返回运行ID
    /// </summary>
    Task<BaseOutput<long>> TriggerAsync(CiTriggerDto dto);

    /// <summary>
    /// 查询运行状态与结果（需令牌）
    /// </summary>
    Task<BaseOutput<CiRunDto?>> GetRunAsync(long runId, string token);
}

/// <summary>
/// CI 运行执行器。由 Hangfire 作业服务器反射调用，需保证方法签名稳定。
/// </summary>
public interface ICiJobExecutor
{
    /// <summary>
    /// 执行指定的 CI 运行
    /// </summary>
    Task ExecuteAsync(long runId);
}
