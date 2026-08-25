using System.Text.Json;
using H.Assistant.Application.Contracts;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Base;
using H.Util.Ids;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Validation;

namespace H.LowCode.DesignEngine.Application;

/// <summary>
/// 应用 AI 生成服务
/// 基于口语化描述生成应用/页面/菜单/数据源与页面组件树，AI 基础能力依赖 Assistant 应用
/// </summary>
public class AppAiGenerateAppService : ApplicationService, IAppAiGenerateAppService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const int MaxNameLength = 30;
    private const int MaxDescriptionLength = 200;
    private const int MaxLabelLength = 50;
    private const int MaxPages = 10;
    private const int MaxDataSources = 15;
    private const int MaxMenus = 30;

    private readonly IAiCompletionAppService _aiCompletion;

    public AppAiGenerateAppService(IAiCompletionAppService aiCompletion)
    {
        _aiCompletion = aiCompletion;
    }

    private IAppRepository _appRepository => LazyServiceProvider.GetRequiredService<IAppRepository>();
    private IPageRepository _pageRepository => LazyServiceProvider.GetRequiredService<IPageRepository>();
    private IMenuRepository _menuRepository => LazyServiceProvider.GetRequiredService<IMenuRepository>();
    private IDataSourceRepository _dataSourceRepository => LazyServiceProvider.GetRequiredService<IDataSourceRepository>();
    private IComponentLibraryRepository _componentLibraryRepository => LazyServiceProvider.GetRequiredService<IComponentLibraryRepository>();
    private IComponentPartsRepository _componentPartsRepository => LazyServiceProvider.GetRequiredService<IComponentPartsRepository>();

    #region 我的应用-创建应用

    public async Task<BaseOutput<AiGeneratedAppDto>> GenerateAppAsync(AiGenerateInputDto input)
    {
        EnsureDescription(input);

        var result = (await _aiCompletion.CompleteAsync(new AiCompletionInputDto
        {
            SystemPrompt = await BuildSystemPromptAsync(AppScene.App),
            UserMessage = input.Description,
            Temperature = 0.3f,
            MaxTokens = 16384
        })).Data;

        var generated = ParseJson<AiGeneratedAppDto>(result!.Content);
        if (generated == null || string.IsNullOrWhiteSpace(generated.Name))
        {
            throw new UserFriendlyException("AI 返回的应用结构无效，请补充需求描述后重试");
        }

        NormalizeGenerated(generated, includeAppInfo: true);
        return new(generated);
    }

    [DisableValidation]
    public async Task<BaseOutput<AppPartsSchema>> CreateAppFromAiAsync(AiGeneratedAppDto generated)
    {
        if (generated == null || string.IsNullOrWhiteSpace(generated.Name))
        {
            throw new UserFriendlyException("应用名称不能为空");
        }

        NormalizeGenerated(generated, includeAppInfo: true);

        //1.创建应用
        var app = new AppPartsSchema
        {
            Id = ShortIdGenerator.Generate(),
            Name = Truncate(generated.Name, MaxNameLength),
            Description = Truncate(generated.Description, MaxDescriptionLength),
            PublishStatus = PublishStatusEnum.Development,
            Order = 0
        };
        await _appRepository.SaveAsync(app);

        var pageIdMap = await SavePagesMenusDataSourcesAsync(app.Id, generated);

        //默认首页
        if (pageIdMap.Count > 0)
        {
            app.HomePageId = pageIdMap.Values.First();
            await _appRepository.SaveAsync(app);
        }

        return new(app);
    }

    #endregion

    #region 页面管理

    public async Task<BaseOutput<AiGeneratedAppDto>> GenerateAppContentAsync(string appId, AiGenerateInputDto input)
    {
        ArgumentException.ThrowIfNullOrEmpty(appId);
        EnsureDescription(input);

        var app = await _appRepository.GetAsync(appId)
            ?? throw new UserFriendlyException($"应用不存在：{appId}");

        var existingContext = await BuildExistingContextAsync(appId);

        var result = (await _aiCompletion.CompleteAsync(new AiCompletionInputDto
        {
            SystemPrompt = await BuildSystemPromptAsync(AppScene.Content),
            UserMessage = $"应用名称：{app.Name}\n应用描述：{app.Description}{existingContext}\n\n用户需求：\n{input.Description}",
            Temperature = 0.3f,
            MaxTokens = 16384
        })).Data;

        var generated = ParseJson<AiGeneratedAppDto>(result!.Content)
            ?? throw new UserFriendlyException("AI 返回的内容无效，请补充需求描述后重试");

        if (generated.Pages.Count == 0 && generated.Menus.Count == 0 && generated.DataSources.Count == 0)
        {
            throw new UserFriendlyException("AI 未生成有效内容，请补充需求描述后重试");
        }

        NormalizeGenerated(generated, includeAppInfo: false);
        return new(generated);
    }

    [DisableValidation]
    public async Task<BaseOutput<bool>> CreateAppContentFromAiAsync(string appId, AiGeneratedAppDto generated)
    {
        ArgumentException.ThrowIfNullOrEmpty(appId);

        var app = await _appRepository.GetAsync(appId)
            ?? throw new UserFriendlyException($"应用不存在：{appId}");

        if (generated == null || (generated.Pages.Count == 0 && generated.Menus.Count == 0 && generated.DataSources.Count == 0))
        {
            throw new UserFriendlyException("AI 生成内容为空");
        }

        NormalizeGenerated(generated, includeAppInfo: false);

        await SavePagesMenusDataSourcesAsync(appId, generated);
        return new(true);
    }

    #endregion

    #region 页面设计器

    public async Task<BaseOutput<List<ComponentPartsSchema>>> GeneratePageComponentsAsync(string appId, AiGenerateInputDto input)
    {
        ArgumentException.ThrowIfNullOrEmpty(appId);
        EnsureDescription(input);

        var defines = await LoadComponentDefinesAsync();

        var result = (await _aiCompletion.CompleteAsync(new AiCompletionInputDto
        {
            SystemPrompt = BuildComponentsSystemPrompt(defines),
            UserMessage = input.Description,
            Temperature = 0.3f,
            MaxTokens = 16384
        })).Data;

        var spec = ParseJson<AiGeneratedPageDto>(result!.Content)
            ?? throw new UserFriendlyException("AI 返回的页面结构无效，请补充需求描述后重试");

        if (spec.Components.Count == 0)
        {
            throw new UserFriendlyException("AI 未生成任何组件，请补充需求描述后重试");
        }

        var components = BuildComponents(spec.Components, defines, out _);
        if (components.Count == 0)
        {
            throw new UserFriendlyException("AI 生成的组件均无法匹配可用物料，请调整描述后重试");
        }

        return new(components);
    }

    /// <inheritdoc />
    public async Task<BaseOutput<ComponentPartsSchema>> GenerateComponentPartsAsync(string libraryId, string partsId, AiGenerateInputDto input)
    {
        ArgumentException.ThrowIfNullOrEmpty(libraryId);
        ArgumentException.ThrowIfNullOrEmpty(partsId);
        EnsureDescription(input);

        var current = await _componentPartsRepository.GetByIdAsync(libraryId, partsId)
            ?? throw new UserFriendlyException($"组件物料不存在：{partsId}");

        //当前物料 JSON 作为修改上下文
        var currentJson = current.ToJson();

        var result = (await _aiCompletion.CompleteAsync(new AiCompletionInputDto
        {
            SystemPrompt = ComponentPartsSystemPrompt,
            UserMessage = $"当前组件物料 JSON：\n{currentJson}\n\n修改需求：\n{input.Description}",
            Temperature = 0.3f,
            MaxTokens = 16384
        })).Data;

        var draft = ParseJson<ComponentPartsSchema>(result!.Content)
            ?? throw new UserFriendlyException("AI 返回的组件结构无效，请调整描述后重试");

        if (draft.Fragment == null)
        {
            throw new UserFriendlyException("AI 返回的组件缺少渲染片段（frag），请调整描述后重试");
        }

        //身份字段不允许 AI 修改，强制回写
        draft.LibraryId = libraryId;
        draft.PartsId = partsId;
        draft.Id = current.Id;
        draft.ComponentType = current.ComponentType;

        if (string.IsNullOrWhiteSpace(draft.Label))
        {
            draft.Label = current.Label;
        }

        return new(draft);
    }

    #endregion

    #region 落库（页面/菜单/数据源）

    /// <summary>
    /// 落库页面+菜单+数据源（数据源先行，供组件绑定；返回 pageTempId → 实际页面Id）
    /// </summary>
    private async Task<Dictionary<string, string>> SavePagesMenusDataSourcesAsync(string appId, AiGeneratedAppDto generated)
    {
        var pageIdMap = new Dictionary<string, string>();

        //1.数据源（tempId → 实例，供组件绑定引用）
        var dataSourceMap = new Dictionary<string, DataSourceSchema>(StringComparer.OrdinalIgnoreCase);
        foreach (var dsSpec in generated.DataSources)
        {
            var ds = CreateDataSourceSchema(appId, dsSpec);
            await _dataSourceRepository.SaveAsync(appId, ds);
            dataSourceMap[dsSpec.TempId] = ds;
        }

        //2.页面（组件树按物料定义实例化 + 绑定数据源）
        var defines = await LoadComponentDefinesAsync();
        for (var i = 0; i < generated.Pages.Count; i++)
        {
            var pageSpec = generated.Pages[i];

            var page = new PagePartsSchema
            {
                AppId = appId,
                Id = ShortIdGenerator.Generate(),
                Name = string.IsNullOrWhiteSpace(pageSpec.Name) ? $"页面{i + 1}" : Truncate(pageSpec.Name, MaxNameLength),
                PageType = ParsePageType(pageSpec.PageType),
                Order = i,
                PublishStatus = 0
            };

            var components = BuildComponents(pageSpec.Components, defines, out var componentRefs);
            page.Components = BindComponentsDataSource(components, componentRefs, dataSourceMap);
            await _pageRepository.SaveAsync(page);

            if (!string.IsNullOrWhiteSpace(pageSpec.TempId))
            {
                pageIdMap[pageSpec.TempId] = page.Id;
            }
        }

        //3.菜单（父菜单优先创建；目录无页面地址，菜单地址指向关联页面Id）
        var menuTempIdMap = new Dictionary<string, MenuSchema>(StringComparer.OrdinalIgnoreCase);
        var pending = generated.Menus.ToList();
        var order = 0;

        while (pending.Count > 0)
        {
            var progress = false;
            foreach (var menuSpec in pending.ToArray())
            {
                //父菜单尚未创建时延后处理（先建父再建子）
                var parentRef = menuSpec.ParentTempId?.Trim();
                if (!string.IsNullOrEmpty(parentRef) && !menuTempIdMap.ContainsKey(parentRef!))
                {
                    continue;
                }

                var menu = new MenuSchema
                {
                    AppId = appId,
                    Id = ShortIdGenerator.Generate(),
                    ParentId = string.IsNullOrEmpty(parentRef) ? null : menuTempIdMap[parentRef!].Id,
                    Title = Truncate(menuSpec.Title, MaxNameLength),
                    Icon = menuSpec.Icon?.Trim(),
                    MenuType = menuSpec.MenuType == 1 ? 1 : 0,
                    MenuUrl = pageIdMap.TryGetValue(menuSpec.PageTempId ?? string.Empty, out var pageId) ? pageId : null,
                    Order = order++,
                    Childrens = []
                };
                await _menuRepository.SaveAsync(menu);

                menuTempIdMap[menuSpec.TempId] = menu;
                pending.Remove(menuSpec);
                progress = true;
            }

            //剩余菜单父引用无法解析（如循环引用），作为根菜单创建
            if (!progress)
            {
                foreach (var menuSpec in pending)
                {
                    var menu = new MenuSchema
                    {
                        AppId = appId,
                        Id = ShortIdGenerator.Generate(),
                        Title = Truncate(menuSpec.Title, MaxNameLength),
                        Icon = menuSpec.Icon?.Trim(),
                        MenuType = menuSpec.MenuType == 1 ? 1 : 0,
                        MenuUrl = pageIdMap.TryGetValue(menuSpec.PageTempId ?? string.Empty, out var pageId) ? pageId : null,
                        Order = order++,
                        Childrens = []
                    };
                    await _menuRepository.SaveAsync(menu);
                }
                break;
            }
        }

        return pageIdMap;
    }

    private DataSourceSchema CreateDataSourceSchema(string appId, AiGeneratedDataSourceDto dsSpec)
    {
        var ds = new DataSourceSchema
        {
            AppId = appId,
            Id = ShortIdGenerator.Generate(),
            Name = NormalizeTableName(dsSpec.Name),
            DisplayName = string.IsNullOrWhiteSpace(dsSpec.DisplayName) ? dsSpec.Name : Truncate(dsSpec.DisplayName, MaxNameLength),
            Description = Truncate(dsSpec.Description, MaxDescriptionLength),
            DataSourceType = ComponentDataSourceTypeEnum.DB,
            PublishStatus = true,
            EnableSoftDelete = true,
            TableFields = []
        };

        foreach (var field in dsSpec.Fields)
        {
            var fieldName = NormalizeFieldName(field.Name);
            ds.TableFields.Add(new TableFieldSchema
            {
                Id = fieldName,
                Name = fieldName,
                DisplayName = string.IsNullOrWhiteSpace(field.DisplayName) ? fieldName : Truncate(field.DisplayName, MaxLabelLength),
                Type = NormalizeFieldType(field.Type),
                IsPrimaryKey = field.IsPrimaryKey,
                IsNullable = field.IsNullable,
                IsUnique = false
            });
        }

        //保证主键存在
        if (!ds.TableFields.Any(f => f.IsPrimaryKey))
        {
            ds.TableFields.Insert(0, new TableFieldSchema
            {
                Id = "f_id",
                Name = "f_id",
                DisplayName = "编号",
                Type = "varchar(50)",
                IsPrimaryKey = true,
                IsNullable = false,
                IsUnique = true
            });
        }

        return ds;
    }

    #endregion

    #region 组件实例化（规格 → ComponentPartsSchema）

    /// <summary>
    /// 加载全部组件物料定义（partsId → 定义）
    /// </summary>
    private async Task<Dictionary<string, ComponentPartsSchema>> LoadComponentDefinesAsync()
    {
        var map = new Dictionary<string, ComponentPartsSchema>(StringComparer.OrdinalIgnoreCase);
        var libraries = await _componentLibraryRepository.GetListAsync() ?? [];
        foreach (var library in libraries)
        {
            var components = await _componentPartsRepository.GetAllComponentsAsync(library.LibraryId) ?? [];
            foreach (var component in components)
            {
                if (!string.IsNullOrWhiteSpace(component.PartsId) && !map.ContainsKey(component.PartsId.Trim()))
                {
                    map[component.PartsId.Trim()] = component;
                }
            }
        }
        return map;
    }

    /// <summary>
    /// 按组件规格递归实例化组件树（基于真实物料定义克隆，无法匹配的规格跳过）
    /// componentRefs 输出 组件实例 → 数据源临时ID 的映射
    /// </summary>
    private List<ComponentPartsSchema> BuildComponents(List<AiGeneratedComponentDto> specs, Dictionary<string, ComponentPartsSchema> defines, out Dictionary<ComponentPartsSchema, string> componentRefs)
    {
        var components = new List<ComponentPartsSchema>();
        componentRefs = [];

        foreach (var spec in specs ?? [])
        {
            var component = BuildComponent(spec, defines, componentRefs);
            if (component != null)
            {
                components.Add(component);
            }
        }
        return components;
    }

    private ComponentPartsSchema? BuildComponent(AiGeneratedComponentDto? spec, Dictionary<string, ComponentPartsSchema> defines, Dictionary<ComponentPartsSchema, string> componentRefs)
    {
        if (spec == null || string.IsNullOrWhiteSpace(spec.PartsId))
        {
            return null;
        }

        if (!defines.TryGetValue(spec.PartsId.Trim(), out var define))
        {
            return null;
        }

        var component = define.DeepClone();

        //记录数据源引用（落库时绑定为实际数据源）
        if (!string.IsNullOrWhiteSpace(spec.DataSourceRef))
        {
            componentRefs[component] = spec.DataSourceRef.Trim();
        }

        if (!string.IsNullOrWhiteSpace(spec.Label))
        {
            component.Label = Truncate(spec.Label, MaxLabelLength);
        }

        var attrNames = (define.AttributeDefineGroups ?? [])
            .SelectMany(g => g.AttributeDefines ?? [])
            .Select(a => a.AttributeName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        //输入提示（仅物料声明了 placeholder 属性时设置）
        if (!string.IsNullOrWhiteSpace(spec.Placeholder) && component.Fragment != null && attrNames.Contains("placeholder"))
        {
            SetFragmentAttribute(component.Fragment, "placeholder", spec.Placeholder.Trim());
        }

        //显示文本（仅物料声明了 content 属性时设置，如按钮文字）
        if (!string.IsNullOrWhiteSpace(spec.Text) && component.Fragment != null && attrNames.Contains("content"))
        {
            component.Fragment.Content = spec.Text.Trim();
        }

        foreach (var childSpec in spec.Children ?? [])
        {
            var child = BuildComponent(childSpec, defines, componentRefs);
            if (child == null)
            {
                continue;
            }

            child.ParentId = component.Id;
            component.Childrens.Add(child);
        }

        return component;
    }

    private static void SetFragmentAttribute(ComponentPartsFragmentSchema fragment, string name, object value)
    {
        var attrs = fragment.Attributes?.ToList() ?? [];
        var existing = attrs.FirstOrDefault(a => string.Equals(a.AttributeName, name, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.AttributeValue = value;
        }
        else
        {
            attrs.Add(new ComponentAttributeFragmentSchema
            {
                AttributeName = name,
                AttributeClrType = "System.String",
                AttributeValue = value
            });
        }
        fragment.Attributes = attrs.ToArray();
    }

    /// <summary>
    /// 将组件引用的数据源临时ID绑定为实际数据源（表格类组件同时生成列配置）
    /// </summary>
    private List<ComponentPartsSchema> BindComponentsDataSource(
        List<ComponentPartsSchema> components,
        Dictionary<ComponentPartsSchema, string> componentRefs,
        Dictionary<string, DataSourceSchema> dataSourceMap)
    {
        foreach (var (component, refId) in componentRefs)
        {
            if (!dataSourceMap.TryGetValue(refId.Trim(), out var ds))
            {
                continue;
            }

            component.DataSource ??= new();
            component.DataSource.DataSourceGroupType = ComponentDataSourceGroupTypeEnum.Table;
            component.DataSource.DataSourceType = ComponentDataSourceTypeEnum.DB;
            component.DataSource.DataSourceId = ds.Id;
            component.DataSource.DataSourceName = ds.Name;

            //生成表格列配置（与 TablePropertySchema 结构一致）
            var tableProperty = new TablePropertySchema
            {
                Columns = ds.TableFields
                    .Select((f, i) => new TableColumnSchema
                    {
                        Id = f.Name,
                        Name = f.Name,
                        Title = f.DisplayName ?? f.Name,
                        IsPrimaryKey = f.IsPrimaryKey,
                        Order = i
                    })
                    .ToList()
            };
            component.DataSource.DataSourceValue = tableProperty.ToJson();
        }

        return components;
    }

    #endregion

    #region 提示词

    /// <summary>
    /// AI 场景
    /// </summary>
    private enum AppScene
    {
        /// <summary>创建应用（含应用命名）</summary>
        App,

        /// <summary>已有应用内生成 页面/菜单/数据源</summary>
        Content
    }

    /// <summary>
    /// 构建应用级系统提示词（附可用物料清单）
    /// </summary>
    private async Task<string> BuildSystemPromptAsync(AppScene scene)
    {
        var defines = await LoadComponentDefinesAsync();
        var partsCatalog = string.Join("\n", defines.Values
            .Select(d => $"- partsId={d.PartsId} 名称={d.Label} 类型={(d.IsContainer ? "容器" : d.IsSupportDataSource ? "数据组件" : "基础")}")
            .ToList());

        var header = scene == AppScene.App
            ? """
              你是一名资深低代码应用设计师。请根据用户的口语化描述，设计一个完整的低代码应用方案，包含应用信息、页面、导航菜单与数据源（数据表）。
              """
            : """
              你是一名资深低代码应用设计师。用户已有一个低代码应用，请根据用户的口语化描述，为该应用增量生成页面、导航菜单与数据源（数据表）。只输出新增内容，不要输出已存在页面的修改或删除。
              """;

        return $$"""
            {{header}}
            输出要求：
            1. 只输出一个 JSON 对象，不要输出任何解释文字，也不要使用 markdown 代码块标记。
            2. JSON 结构如下：
            {
              "name": "应用名称（场景为已有应用生成时可留空字符串）",
              "description": "应用描述（同上，可留空）",
              "pages": [
                {
                  "tempId": "p1",
                  "name": "页面名称",
                  "pageType": "normal|form|table|report 之一（表单页用 form，列表页用 table，其余用 normal）",
                  "components": [ { "partsId": "input", "label": "字段标签", "placeholder": "请输入", "text": "", "dataSourceRef": "", "children": [] } ]
                }
              ],
              "menus": [
                { "tempId": "m1", "parentTempId": null, "title": "菜单名称", "icon": "home", "menuType": 0, "pageTempId": "p1" }
              ],
              "dataSources": [
                {
                  "tempId": "d1",
                  "name": "tb_order",
                  "displayName": "订单表",
                  "description": "存储订单信息",
                  "fields": [
                    { "name": "f_title", "displayName": "标题", "type": "varchar(200)", "isPrimaryKey": false, "isNullable": true }
                  ]
                }
              ]
            }
            3. 可用组件物料清单（components[].partsId 只能从中选择）：
            {{partsCatalog}}
            4. 规则：
            - pages：tempId 依次为 p1、p2……；每个页面 2~8 个顶层组件；列表/报表页用 table 组件并填写 dataSourceRef 引用对应数据源的 tempId；表单页用 card 或 flex 包裹输入类组件（input/textarea/select/radio/checkbox/switch/datepicker/timepicker），末尾加 button 提交按钮（text 填"提交"）；children 仅用于容器组件（card/flex/layout/tabs）嵌套，层级最多 2 层。
            - dataSources：tempId 依次为 d1、d2……；name 用 tb_ 前缀加小写下划线英文；fields 的 name 用 f_ 前缀加小写下划线英文；type 从 varchar(50)/varchar(200)/text/int/bigint/datetime/bool/decimal 中选；每张表恰好一个 isPrimaryKey=true 的字段；被 table 组件引用的数据源必须包含业务需要的全部字段。
            - menus：tempId 依次为 m1、m2……；menuType 0-菜单（必须填 pageTempId 指向 pages 中的 tempId）、1-目录（pageTempId 留空，可作为父级）；parentTempId 引用其它菜单的 tempId，根菜单为 null；层级最多 2 层；icon 从 home/appstore/menu/bars/database/setting/tool/api/user/team/cloud-upload/deployment-unit/bar-chart 中选。
            - 只有确实需要持久化业务数据时才生成 dataSources；简单展示型页面可不生成。
            - 所有名称、描述使用中文；不要生成 description 中未提及的多余页面。
            """;
    }

    /// <summary>
    /// 组件物料修改系统提示词（输入当前物料 JSON，输出修改后的完整 JSON）
    /// </summary>
    private static string ComponentPartsSystemPrompt => """
        你是一名资深低代码组件物料工程师。用户会提供一个组件物料的当前 JSON 定义与口语化修改需求，请按需求修改物料定义。
        输出要求：
        1. 只输出一个 JSON 对象（即修改后的完整组件物料定义），不要输出任何解释文字，也不要使用 markdown 代码块标记。
        2. 物料 JSON 结构说明：
        - partsId：组件物料唯一标识；libid：所属组件库Id；id：物料实例Id；ct：1-原子组件 2-组合组件 —— 这四个字段以及 v(版本) 必须原样保留，禁止修改。
        - lb：物料显示名称；desc：物料描述；order：排序；pub：发布状态(1-发布)。
        - frag：渲染片段。dt 为 "html:{标签}" 形式（如 "html:button"、"html:div"）或 .NET 组件类型全名（如 "H.LowCode.Components.LcTable, H.LowCode.Components"）——非必要不要修改 dt。
          frag.attrs：元素属性数组 [{ "attrn": "属性名", "attrt": "System.String|System.Boolean|System.Int32", "attrv": 属性值 }]；
          frag.childs：子片段数组（结构与 frag 相同，用于构建内部 HTML 结构）；frag.content：文本内容（如按钮文字）；frag.evs：元素事件配置。
        - attrdefgroups：设置面板属性定义分组 [{ "gn": "分组名", "attrdefs": [{ "attrn": "属性名", "disn": "显示名", "pt": 1, "dftval": 默认值, "attrt": "类型全名", "attrv": 当前值, "desc": "描述", "ops": {"选项值":"选项文本"} }] }]，
          其中 pt 为设置控件类型：1-Input 2-InputNumber 3-Radio 4-Checkbox 5-Select 6-Switch 7-Date 8-Textarea 9-Options 10-Table。
          attrdefs[].attrn 必须与 frag.attrs[].attrn 或 frag 内部子片段的属性对应，修改默认值时 attrv 要同步更新。
        - evdefs：事件定义 [{ "en": "OnClick", "disn": "点击事件", "desc": "描述" }]；stydefs：样式定义 [{ "sn": "样式名", "disn": "显示名", "cssprop": "css属性", "st": "类型", "dftval": "默认值" }]。
        - stl：设计器画布尺寸 { "itemw": 栅格宽0~24, "itemh": 高度px, "labelw": 标签宽 }。
        - childs：子组件实例数组（组合组件的可视化结构，每个子项结构与物料相同）。
        3. 规则：
        - 只修改与需求相关的部分，其余内容原样保留；保持 JSON 结构合法、字段名不变、类型一致（布尔/数字/字符串）。
        - 新增视觉样式优先通过 frag.attrs 的 class/style 属性实现（项目使用 hc-* 工具类，如 hc-btn-primary、hc-tag-blue）。
        - 新增可配置能力时：先在 frag.attrs 加属性（或复用已有属性），再在 attrdefgroups 对应分组加 attrdefs 定义，两者 attrn 一致。
        - 显示名称、描述使用中文；不要删除需求未提及的已有功能。
        """;

    /// <summary>
    /// 构建页面组件树系统提示词（附可用物料清单）
    /// </summary>
    private static string BuildComponentsSystemPrompt(Dictionary<string, ComponentPartsSchema> defines)
    {
        var partsCatalog = string.Join("\n", defines.Values
            .Select(d => $"- partsId={d.PartsId} 名称={d.Label} 类型={(d.IsContainer ? "容器" : d.IsSupportDataSource ? "数据组件" : "基础")}")
            .ToList());

        return $$"""
            你是一名资深低代码页面设计师。请根据用户的口语化描述，为一个低代码页面设计组件布局方案。
            输出要求：
            1. 只输出一个 JSON 对象，不要输出任何解释文字，也不要使用 markdown 代码块标记。
            2. JSON 结构如下：
            {
              "tempId": "",
              "name": "",
              "pageType": "normal",
              "components": [
                { "partsId": "card", "label": "基本信息", "placeholder": "", "text": "", "dataSourceRef": "", "children": [ { "partsId": "input", "label": "姓名", "placeholder": "请输入姓名" } ] }
              ]
            }
            3. 可用组件物料清单（components[].partsId 只能从中选择）：
            {{partsCatalog}}
            4. 规则：
            - name/pageType/tempId 固定填空字符串或 normal，无需设计。
            - components 是页面顶层的组件数组，按从上到下的排列顺序输出，共 3~10 个顶层组件。
            - 表单类内容用 card 或 flex 作为容器包裹输入类组件（input/textarea/select/radio/checkbox/switch/datepicker/timepicker/upload/autocomplete/treeselect），每个输入组件 label 填字段中文名，placeholder 填输入提示。
            - 数据列表用 table 组件；统计卡片用 statistic；说明文字用 alert（text 填提示内容）或 divider；操作按钮用 button（text 填按钮文字）。
            - children 仅用于容器组件（card/flex/layout/tabs/grid）内部嵌套子组件，层级最多 2 层，非容器组件不要再嵌套 children。
            - 未提供数据源信息时不要填写 dataSourceRef。
            - 所有文案使用中文。
            """;
    }

    #endregion

    #region 已有应用上下文

    /// <summary>
    /// 构建已有应用的页面/菜单/数据源摘要（辅助 AI 增量生成）
    /// </summary>
    private async Task<string> BuildExistingContextAsync(string appId)
    {
        var sections = new List<string>();

        var pages = await _pageRepository.GetListAsync(appId);
        if (pages != null && pages.Count > 0)
        {
            sections.Add("已有页面：" + string.Join("、", pages.Select(p => p.PageName)));
        }

        var menus = await _menuRepository.GetListAsync(appId);
        var menuTitles = FlattenMenuTitles(menus);
        if (menuTitles.Count > 0)
        {
            sections.Add("已有菜单：" + string.Join("、", menuTitles));
        }

        var dataSources = await _dataSourceRepository.GetListAsync(appId);
        if (dataSources != null && dataSources.Count > 0)
        {
            sections.Add("已有数据源：" + string.Join("、", dataSources.Select(d => d.DisplayName ?? d.Name)));
        }

        if (sections.Count == 0)
        {
            return string.Empty;
        }

        return "\n\n当前应用现状（生成时避免重复，可在此基础上扩展）：\n" + string.Join("\n", sections);
    }

    private static List<string> FlattenMenuTitles(IList<MenuSchema>? menus)
    {
        var result = new List<string>();
        if (menus == null) return result;
        foreach (var menu in menus)
        {
            if (!string.IsNullOrWhiteSpace(menu.Title)) result.Add(menu.Title);
            result.AddRange(FlattenMenuTitles(menu.Childrens));
        }
        return result;
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

    private static void NormalizeGenerated(AiGeneratedAppDto generated, bool includeAppInfo)
    {
        if (includeAppInfo)
        {
            generated.Name = Truncate(generated.Name, MaxNameLength);
            generated.Description = Truncate(generated.Description, MaxDescriptionLength);
        }

        if (generated.Pages.Count > MaxPages) generated.Pages = generated.Pages.Take(MaxPages).ToList();
        if (generated.DataSources.Count > MaxDataSources) generated.DataSources = generated.DataSources.Take(MaxDataSources).ToList();
        if (generated.Menus.Count > MaxMenus) generated.Menus = generated.Menus.Take(MaxMenus).ToList();

        //补齐缺失 TempId 并去重
        for (var i = 0; i < generated.Pages.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(generated.Pages[i].TempId))
            {
                generated.Pages[i].TempId = $"p{i + 1}";
            }
        }
        for (var i = 0; i < generated.DataSources.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(generated.DataSources[i].TempId))
            {
                generated.DataSources[i].TempId = $"d{i + 1}";
            }
        }
        for (var i = 0; i < generated.Menus.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(generated.Menus[i].TempId))
            {
                generated.Menus[i].TempId = $"m{i + 1}";
            }
        }

        //过滤悬空引用（菜单指向不存在的页面/父菜单）
        var pageTempIds = generated.Pages.Select(p => p.TempId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var menuTempIds = generated.Menus.Select(m => m.TempId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var menu in generated.Menus)
        {
            if (!string.IsNullOrEmpty(menu.PageTempId) && !pageTempIds.Contains(menu.PageTempId))
            {
                menu.PageTempId = string.Empty;
            }
            if (!string.IsNullOrEmpty(menu.ParentTempId) && !menuTempIds.Contains(menu.ParentTempId))
            {
                menu.ParentTempId = string.Empty;
            }
        }

        //组件规格中的 dataSourceRef 校验合法性（悬空引用置空）
        var dsTempIds = generated.DataSources.Select(d => d.TempId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var page in generated.Pages)
        {
            ValidateComponentRefs(page.Components, dsTempIds);
        }
    }

    private static void ValidateComponentRefs(List<AiGeneratedComponentDto> components, HashSet<string> dsTempIds)
    {
        foreach (var component in components ?? [])
        {
            if (!string.IsNullOrEmpty(component.DataSourceRef) && !dsTempIds.Contains(component.DataSourceRef))
            {
                component.DataSourceRef = string.Empty;
            }
            ValidateComponentRefs(component.Children, dsTempIds);
        }
    }

    private static PageTypeEnum ParsePageType(string? type) => (type ?? string.Empty).Trim().ToLowerInvariant() switch
    {
        "form" => PageTypeEnum.Form,
        "table" or "list" => PageTypeEnum.Table,
        "report" => PageTypeEnum.Report,
        _ => PageTypeEnum.Normal
    };

    private static string NormalizeTableName(string? name)
    {
        var normalized = ToIdentifier(name);
        if (string.IsNullOrEmpty(normalized))
        {
            normalized = $"tb_{ShortIdGenerator.Generate().ToLowerInvariant()}";
        }
        if (!normalized.StartsWith("tb_", StringComparison.Ordinal))
        {
            normalized = $"tb_{normalized}";
        }
        return normalized.Length > 60 ? normalized[..60] : normalized;
    }

    private static string NormalizeFieldName(string? name)
    {
        var normalized = ToIdentifier(name);
        if (string.IsNullOrEmpty(normalized))
        {
            normalized = $"f_{ShortIdGenerator.Generate().ToLowerInvariant()}";
        }
        if (!normalized.StartsWith("f_", StringComparison.Ordinal))
        {
            normalized = $"f_{normalized}";
        }
        return normalized.Length > 60 ? normalized[..60] : normalized;
    }

    /// <summary>
    /// 归一化为小写下划线标识符（中文转拼音不可行时退化为 pinyin 首字符剔除后的 ASCII 部分）
    /// </summary>
    private static string ToIdentifier(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        var builder = new System.Text.StringBuilder();
        foreach (var c in name.Trim().ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(c))
            {
                builder.Append(c);
            }
            else if (c is ' ' or '-' or '_' or '.')
            {
                builder.Append('_');
            }
        }

        //去除连续下划线与首尾下划线
        var result = System.Text.RegularExpressions.Regex.Replace(builder.ToString(), "_{2,}", "_").Trim('_');
        return result;
    }

    private static string NormalizeFieldType(string? type) => (type ?? string.Empty).Trim().ToLowerInvariant() switch
    {
        "int" or "integer" => "int",
        "bigint" or "long" => "bigint",
        "datetime" or "date" or "time" or "timestamp" => "datetime",
        "bool" or "boolean" or "bit" => "bool",
        "decimal" or "numeric" or "money" => "decimal",
        "text" or "longtext" => "text",
        _ => "varchar(200)"
    };

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        value = value.Trim();
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    #endregion
}
