//using AntDesign;
//using H.LowCode.MetaSchema;
//using H.LowCode.DesignEngine.Abstraction;
//using System.Text.Json;
//using H.LowCode.PartsMetaSchema;

//namespace H.LowCode.Components.AntBlazor;

//public class SeniorComponentProvider : IComponentProvider
//{
//    public string Label { get; set; } = "高级组件";

//    public string Name { get; } = "";

//    public IEnumerable<ComponentPartsSchema> LoadComponents()
//    {
//        return [];
//    }

//    public IEnumerable<ComponentSchema> LoadComponent()
//    {
//        List<ComponentSchema> components =
//        [
//            new("tabs"){
//                IsHiddenTitle = true,
//                ComponentFragments =
//                [
//                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcTabs).GetFullNameWithAssemblyName() },
//                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueTypeEnum.String, StringValue = "{Self}" },
//                    new(){ Index = 2, FragmentEnum = FragmentEnum.Attribute, Name = "ItemCount", ValueType = ComponentValueTypeEnum.Integer, IntValue = 3 },
//                ],
//                Property = new()
//                {
//                    Label = "标签页"
//                },
//                Style = new()
//                {
//                    DefaultStyle = "background-color: #ffffff00; height: auto",
//                    ItemWidth = 24,
//                    ItemHeight = 300
//                }
//            },
//            new("list"){
//                IsHiddenTitle = true,
//                ComponentFragments =
//                [
//                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(AntList<string>).GetFullNameWithAssemblyName() }
//                ],
//                Property = new()
//                {
//                    Label = "List 列表",
//                    IsSupportDataSource = true
//                }
//            },
//            new("card"){
//                IsHiddenTitle = true,
//                ComponentFragments =
//                [
//                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcCard).GetFullNameWithAssemblyName() },
//                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueTypeEnum.String, StringValue = "{Self}" }
//                ],
//                Property = new()
//                {
//                    Label = "卡片",
//                    IsSupportDataSource = true
//                },
//                Style = new()
//                {
//                    DefaultStyle = "height: auto"
//                }
//            },
//            new("table"){
//                IsHiddenTitle = true,
//                ComponentFragments =
//                [
//                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcTable).GetFullNameWithAssemblyName() },
//                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueTypeEnum.String, StringValue = "{Self}" }
//                ],
//                Property = new()
//                {
//                    Label = "表格",
//                    IsSupportDataSource = true,
//                    ComponentValueType = ComponentValueTypeEnum.Table,
//                },
//                Style = new()
//                {
//                    DefaultStyle = "height: auto",
//                    ItemWidth = 24
//                }
//            },
//        ];
//        return components;
//    }
//}
