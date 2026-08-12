using H.Approval.Application.Contracts;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Volo.Abp.DependencyInjection;

namespace H.Approval.Application;

/// <summary>
/// 审批模板提供者。
/// <para>从内嵌 JSON 资源(approval-templates.json)加载预置审批模板,供"从模板创建"使用。模板不落库。</para>
/// </summary>
public class ApprovalTemplateProvider : ISingletonDependency
{
    /// <summary>内嵌资源逻辑名(见 csproj EmbeddedResource LogicalName)</summary>
    private const string ResourceName = "H.Approval.Templates.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ILogger<ApprovalTemplateProvider> _logger;
    private readonly Lazy<List<ApprovalTemplateDto>> _templates;

    public ApprovalTemplateProvider(ILogger<ApprovalTemplateProvider> logger)
    {
        _logger = logger;
        _templates = new Lazy<List<ApprovalTemplateDto>>(LoadTemplates);
    }

    /// <summary>
    /// 获取全部预置模板(首次访问时加载并缓存)。
    /// </summary>
    public List<ApprovalTemplateDto> GetTemplates() => _templates.Value;

    private List<ApprovalTemplateDto> LoadTemplates()
    {
        try
        {
            var assembly = typeof(ApprovalTemplateProvider).Assembly;
            using var stream = assembly.GetManifestResourceStream(ResourceName);
            if (stream == null)
            {
                _logger.LogWarning("未找到审批模板资源: {Resource}", ResourceName);
                return new List<ApprovalTemplateDto>();
            }

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var root = JsonSerializer.Deserialize<TemplateRoot>(json, JsonOptions) ?? new TemplateRoot();

            // 所有模板共用同一套默认审批流(发起人 -> 审批人)
            var defaultFlow = BuildDefaultFlow();

            var result = new List<ApprovalTemplateDto>();
            foreach (var category in root.Categories.OrderBy(c => c.Sort))
            {
                foreach (var d in category.Templates.OrderBy(t => t.Sort))
                {
                    var fields = d.Fields ?? new List<FormFieldModel>();
                    foreach (var field in fields)
                    {
                        if (string.IsNullOrEmpty(field.Id))
                        {
                            field.Id = Guid.NewGuid().ToString("N")[..8];
                        }
                    }

                    result.Add(new ApprovalTemplateDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Icon = d.Icon,
                        CategoryName = category.Name,
                        CategorySort = category.Sort,
                        Sort = d.Sort,
                        Description = d.Description,
                        FormJson = new FormSchema { Fields = fields }.Serialize(),
                        DefinitionJson = defaultFlow
                    });
                }
            }

            _logger.LogInformation("已加载 {Count} 个审批模板", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载审批模板失败");
            return new List<ApprovalTemplateDto>();
        }
    }

    /// <summary>
    /// 构建默认审批流: 发起人 -> 审批人(部门主管,依次) -> 结束(隐式)。
    /// </summary>
    private static string BuildDefaultFlow()
    {
        var root = new StartNodeModel
        {
            Id = Guid.NewGuid().ToString(),
            NodeName = "发起人",
            StartType = StartTypeEnum.All,
            ChildNodes = new List<NodeModelBase>
            {
                new ApproveModel
                {
                    Id = Guid.NewGuid().ToString(),
                    NodeName = "审批人",
                    ApproverType = ApproverTypeEnum.DepartmentManager,
                    ApproverMode = ApproverModeEnum.Sequential
                }
            }
        };
        return NodeSerializer.Serialize(root);
    }

    /// <summary>
    /// JSON 模板根节点(分类 + 模板子节点)。
    /// </summary>
    private class TemplateRoot
    {
        public List<TemplateCategory> Categories { get; set; } = new();
    }

    /// <summary>
    /// JSON 模板分类节点。
    /// </summary>
    private class TemplateCategory
    {
        public string Name { get; set; } = string.Empty;
        public int Sort { get; set; }
        public List<TemplateDefinition> Templates { get; set; } = new();
    }

    /// <summary>
    /// JSON 模板反序列化中间模型。
    /// </summary>
    private class TemplateDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int Sort { get; set; }
        public string? Description { get; set; }
        public List<FormFieldModel>? Fields { get; set; }
    }
}
