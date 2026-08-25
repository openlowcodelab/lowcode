namespace H.LowCode.DesignEngine.Application.Contracts;

/// <summary>
/// AI 生成输入（口语化描述）
/// </summary>
public class AiGenerateInputDto
{
    /// <summary>
    /// 用户的口语化需求描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// AI 生成的应用草稿（应用信息 + 页面 + 菜单 + 数据源，未落库）
/// </summary>
public class AiGeneratedAppDto
{
    /// <summary>应用名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>应用描述</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>页面集合</summary>
    public List<AiGeneratedPageDto> Pages { get; set; } = [];

    /// <summary>菜单集合（平铺，通过 TempId/ParentTempId 表达层级）</summary>
    public List<AiGeneratedMenuDto> Menus { get; set; } = [];

    /// <summary>数据源集合（平铺，通过 TempId 被页面组件引用）</summary>
    public List<AiGeneratedDataSourceDto> DataSources { get; set; } = [];
}

/// <summary>
/// AI 生成的页面（含组件规格描述，TempId 用于菜单/数据源引用）
/// </summary>
public class AiGeneratedPageDto
{
    /// <summary>临时ID（如 p1、p2，用于菜单引用）</summary>
    public string TempId { get; set; } = string.Empty;

    /// <summary>页面名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>页面类型：normal-普通 form-表单 table-列表 report-报表</summary>
    public string PageType { get; set; } = "normal";

    /// <summary>页面组件树规格</summary>
    public List<AiGeneratedComponentDto> Components { get; set; } = [];
}

/// <summary>
/// AI 生成的组件规格（服务端按组件物料定义实例化为真实组件）
/// </summary>
public class AiGeneratedComponentDto
{
    /// <summary>
    /// 组件物料 Id（必须取自可用物料清单中的 partsId，如 input、table、card）
    /// </summary>
    public string PartsId { get; set; } = string.Empty;

    /// <summary>组件显示名称/标签（如表单字段 Label、卡片标题等）</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>输入提示文本（仅输入类组件有效）</summary>
    public string Placeholder { get; set; } = string.Empty;

    /// <summary>显示文本（如按钮文字、提示内容等）</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// 绑定的数据源临时ID（仅表格/列表类组件有效，对应 dataSources[].tempId）
    /// </summary>
    public string DataSourceRef { get; set; } = string.Empty;

    /// <summary>子组件（仅容器类组件：card/flex/layout/tabs 等）</summary>
    public List<AiGeneratedComponentDto> Children { get; set; } = [];
}

/// <summary>
/// AI 生成的菜单
/// </summary>
public class AiGeneratedMenuDto
{
    /// <summary>临时ID（如 m1、m2，用于子菜单父级引用）</summary>
    public string TempId { get; set; } = string.Empty;

    /// <summary>父菜单临时ID（根菜单为 null）</summary>
    public string ParentTempId { get; set; } = string.Empty;

    /// <summary>菜单名称</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>图标（home/appstore/database/setting/tool/user/team/bar-chart 等）</summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>菜单类型：0-菜单 1-目录</summary>
    public int MenuType { get; set; }

    /// <summary>关联页面临时ID（目录类型可为空）</summary>
    public string PageTempId { get; set; } = string.Empty;
}

/// <summary>
/// AI 生成的数据源（数据表）
/// </summary>
public class AiGeneratedDataSourceDto
{
    /// <summary>临时ID（如 d1、d2，用于组件 dataSourceRef 引用）</summary>
    public string TempId { get; set; } = string.Empty;

    /// <summary>表名（tb_ 前缀，如 tb_order）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>显示名称</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>描述</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>字段集合</summary>
    public List<AiGeneratedFieldDto> Fields { get; set; } = [];
}

/// <summary>
/// AI 生成的数据源字段
/// </summary>
public class AiGeneratedFieldDto
{
    /// <summary>字段名（f_ 前缀，如 f_title）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>显示名称</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>字段类型：varchar(50)/text/int/bigint/datetime/bool/decimal</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>是否主键</summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>是否可空</summary>
    public bool IsNullable { get; set; } = true;
}
