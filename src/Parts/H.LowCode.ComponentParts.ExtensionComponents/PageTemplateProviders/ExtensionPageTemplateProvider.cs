using H.LowCode.DesignEngine.Abstraction;
using H.LowCode.MetaSchema;
using H.LowCode.ComponentParts.BasicComponents;

namespace H.LowCode.ComponentParts.ExtensionComponents;

public class ExtensionPageTemplateProvider : IPageTemplateProvider
{
    public string Title { get; set; } = "页面模板";

    public IEnumerable<ComponentSchema> LoadPageTemplate()
    {
        List<ComponentSchema> components =
        [
            new("formtemplate"){
                IsHiddenTitle = true,
                IsTemplate = true,
                ComponentProperty = new()
                {
                    Title = "基础表单",
                    ComponentValueType = ComponentValueType.None
                },
                ComponentStyle = new()
                {
                    ItemWidth = 24,
                    DefaultStyle = "height:100%"
                },
                Childrens = [
                    new("button")
                    {
                        IsHiddenTitle = true,
                        ComponentFragments =
                        [
                            new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(ButtonWrap).GetFullNameWithAssemblyName() },
                            new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Text", ValueType = ComponentValueType.String, StringValue = "提交" }
                        ],
                        ComponentProperty = new()
                        {
                            Title = "提交",
                            ComponentValueType = ComponentValueType.None
                        },
                        ComponentStyle = new()
                        {
                            ItemHeight = 40,
                            DefaultStyle = "margin:0 8px; position:fixed; bottom:8px; right: 320px;"
                        }
                    },
                    new("button")
                    {
                        IsHiddenTitle = true,
                        ComponentFragments =
                        [
                            new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(ButtonWrap).GetFullNameWithAssemblyName() },
                            new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Text", ValueType = ComponentValueType.String, StringValue = "取消" }
                        ],
                        ComponentProperty = new()
                        {
                            Title = "取消",
                            ComponentValueType = ComponentValueType.None
                        },
                        ComponentStyle = new()
                        {
                            ItemWidth = 24,
                            ItemHeight = 40,
                            DefaultStyle = "margin:0 8px; position:fixed; bottom:8px; right: 250px;"
                        }
                    }
                ]
            },
            new("groupformtemplate"){
                IsHiddenTitle = true,
                IsContainerComponent = true,
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(GroupFormTemplate).GetFullNameWithAssemblyName() },
                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueType.String, StringValue = "{Self}" }
                ],
                ComponentProperty = new()
                {
                    Title = "分组表单",
                    ComponentValueType = ComponentValueType.None
                },
                ComponentStyle = new()
                {
                    ItemWidth = 24,
                    DefaultStyle = "height:100%"
                }
            },
            new("tabletemplate"){
                IsHiddenTitle = true,
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(TableTemplate).GetFullNameWithAssemblyName() }
                ],
                ComponentProperty = new()
                {
                    Title = "列表页面(Table)",
                    ComponentValueType = ComponentValueType.None
                },
                ComponentStyle = new()
                {
                    ItemWidth = 24,
                    DefaultStyle = "height:100%"
                }
            },
            new("listtemplate"){
                IsHiddenTitle = true,
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(ListTemplate).GetFullNameWithAssemblyName() }
                ],
                ComponentProperty = new()
                {
                    Title = "列表页面(List)",
                    ComponentValueType = ComponentValueType.None
                },
                ComponentStyle = new()
                {
                    ItemWidth = 24,
                    DefaultStyle = "height:100%"
                }
            },
            new("cardtemplate"){
                IsHiddenTitle = true,
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(CardTemplate).GetFullNameWithAssemblyName() }
                ],
                ComponentProperty = new()
                {
                    Title = "列表页面(Card)",
                    ComponentValueType = ComponentValueType.None
                },
                ComponentStyle = new()
                {
                    ItemWidth = 24,
                    DefaultStyle = "height:100%"
                }
            }
        ];
        return components;
    }
}
