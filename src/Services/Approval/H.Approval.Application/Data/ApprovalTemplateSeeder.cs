using H.Approval.Application.Contracts;
using H.Approval.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace H.Approval.Application.Data;

/// <summary>
/// 常见审批模板数据初始化(应用启动时幂等播种)
/// <para>创建 3 个分类(报销/出勤休假/其它)与 8 个预置审批定义(含表单与默认审批流)。</para>
/// </summary>
public class ApprovalTemplateSeeder : ITransientDependency
{
    private readonly ILogger<ApprovalTemplateSeeder> _logger;
    private readonly IApprovalCategoryRepository _categoryRepository;
    private readonly IApprovalDefinitionRepository _definitionRepository;

    public ApprovalTemplateSeeder(
        ILogger<ApprovalTemplateSeeder> logger,
        IApprovalCategoryRepository categoryRepository,
        IApprovalDefinitionRepository definitionRepository)
    {
        _logger = logger;
        _categoryRepository = categoryRepository;
        _definitionRepository = definitionRepository;
    }

    public async Task SeedAsync()
    {
        // 1. 确保分类存在(按名称幂等)
        var reimburse = await EnsureCategoryAsync("报销", 1);
        var attendance = await EnsureCategoryAsync("出勤休假", 2);
        var others = await EnsureCategoryAsync("其它", 3);

        // 2. 已有定义按名称判重
        var existing = await _definitionRepository.GetAllAsync();
        var existingNames = existing.Select(d => d.Name).ToHashSet();

        var templates = BuildTemplates(reimburse, attendance, others);

        var inserted = 0;
        foreach (var template in templates)
        {
            if (existingNames.Contains(template.Name))
            {
                continue;
            }
            await _definitionRepository.InsertAsync(template);
            inserted++;
        }

        if (inserted > 0)
        {
            _logger.LogInformation("已初始化 {Count} 个常见审批模板", inserted);
        }
    }

    private async Task<ApprovalCategory> EnsureCategoryAsync(string name, int sort)
    {
        var existing = await _categoryRepository.GetByNameAsync(name);
        if (existing != null)
        {
            return existing;
        }

        var entity = new ApprovalCategory(Guid.NewGuid().ToString())
        {
            Name = name,
            Sort = sort,
            CreationTime = DateTime.Now
        };
        await _categoryRepository.InsertAsync(entity);
        _logger.LogInformation("已创建审批分类: {Name}", name);
        return entity;
    }

    private static List<ApprovalDefinition> BuildTemplates(
        ApprovalCategory reimburse, ApprovalCategory attendance, ApprovalCategory others)
    {
        return new List<ApprovalDefinition>
        {
            // ===== 报销类 =====
            MakeDef("费用报销", "💰", reimburse, "员工日常费用报销申请", new List<FormFieldModel>
            {
                Field(FormFieldTypes.Radio, "报销类型", required: true,
                    options: new List<string> { "差旅费", "办公用品", "招待费", "其它" }),
                Field(FormFieldTypes.Amount, "报销金额", required: true, placeholder: "请输入报销金额"),
                Field(FormFieldTypes.Textarea, "费用明细", placeholder: "请填写费用明细"),
                Field(FormFieldTypes.Input, "备注", placeholder: "选填")
            }),

            // ===== 出勤休假类 =====
            MakeDef("请假", "🌴", attendance, "请假申请(自动计算请假天数)", new List<FormFieldModel>
            {
                Field(FormFieldTypes.Radio, "请假类型", required: true,
                    options: new List<string> { "年假", "事假", "病假", "调休", "婚假", "产假", "陪产假", "丧假" }),
                Field(FormFieldTypes.DateRange, "请假时间", required: true),
                Field(FormFieldTypes.Textarea, "请假事由", required: true, placeholder: "请填写请假事由")
            }),
            MakeDef("出差", "✈️", attendance, "出差申请(自动计算出差天数)", new List<FormFieldModel>
            {
                Field(FormFieldTypes.Input, "出差地点", required: true, placeholder: "请输入出差地点"),
                Field(FormFieldTypes.DateRange, "出差时间", required: true),
                Field(FormFieldTypes.Radio, "交通工具",
                    options: new List<string> { "飞机", "高铁", "汽车", "其它" }),
                Field(FormFieldTypes.Textarea, "出差事由", placeholder: "请填写出差事由")
            }),
            MakeDef("外出", "🚶", attendance, "工作时间外出申请", new List<FormFieldModel>
            {
                Field(FormFieldTypes.DateRange, "外出时间", required: true),
                Field(FormFieldTypes.Textarea, "外出事由", required: true, placeholder: "请填写外出事由")
            }),
            MakeDef("补卡", "🔄", attendance, "漏打卡补卡申请", new List<FormFieldModel>
            {
                Field(FormFieldTypes.Date, "补卡日期", required: true),
                Field(FormFieldTypes.Radio, "补卡时段", required: true,
                    options: new List<string> { "上班", "下班" }),
                Field(FormFieldTypes.Textarea, "补卡原因", required: true, placeholder: "请填写补卡原因")
            }),
            MakeDef("加班", "⏰", attendance, "加班申请", new List<FormFieldModel>
            {
                Field(FormFieldTypes.DateRange, "加班时间", required: true),
                Field(FormFieldTypes.Textarea, "加班事由", required: true, placeholder: "请填写加班事由")
            }),

            // ===== 其它 =====
            MakeDef("物品领用", "📦", others, "办公物品领用申请", new List<FormFieldModel>
            {
                Field(FormFieldTypes.Input, "领用物品", required: true, placeholder: "请输入领用物品名称"),
                Field(FormFieldTypes.Number, "领用数量", required: true, placeholder: "请输入数量"),
                Field(FormFieldTypes.Textarea, "领用用途", placeholder: "请填写领用用途")
            }),
            MakeDef("通用审批", "📝", others, "通用事项审批", new List<FormFieldModel>
            {
                Field(FormFieldTypes.Textarea, "审批事项", required: true, placeholder: "请填写需要审批的事项")
            })
        };
    }

    private static ApprovalDefinition MakeDef(
        string name, string icon, ApprovalCategory category, string? description, List<FormFieldModel> fields)
    {
        return new ApprovalDefinition(Guid.NewGuid().ToString())
        {
            Name = name,
            Icon = icon,
            Description = description,
            CategoryId = category.Id,
            CategoryName = category.Name,
            FormJson = new FormSchema { Fields = fields }.Serialize(),
            DefinitionJson = BuildDefaultFlow(),
            WhoCanStart = "All",
            AdminType = "All",
            Version = 1,
            IsEnabled = true,
            CreationTime = DateTime.Now
        };
    }

    private static FormFieldModel Field(
        string type, string label, bool required = false, string? placeholder = null, List<string>? options = null)
    {
        return new FormFieldModel
        {
            Id = Guid.NewGuid().ToString("N")[..8],
            Type = type,
            Label = label,
            Required = required,
            Placeholder = placeholder,
            Options = options ?? new List<string>()
        };
    }

    /// <summary>
    /// 构建默认审批流: 发起人 -> 审批人(部门主管,依次) -> 结束(隐式)。
    /// 部门主管在引擎中暂回退为发起人,保证开箱即用。
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
}
