using H.Abp.Application.Contracts;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Base;

namespace H.LowCode.DesignEngine.Application.Contracts;

/// <summary>
/// 应用 AI 生成服务接口
/// 基于口语化描述生成应用/页面/菜单/数据源，AI 基础能力依赖 Assistant 应用
/// </summary>
public interface IAppAiGenerateAppService : IAppService
{
    /// <summary>
    /// 我的应用-创建应用：根据口语化描述生成应用草稿（应用信息+页面+菜单+数据源，不落库）
    /// </summary>
    Task<BaseOutput<AiGeneratedAppDto>> GenerateAppAsync(AiGenerateInputDto input);

    /// <summary>
    /// 确认并落库 AI 生成的应用（应用+页面+菜单+数据源）
    /// </summary>
    Task<BaseOutput<AppPartsSchema>> CreateAppFromAiAsync(AiGeneratedAppDto generated);

    /// <summary>
    /// 页面管理：为已有应用生成页面+菜单+数据源草稿（不落库）
    /// </summary>
    Task<BaseOutput<AiGeneratedAppDto>> GenerateAppContentAsync(string appId, AiGenerateInputDto input);

    /// <summary>
    /// 确认并落库 AI 生成的页面+菜单+数据源
    /// </summary>
    Task<BaseOutput<bool>> CreateAppContentFromAiAsync(string appId, AiGeneratedAppDto generated);

    /// <summary>
    /// 页面设计器：根据口语化描述生成页面组件树（返回真实组件实例，不落库）
    /// </summary>
    Task<BaseOutput<List<ComponentPartsSchema>>> GeneratePageComponentsAsync(string appId, AiGenerateInputDto input);

    /// <summary>
    /// 我的物料-组件库：根据口语化描述生成组件物料修改草稿（返回完整 ComponentPartsSchema，不落库）
    /// </summary>
    Task<BaseOutput<ComponentPartsSchema>> GenerateComponentPartsAsync(string libraryId, string partsId, AiGenerateInputDto input);
}
