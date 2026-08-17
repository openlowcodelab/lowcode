using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试项目模板服务接口
/// 模板以 JSON 文件形式存储于 Testing data 目录（projects.json 索引 + 每模板一个子目录）
/// </summary>
public interface IProjectTemplateAppService : IAppService
{
    /// <summary>
    /// 获取所有模板
    /// </summary>
    Task<BaseOutput<List<ProjectTemplateDto>>> GetTemplatesAsync();

    /// <summary>
    /// 从模板创建项目（项目、服务、环境、分类、用例）
    /// </summary>
    Task<BaseOutput<long>> CreateProjectFromTemplateAsync(string templateId, string name, string? description);

    /// <summary>
    /// 将已有项目保存为模板，返回模板ID
    /// </summary>
    Task<BaseOutput<string>> SaveProjectAsTemplateAsync(long projectId, string name, string? description);

    /// <summary>
    /// 更新模板名称与描述
    /// </summary>
    Task<BaseOutput<bool>> UpdateTemplateAsync(string templateId, string name, string? description);

    /// <summary>
    /// 删除模板
    /// </summary>
    Task<BaseOutput<bool>> DeleteTemplateAsync(string templateId);
}
