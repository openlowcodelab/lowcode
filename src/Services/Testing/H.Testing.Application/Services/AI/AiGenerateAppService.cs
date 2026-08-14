using H.Assistant.Application.Contracts;
using H.Testing.Application.Contracts;
using System.Text.Json;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.Testing.Application;

/// <summary>
/// 测试项目 AI 服务
/// 基于口语化描述生成/变更测试项目（分类、用例），AI 基础能力依赖 Assistant 应用
/// </summary>
public class AiGenerateAppService : ApplicationService, IAiGenerateAppService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const int MaxNameLength = 20;
    private const int MaxCaseNameLength = 50;
    private const int MaxDescriptionLength = 100;
    private const int MaxCaseDescriptionLength = 200;

    private readonly IAiCompletionAppService _aiCompletion;
    private readonly IProjectAppService _projectService;
    private readonly ICaseCategoryAppService _categoryService;
    private readonly ICaseAppService _caseService;
    private readonly ICaseStepAppService _caseStepService;
    private readonly IProjectServiceConfigAppService _serviceConfigService;
    private readonly IProjectEnvAppService _environmentService;
    private readonly IProjectKnowledgeAppService _knowledgeService;

    public AiGenerateAppService(
        IAiCompletionAppService aiCompletion,
        IProjectAppService projectService,
        ICaseCategoryAppService categoryService,
        ICaseAppService caseService,
        ICaseStepAppService caseStepService,
        IProjectServiceConfigAppService serviceConfigService,
        IProjectEnvAppService environmentService,
        IProjectKnowledgeAppService knowledgeService)
    {
        _aiCompletion = aiCompletion;
        _projectService = projectService;
        _categoryService = categoryService;
        _caseService = caseService;
        _caseStepService = caseStepService;
        _serviceConfigService = serviceConfigService;
        _environmentService = environmentService;
        _knowledgeService = knowledgeService;
    }

    #region 生成测试项目

    public async Task<AiGeneratedProjectDto> GenerateProjectAsync(AiGenerateInputDto input)
    {
        EnsureDescription(input);

        var result = await _aiCompletion.CompleteAsync(new AiCompletionInputDto
        {
            SystemPrompt = CreateProjectSystemPrompt,
            UserMessage = input.Description,
            Temperature = 0.3f,
            MaxTokens = 16384
        });

        var generated = ParseJson<AiGeneratedProjectDto>(result.Content);
        if (generated == null || string.IsNullOrWhiteSpace(generated.Name))
        {
            throw new UserFriendlyException("AI 返回的项目结构无效，请补充需求描述后重试");
        }

        NormalizeGeneratedProject(generated);
        return generated;
    }

    public async Task<long> CreateProjectFromAiAsync(AiGeneratedProjectDto generated)
    {
        if (generated == null || string.IsNullOrWhiteSpace(generated.Name))
        {
            throw new UserFriendlyException("项目名称不能为空");
        }

        NormalizeGeneratedProject(generated);

        var projectId = await _projectService.CreateAsync(new ProjectDto
        {
            Name = generated.Name,
            Description = generated.Description,
            Status = ProjectStatus.Active
        });

        // 1. 创建被测服务（tempId → 服务ID）
        var serviceIdMap = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        foreach (var aiService in generated.Services.Where(s => !string.IsNullOrWhiteSpace(s.Name)))
        {
            var created = await _serviceConfigService.CreateProjectServiceAsync(new ProjectServiceDto
            {
                ProjectId = projectId,
                Name = Truncate(aiService.Name, 100),
                Description = Truncate(aiService.Description, 500)
            });
            if (!string.IsNullOrEmpty(aiService.TempId))
            {
                serviceIdMap[aiService.TempId] = created.Id;
            }
        }

        // 2. 创建环境与各服务的基础地址配置
        foreach (var aiEnv in generated.Environments.Where(e => !string.IsNullOrWhiteSpace(e.Name)))
        {
            var environmentId = await _environmentService.CreateAsync(new ProjectEnvDto
            {
                ProjectId = projectId,
                Name = Truncate(aiEnv.Name, 100),
                Description = Truncate(aiEnv.Description, 500),
                Type = ParseEnvironmentType(aiEnv.Type),
                Variables = aiEnv.Variables ?? []
            });

            foreach (var config in aiEnv.ServiceConfigs.Where(c => !string.IsNullOrWhiteSpace(c.BaseUrl)))
            {
                if (!serviceIdMap.TryGetValue(config.ServiceTempId ?? string.Empty, out var projectServiceId))
                {
                    continue;
                }

                await _serviceConfigService.CreateEnvironmentServiceConfigAsync(new ProjectEnvConfigDto
                {
                    EnvironmentId = environmentId,
                    ProjectServiceId = projectServiceId,
                    BaseUrl = config.BaseUrl.Trim()
                });
            }
        }

        // 3. 创建分类
        var tempIdMap = await CreateCategoriesInOrderAsync(
            projectId,
            generated.Categories.Select(c => (c.TempId, c.Name, ParentRef: c.ParentTempId)).ToList(),
            ResolveTempIdOnly);

        // 4. 创建用例（含测试步骤）
        foreach (var category in generated.Categories)
        {
            if (category.Cases == null || !tempIdMap.TryGetValue(category.TempId, out var categoryId))
            {
                continue;
            }

            foreach (var aiCase in category.Cases.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
            {
                var caseId = await _caseService.CreateAsync(new CaseDto
                {
                    Name = Truncate(aiCase.Name, MaxCaseNameLength),
                    Description = Truncate(aiCase.Description, MaxCaseDescriptionLength),
                    ProjectId = projectId,
                    CategoryId = categoryId,
                    Level = NormalizeLevel(aiCase.Level),
                    Status = CaseStatus.Active
                });

                var steps = MapSteps(aiCase.Steps, serviceIdMap);
                if (steps.Count > 0)
                {
                    await _caseStepService.SaveAsync(caseId, steps);
                }
            }
        }

        return projectId;
    }

    #endregion

    #region 变更已有项目

    public async Task<AiModificationPlanDto> GenerateModificationAsync(long projectId, AiGenerateInputDto input)
    {
        EnsureDescription(input);

        var project = await _projectService.GetByIdAsync(projectId)
            ?? throw new UserFriendlyException("项目不存在");
        var categories = FlattenCategoryTree(await _categoryService.GetByProjectIdAsync(projectId));
        var cases = await _caseService.GetByProjectIdAsync(projectId);

        var context = JsonSerializer.Serialize(new
        {
            project = new { project.Name, project.Description },
            categories = categories.Select(c => new { c.Id, c.Name, c.ParentId }),
            cases = cases.Select(c => new { c.Id, c.Name, c.CategoryId })
        }, JsonOptions);

        // 读取项目知识库内容作为生成上下文，辅助编写更贴合业务的用例
        var knowledgeDigest = await _knowledgeService.GetKnowledgeDigestAsync(projectId);
        var knowledgeSection = string.IsNullOrEmpty(knowledgeDigest)
            ? string.Empty
            : $"\n\n项目知识库（描述项目功能与逻辑，设计用例时应优先覆盖其中的关键流程与规则）：\n{knowledgeDigest}";

        var result = await _aiCompletion.CompleteAsync(new AiCompletionInputDto
        {
            SystemPrompt = ModificationSystemPrompt,
            UserMessage = $"已有项目结构：\n{context}{knowledgeSection}\n\n用户需求：\n{input.Description}",
            Temperature = 0.3f,
            MaxTokens = 8192
        });

        var plan = ParseJson<AiModificationPlanDto>(result.Content)
            ?? throw new UserFriendlyException("AI 返回的变更计划无效，请补充需求描述后重试");
        NormalizeModificationPlan(plan);
        return plan;
    }

    public async Task ApplyModificationAsync(long projectId, AiModificationPlanDto plan)
    {
        if (plan == null)
        {
            throw new UserFriendlyException("变更计划不能为空");
        }

        NormalizeModificationPlan(plan);

        var existingCategories = FlattenCategoryTree(await _categoryService.GetByProjectIdAsync(projectId));
        var existingCategoryIds = existingCategories.Select(c => c.Id).ToHashSet();

        // 1. 新增分类（父分类优先）
        var tempIdMap = await CreateCategoriesInOrderAsync(
            projectId,
            plan.AddCategories.Select(c => (c.TempId, c.Name, c.ParentRef)).ToList(),
            reference => ResolveCategoryRef(reference, existingCategoryIds));

        // 2. 修改分类
        foreach (var update in plan.UpdateCategories)
        {
            var current = existingCategories.FirstOrDefault(c => c.Id == update.Id);
            if (current == null)
            {
                continue;
            }

            await _categoryService.UpdateAsync(update.Id, new CaseCategoryDto
            {
                Id = current.Id,
                Name = string.IsNullOrWhiteSpace(update.Name) ? current.Name : Truncate(update.Name, MaxNameLength),
                ProjectId = projectId,
                ParentId = current.ParentId,
                Order = current.Order
            });
        }

        // 3. 新增用例
        foreach (var aiCase in plan.AddCases.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
        {
            await _caseService.CreateAsync(new CaseDto
            {
                Name = Truncate(aiCase.Name, MaxNameLength),
                Description = Truncate(aiCase.Description, MaxCaseDescriptionLength),
                ProjectId = projectId,
                CategoryId = ResolveCategoryRef(aiCase.CategoryRef, existingCategoryIds) ?? (tempIdMap.TryGetValue(aiCase.CategoryRef ?? string.Empty, out var newId) ? newId : null),
                Level = NormalizeLevel(aiCase.Level),
                Status = CaseStatus.Active
            });
        }

        // 4. 修改用例
        foreach (var update in plan.UpdateCases)
        {
            var current = await _caseService.GetByIdAsync(update.Id);
            if (current == null || current.ProjectId != projectId)
            {
                continue;
            }

            current.Name = string.IsNullOrWhiteSpace(update.Name) ? current.Name : Truncate(update.Name, MaxNameLength);
            current.Description = string.IsNullOrWhiteSpace(update.Description) ? current.Description : Truncate(update.Description, MaxCaseDescriptionLength);
            if (!string.IsNullOrWhiteSpace(update.Level))
            {
                current.Level = NormalizeLevel(update.Level);
            }

            await _caseService.UpdateAsync(update.Id, current);
        }
    }

    #endregion

    #region 分类创建与引用解析

    /// <summary>
    /// 按父子依赖顺序创建分类，返回 TempId → 实际ID 映射
    /// </summary>
    private async Task<Dictionary<string, long>> CreateCategoriesInOrderAsync(
        long projectId,
        List<(string TempId, string Name, string? ParentRef)> categories,
        Func<string?, long?> parentResolver)
    {
        var tempIdMap = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        var remaining = categories
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .ToList();
        var order = 0;

        var progress = true;
        while (remaining.Count > 0 && progress)
        {
            progress = false;
            foreach (var category in remaining.ToList())
            {
                // 父分类尚未创建时延后处理（先建父再建子）
                var isTempParent = !string.IsNullOrEmpty(category.ParentRef)
                    && !long.TryParse(category.ParentRef, out _)
                    && !tempIdMap.ContainsKey(category.ParentRef!);
                if (isTempParent)
                {
                    continue;
                }

                var created = await _categoryService.CreateAsync(new CaseCategoryDto
                {
                    Name = Truncate(category.Name, MaxNameLength),
                    ProjectId = projectId,
                    ParentId = ResolveParentId(category.ParentRef, tempIdMap, parentResolver),
                    Order = order++
                });

                if (!string.IsNullOrEmpty(category.TempId))
                {
                    tempIdMap[category.TempId] = created.Id;
                }

                remaining.Remove(category);
                progress = true;
            }
        }

        // 剩余分类的父引用无法解析（如循环引用），作为根分类创建
        foreach (var category in remaining)
        {
            var created = await _categoryService.CreateAsync(new CaseCategoryDto
            {
                Name = Truncate(category.Name, MaxNameLength),
                ProjectId = projectId,
                ParentId = null,
                Order = order++
            });

            if (!string.IsNullOrEmpty(category.TempId))
            {
                tempIdMap[category.TempId] = created.Id;
            }
        }

        return tempIdMap;
    }

    private static long? ResolveParentId(
        string? parentRef,
        Dictionary<string, long> tempIdMap,
        Func<string?, long?> existingResolver)
    {
        if (string.IsNullOrWhiteSpace(parentRef))
        {
            return null;
        }

        if (tempIdMap.TryGetValue(parentRef, out var newParentId))
        {
            return newParentId;
        }

        return existingResolver(parentRef);
    }

    /// <summary>
    /// 仅解析 TempId（创建全新项目场景，无已有分类）
    /// </summary>
    private static long? ResolveTempIdOnly(string? reference) => null;

    /// <summary>
    /// 解析分类引用：数字视为已有分类ID（需属于当前项目），否则返回 null
    /// </summary>
    private static long? ResolveCategoryRef(string? reference, HashSet<long> existingCategoryIds)
    {
        if (long.TryParse(reference, out var id) && existingCategoryIds.Contains(id))
        {
            return id;
        }

        return null;
    }

    #endregion

    #region 工具方法

    private static void EnsureDescription(AiGenerateInputDto input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.Description))
        {
            throw new UserFriendlyException("请输入需求描述");
        }
    }

    /// <summary>
    /// 从 AI 返回文本中提取 JSON（去除 markdown 代码块标记与多余文本）
    /// </summary>
    private static string ExtractJson(string content)
    {
        var text = (content ?? string.Empty).Trim();

        if (text.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewline = text.IndexOf('\n');
            if (firstNewline > 0)
            {
                text = text[(firstNewline + 1)..];
            }

            var lastFence = text.LastIndexOf("```", StringComparison.Ordinal);
            if (lastFence > 0)
            {
                text = text[..lastFence];
            }
        }

        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            throw new UserFriendlyException("AI 返回的内容无法解析，请重试");
        }

        return text[start..(end + 1)];
    }

    private static T? ParseJson<T>(string content)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(ExtractJson(content), JsonOptions);
        }
        catch (JsonException)
        {
            throw new UserFriendlyException("AI 返回的内容无法解析，请重试");
        }
    }

    private static void NormalizeGeneratedProject(AiGeneratedProjectDto generated)
    {
        generated.Name = Truncate(generated.Name, MaxNameLength);
        generated.Description = Truncate(generated.Description, MaxDescriptionLength);
        generated.Categories ??= [];
        generated.Services ??= [];
        generated.Environments ??= [];

        // 补齐缺失的 TempId，并过滤自引用
        for (var i = 0; i < generated.Categories.Count; i++)
        {
            var category = generated.Categories[i];
            if (string.IsNullOrWhiteSpace(category.TempId))
            {
                category.TempId = $"c{i + 1}";
            }

            if (category.ParentTempId == category.TempId)
            {
                category.ParentTempId = null;
            }

            category.Cases ??= [];
            foreach (var aiCase in category.Cases)
            {
                aiCase.Steps ??= [];
            }
        }

        for (var i = 0; i < generated.Services.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(generated.Services[i].TempId))
            {
                generated.Services[i].TempId = $"s{i + 1}";
            }
        }

        foreach (var environment in generated.Environments)
        {
            environment.ServiceConfigs ??= [];
        }
    }

    /// <summary>
    /// 将 AI 生成的步骤映射为用例步骤（api → HttpRequest，ui → 对应界面操作类型）
    /// </summary>
    private static List<CaseStepDto> MapSteps(List<AiGeneratedStepDto>? aiSteps, Dictionary<string, long> serviceIdMap)
    {
        var steps = new List<CaseStepDto>();
        if (aiSteps == null)
        {
            return steps;
        }

        var order = 0;
        foreach (var aiStep in aiSteps.Where(s => !string.IsNullOrWhiteSpace(s.Name)))
        {
            var step = new CaseStepDto
            {
                Name = Truncate(aiStep.Name, MaxNameLength),
                ExpectedResult = Truncate(aiStep.Description, MaxDescriptionLength),
                Order = order++
            };

            var type = (aiStep.Type ?? string.Empty).Trim().ToLowerInvariant();
            if (type is "api" or "http" or "httprequest")
            {
                step.Type = StepType.HttpRequest;
                step.ApiConfig = new ApiStepConfig
                {
                    Method = NormalizeHttpMethod(aiStep.Method),
                    Url = (aiStep.Url ?? string.Empty).Trim(),
                    Body = aiStep.Body ?? string.Empty,
                    ServiceId = serviceIdMap.TryGetValue(aiStep.ServiceRef ?? string.Empty, out var serviceId) ? serviceId : 0
                };
            }
            else
            {
                var action = (aiStep.Action ?? type).Trim().ToLowerInvariant();
                step.Type = MapUiAction(action);
                step.UiConfig = new UiStepConfig
                {
                    Action = action,
                    Selector = (aiStep.Selector ?? string.Empty).Trim(),
                    Value = aiStep.Value ?? string.Empty
                };
            }

            steps.Add(step);
        }

        return steps;
    }

    private static StepType MapUiAction(string action) => action switch
    {
        "navigate" or "open" => StepType.Navigate,
        "click" => StepType.Click,
        "input" or "type" => StepType.Input,
        "select" => StepType.Select,
        "wait" => StepType.Wait,
        "assert" or "verify" => StepType.Assert,
        "screenshot" => StepType.Screenshot,
        "scroll" => StepType.Scroll,
        "hover" => StepType.Hover,
        "keypress" or "key" => StepType.KeyPress,
        _ => StepType.Assert
    };

    private static string NormalizeHttpMethod(string? method)
    {
        var normalized = (method ?? string.Empty).Trim().ToUpperInvariant();
        return normalized is "GET" or "POST" or "PUT" or "DELETE" or "PATCH" or "HEAD" or "OPTIONS" ? normalized : "GET";
    }

    private static EnvironmentType ParseEnvironmentType(string? type) => (type ?? string.Empty).Trim().ToLowerInvariant() switch
    {
        "testing" or "test" => EnvironmentType.Testing,
        "staging" => EnvironmentType.Staging,
        "production" or "prod" => EnvironmentType.Production,
        _ => EnvironmentType.Development
    };

    private static void NormalizeModificationPlan(AiModificationPlanDto plan)
    {
        plan.AddCategories ??= [];
        plan.UpdateCategories ??= [];
        plan.AddCases ??= [];
        plan.UpdateCases ??= [];

        for (var i = 0; i < plan.AddCategories.Count; i++)
        {
            var category = plan.AddCategories[i];
            if (string.IsNullOrWhiteSpace(category.TempId))
            {
                category.TempId = $"c{i + 1}";
            }

            if (category.ParentRef == category.TempId)
            {
                category.ParentRef = null;
            }
        }
    }

    /// <summary>
    /// 归一化 AI 返回的用例级别（单选，无效时默认 P1）
    /// </summary>
    private static CaseLevel NormalizeLevel(string? level)
    {
        var value = level?.Trim().ToUpperInvariant();
        return value is "P0" or "P1" or "P2" or "P3" ? Enum.Parse<CaseLevel>(value) : CaseLevel.P1;
    }

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        value = value.Trim();
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static List<CaseCategoryDto> FlattenCategoryTree(IEnumerable<CaseCategoryDto> nodes)
    {
        var result = new List<CaseCategoryDto>();
        foreach (var node in nodes)
        {
            result.Add(node);
            if (node.Childrens is { Length: > 0 })
            {
                result.AddRange(FlattenCategoryTree(node.Childrens));
            }
        }

        return result;
    }

    private const string CreateProjectSystemPrompt = """
        你是一名资深软件测试架构师。请根据用户的口语化描述，设计一个完整的测试项目方案，包含项目信息、被测服务、测试环境、测试用例分类、测试用例与测试步骤。
        输出要求：
        1. 只输出一个 JSON 对象，不要输出任何解释文字，也不要使用 markdown 代码块标记。
        2. JSON 结构如下：
        {
          "name": "项目名称（简洁）",
          "description": "项目描述",
          "services": [
            { "tempId": "s1", "name": "服务名称（如用户服务）", "description": "服务说明" }
          ],
          "environments": [
            {
              "name": "环境名称（如开发环境）",
              "type": "development",
              "description": "环境说明，可为空字符串",
              "variables": { "变量名": "变量值" },
              "serviceConfigs": [{ "serviceTempId": "s1", "baseUrl": "http://localhost:8080" }]
            }
          ],
          "categories": [
            {
              "tempId": "c1",
              "name": "分类名称",
              "description": "分类描述，可为空字符串",
              "parentTempId": null,
              "cases": [
                {
                  "name": "用例名称",
                  "description": "测试要点与预期结果",
                  "level": "P0",
                  "steps": [
                    { "name": "步骤名称", "type": "api", "method": "POST", "url": "/api/login", "serviceRef": "s1", "body": "{\"username\":\"test\"}", "description": "预期结果" },
                    { "name": "步骤名称", "type": "ui", "action": "click", "selector": "#login-btn", "value": "", "description": "预期结果" }
                  ]
                }
              ]
            }
          ]
        }
        3. 规则：
        - services：按被测系统/服务划分（如 Web前端、用户服务、订单服务），tempId 依次为 s1、s2……。
        - environments：默认创建一个开发环境（type 从 development/testing/staging/production 中选）；用户描述中提到的地址要提取到对应环境的 serviceConfigs.baseUrl 中；未提供地址时 baseUrl 用合理的占位地址（如 http://localhost:8080）；常用配置可放入 variables。
        - categories：tempId 依次为 c1、c2……；子分类的 parentTempId 填写其父分类的 tempId，根分类为 null；分类层级最多 2 层；按功能模块或测试类型划分，数量建议 2~8 个。
        - cases：名称清晰可执行；description 包含关键测试点与预期结果；level 从 P0/P1/P2/P3 中单选一个，P0 为最重要；覆盖正常流程、异常场景与边界条件。
        - steps：每个用例设计 2~6 个具体可执行的步骤，按执行顺序排列；接口类项目用 type=api（method、url 为相对路径、serviceRef 引用服务 tempId、body 为 JSON 字符串、无请求体填空字符串）；界面类项目用 type=ui（action 从 navigate/click/input/select/wait/assert 中选，selector 为元素定位符，value 为输入值或期望值）；每个步骤的 description 写预期结果。
        - assert 步骤的 selector 应使用能定位到具体元素的 CSS 选择器（避免宽泛的标签组合），value 填写元素内应包含的预期文本，仅验证元素可见时 value 填空字符串，不要填 visible。
        - 所有名称与描述使用中文。
        """;

    private const string ModificationSystemPrompt = """
        你是一名资深软件测试架构师。用户已有一个测试项目，请根据用户的口语化描述，针对已有项目生成增量变更操作（新增/修改分类与测试用例）。
        输出要求：
        1. 只输出一个 JSON 对象，不要输出任何解释文字，也不要使用 markdown 代码块标记。
        2. JSON 结构如下：
        {
          "addCategories": [{ "tempId": "c1", "name": "分类名称", "description": "", "parentRef": null }],
          "updateCategories": [{ "id": 123, "name": "新名称", "description": "" }],
          "addCases": [{ "name": "用例名称", "description": "测试要点与预期结果", "level": "P1", "categoryRef": "c1" }],
          "updateCases": [{ "id": 456, "name": "新名称", "description": "", "level": "" }]
        }
        3. 规则：
        - parentRef 与 categoryRef 可填写已有分类的 id（数字），或本次新增分类的 tempId（如 c1）；根分类或未分类填 null。
        - tempId 依次为 c1、c2、c3……
        - 修改操作只填写需要变更的字段，不需要变更的字段填空字符串（空值将被忽略）。
        - 只输出必要的操作，无需变更时输出空数组；不要包含任何删除操作。
        - 新增用例应优先挂到合适的已有分类下；level 从 P0/P1/P2/P3 中单选一个。
        - 若提供了项目知识库内容，须优先依据知识库描述的功能与业务规则设计用例，使其覆盖知识库中提到的关键流程、校验规则与异常场景。
        - 所有名称与描述使用中文。
        """;

    #endregion
}
