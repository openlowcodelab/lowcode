using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试项目 AI 服务接口
/// 基于口语化描述生成/变更测试项目（分类、用例），AI 基础能力依赖 Assistant 应用
/// </summary>
public interface IAiGenerateAppService : IAppService
{
    /// <summary>
    /// 根据口语化描述生成测试项目草稿（含分类与用例，不落库）
    /// </summary>
    Task<BaseOutput<AiGeneratedProjectDto>> GenerateProjectAsync(AiGenerateInputDto input);

    /// <summary>
    /// 确认并落库 AI 生成的测试项目（项目 + 分类 + 用例）
    /// </summary>
    Task<BaseOutput<long>> CreateProjectFromAiAsync(AiGeneratedProjectDto generated);

    /// <summary>
    /// 根据口语化描述生成已有项目的变更计划（新增/修改 分类与用例，不落库）
    /// </summary>
    Task<BaseOutput<AiModificationPlanDto>> GenerateModificationAsync(long projectId, AiGenerateInputDto input);

    /// <summary>
    /// 确认并应用 AI 变更计划
    /// </summary>
    Task<BaseOutput> ApplyModificationAsync(long projectId, AiModificationPlanDto plan);
}
