using H.LowCode.MetaSchema;
using H.LowCode.DesignEngine.Abstraction;
using System.Text.Json;

namespace H.LowCode.ComponentParts.BasicComponents;

public class LayoutComponentProvider : IComponentProvider
{
    public string Title { get; set; } = "布局组件";

    public IEnumerable<ComponentSchema> LoadComponent()
    {
        List<ComponentSchema> components =
        [
            new("grid"){
                IsContainerComponent = true,
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcGrid).GetFullNameWithAssemblyName() },
                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueType.String, StringValue = "{Self}" },
                    new(){ Index = 2, FragmentEnum = FragmentEnum.Attribute, Name = "Rows", ValueType = ComponentValueType.Integer, IntValue = 2 },
                    new(){ Index = 3, FragmentEnum = FragmentEnum.Attribute, Name = "Cols", ValueType = ComponentValueType.Integer, IntValue = 2 }
                ],
                ComponentProperty = new()
                {
                    Title = "Grid 栅格"
                },
                ComponentStyle = new()
                {
                    DefaultStyle = "width:100%; height: auto; background-color: #ffffff00;",
                    ItemWidth = 24
                }
            },
            new("layout"){
                IsContainerComponent = true,
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcLayout).GetFullNameWithAssemblyName() },
                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueType.String, StringValue = "{Self}" }
                ],
                ComponentProperty = new()
                {
                    Title = "Layout 布局"
                },
                ComponentStyle = new()
                {
                    DefaultStyle = "width:100%; height:auto; background-color: #ffffff00;",
                    ItemWidth = 24,
                    ItemHeight = 150
                }
            },
            new("flex"){
                IsContainerComponent = true,
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcFlex).GetFullNameWithAssemblyName() },
                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueType.String, StringValue = "{Self}" }
                ],
                ComponentProperty = new()
                {
                    Title = "Flex 布局"
                },
                ComponentStyle = new()
                {
                    DefaultStyle = "width:100%; height:auto; background-color: #ffffff00;",
                    ItemWidth = 24,
                    ItemHeight = 150
                }
            }
        ];
        return components;
    }
}
