//using H.LowCode.MetaSchema;
//using H.LowCode.DesignEngine.Abstraction;
//using System.Text.Json;
//using H.LowCode.PartsMetaSchema;

//namespace H.LowCode.Components.AntBlazor;

//public class LayoutComponentProvider : IComponentProvider
//{
//    public string Label { get; } = "布局组件";

//    public string Name { get; } = "";

//    public IEnumerable<ComponentPartsSchema> LoadComponents()
//    {
//        return [];
//    }

//    public IEnumerable<ComponentSchema> LoadComponent()
//    {
//        List<ComponentSchema> components =
//        [
//            new("grid"){
//                IsContainer = true,
//                ComponentFragments =
//                [
//                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcGrid).GetFullNameWithAssemblyName() },
//                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueTypeEnum.String, StringValue = "{Self}" },
//                    new(){ Index = 2, FragmentEnum = FragmentEnum.Attribute, Name = "Rows", ValueType = ComponentValueTypeEnum.Integer, IntValue = 2 },
//                    new(){ Index = 3, FragmentEnum = FragmentEnum.Attribute, Name = "Cols", ValueType = ComponentValueTypeEnum.Integer, IntValue = 2 }
//                ],
//                Property = new()
//                {
//                    Label = "Grid 栅格"
//                },
//                Style = new()
//                {
//                    DefaultStyle = "width:100%; height: auto; background-color: #ffffff00;",
//                    ItemWidth = 24
//                }
//            },
//            new("layout"){
//                IsContainer = true,
//                ComponentFragments =
//                [
//                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcLayout).GetFullNameWithAssemblyName() },
//                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueTypeEnum.String, StringValue = "{Self}" }
//                ],
//                Property = new()
//                {
//                    Label = "Layout 布局"
//                },
//                Style = new()
//                {
//                    DefaultStyle = "width:100%; height:auto; background-color: #ffffff00;",
//                    ItemWidth = 24,
//                    ItemHeight = 150
//                }
//            },
//            new("flex"){
//                IsContainer = true,
//                ComponentFragments =
//                [
//                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcFlex).GetFullNameWithAssemblyName() },
//                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueTypeEnum.String, StringValue = "{Self}" }
//                ],
//                Property = new()
//                {
//                    Label = "Flex 布局"
//                },
//                Style = new()
//                {
//                    DefaultStyle = "width:100%; height:auto; background-color: #ffffff00;",
//                    ItemWidth = 24,
//                    ItemHeight = 150
//                }
//            }
//        ];
//        return components;
//    }
//}
