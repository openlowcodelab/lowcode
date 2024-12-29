//using AntDesign;
//using H.LowCode.DesignEngine.Abstraction;
//using H.LowCode.MetaSchema;
//using H.LowCode.PartsMetaSchema;

//namespace H.LowCode.Components.Extension;

//public class ExtensionComponentProvider : IComponentProvider
//{
//    public string Label { get; set; } = "扩展组件";

//    public string Name { get; } = "";

//    public IEnumerable<ComponentPartsSchema> LoadComponents()
//    {
//        return [];
//    }

//    public IEnumerable<ComponentSchema> LoadComponent()
//    {
//        List<ComponentSchema> components =
//        [
//            new("lakexeditor"){
//                IsHiddenTitle = true,
//                ComponentFragments =
//                [
//                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcLakexEditor).GetFullNameWithAssemblyName() },
//                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueTypeEnum.String, StringValue = "{Self}" }
//                ],
//                Property = new()
//                {
//                    Label = "LakexEditor",
//                    ComponentValueType = ComponentValueTypeEnum.String
//                },
//                Style = new()
//                {
//                    DefaultStyle = "height: 100%",
//                    ItemWidth = 24
//                }
//            },
//            new("monacoeditor"){
//                IsHiddenTitle = true,
//                ComponentFragments =
//                [
//                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcMonacoEditor).GetFullNameWithAssemblyName() },
//                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueTypeEnum.String, StringValue = "{Self}" }
//                ],
//                Property = new()
//                {
//                    Label = "MonacoEditor",
//                    ComponentValueType = ComponentValueTypeEnum.String
//                },
//                Style = new()
//                {
//                    ItemWidth = 24,
//                    ItemHeight = 150,
//                    DefaultStyle = "height: 100%"
//                }
//            },
//            //new("userselect"){
//            //    ComponentFragments =
//            //    [
//            //        new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcUserSelect) }
//            //    ],
//            //    Property = new()
//            //    {
//            //        Title = "用户选择"
//            //    }
//            //},
//            //new("region"){
//            //    ComponentFragments =
//            //    [
//            //        new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcRegion) }
//            //    ],
//            //    Property = new()
//            //    {
//            //        Title = "行政区划"
//            //    }
//            //},
//            //new("gaodemap"){
//            //    ComponentFragments =
//            //    [
//            //        new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcGaodeMap) }
//            //    ],
//            //    Property = new()
//            //    {
//            //        Title = "高德地图"
//            //    }
//            //},
//            //new("baidumap"){
//            //    ComponentFragments =
//            //    [
//            //        new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcBaiduMap) }
//            //    ],
//            //    Property = new()
//            //    {
//            //        Title = "百度地图"
//            //    }
//            //}
//        ];
//        return components;
//    }
//}
