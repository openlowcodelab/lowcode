using H.LowCode.DesignEngine.Abstraction;
using H.LowCode.DesignEngine.DefaultComponents.Components;
using H.LowCode.DesignEngine.ExtensionComponents.PageTemplates;
using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.ExtensionComponents.ComponentProviders
{
    public class ExtensionPageTemplateProvider : IPageTemplateProvider
    {
        public string Title { get; set; } = "页面模板";

        public IEnumerable<ComponentSchema> LoadPageTemplate()
        {
            List<ComponentSchema> components =
            [
                new("formtemplate"){
                    IsHiddenTitle = true,
                    IsContainerComponent = true,
                    ComponentFragments =
                    [
                        new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(FormTemplate).GetFullNameWithAssemblyName() },
                        new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueType.String, StringValue = "{Self}" }
                    ],
                    ComponentProperty = new()
                    {
                        Title = "基础表单",
                        ComponentValueType = ComponentValueType.None
                    },
                    ComponentStyle = new()
                    {
                        ItemWidth = 24,
                        DefaultStyle = "height:100%"
                    }
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
}
