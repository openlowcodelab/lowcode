using H.Abp.Application.Contracts;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// AI 基础能力服务接口
/// 基于 Assistant 应用配置的默认模型，向其它应用提供统一的文本生成能力
/// </summary>
public interface IAiCompletionAppService : IAppService
{
    /// <summary>
    /// 文本生成（同步，单轮对话）
    /// </summary>
    Task<AiCompletionResultDto> CompleteAsync(AiCompletionInputDto input);
}
