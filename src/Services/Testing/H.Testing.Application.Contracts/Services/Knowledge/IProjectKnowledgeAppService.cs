using H.Abp.Application.Contracts;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试项目知识库应用服务
/// 每个测试项目绑定一个 Assistant 知识库（懒创建），描述项目功能与逻辑，辅助编写测试用例
/// </summary>
public interface IProjectKnowledgeAppService : IAppService
{
    /// <summary>
    /// 获取项目关联的知识库（不存在时自动创建并绑定）
    /// </summary>
    Task<ProjectKnowledgeDto> GetOrCreateAsync(long projectId);

    /// <summary>
    /// 获取知识库节点树
    /// </summary>
    Task<List<ProjectKnowledgeNodeDto>> GetTreeAsync(long projectId);

    /// <summary>
    /// 创建节点（目录或文档）
    /// </summary>
    Task<ProjectKnowledgeNodeDto> CreateNodeAsync(long projectId, CreateProjectKnowledgeNodeInput input);

    /// <summary>
    /// 重命名节点
    /// </summary>
    Task<ProjectKnowledgeNodeDto> RenameNodeAsync(Guid nodeId, string title);

    /// <summary>
    /// 删除节点（含子节点）
    /// </summary>
    Task DeleteNodeAsync(Guid nodeId);

    /// <summary>
    /// 获取文档内容
    /// </summary>
    Task<string> GetDocumentContentAsync(Guid nodeId);

    /// <summary>
    /// 保存文档内容
    /// </summary>
    Task SaveDocumentAsync(Guid nodeId, string content);

    /// <summary>
    /// AI 口语化描述生成知识文档内容（预览后由前端保存）
    /// </summary>
    Task<AiKnowledgeDocumentDto> GenerateWithAiAsync(long projectId, string description);

    /// <summary>
    /// 读取知识库全部文档内容（标题 + 内容拼接），供 AI 生成用例时作为上下文
    /// </summary>
    Task<string> GetKnowledgeDigestAsync(long projectId, int maxLength = 10000);
}

/// <summary>
/// 创建知识库节点输入
/// </summary>
public class CreateProjectKnowledgeNodeInput
{
    /// <summary> 父节点 ID（根节点为空） </summary>
    public Guid? ParentId { get; set; }

    /// <summary> 节点标题 </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary> Directory / Document </summary>
    public string NodeType { get; set; } = "Document";
}
