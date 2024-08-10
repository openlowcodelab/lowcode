using H.LowCode.DesignEngine.Abstraction;
using H.LowCode.MetaSchema;
using H.LowCode.Parts.ExtensionComponents.Components;

namespace H.LowCode.Parts.ExtensionComponents
{
    public class ExtensionComponentProvider : IComponentProvider
    {
        public string Title { get; set; } = "扩展组件";

        public IEnumerable<ComponentSchema> LoadComponent()
        {
            List<ComponentSchema> components =
            [
                new("lakexeditor"){
                    IsHiddenTitle = true,
                    ComponentFragments =
                    [
                        new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LakexEditor).GetFullNameWithAssemblyName() },
                        new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueType.String, StringValue = "{Self}" }
                    ],
                    ComponentProperty = new()
                    {
                        Title = "LakexEditor",
                        ComponentValueType = ComponentValueType.String
                    },
                    ComponentStyle = new()
                    {
                        DefaultStyle = "height: 100%",
                        ItemWidth = 24
                    }
                },
                new("monacoeditor"){
                    IsHiddenTitle = true,
                    ComponentFragments =
                    [
                        new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(MonacoEditor).GetFullNameWithAssemblyName() },
                        new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueType.String, StringValue = "{Self}" }
                    ],
                    ComponentProperty = new()
                    {
                        Title = "MonacoEditor",
                        ComponentValueType = ComponentValueType.String
                    },
                    ComponentStyle = new()
                    {
                        ItemWidth = 24,
                        ItemHeight = 150,
                        DefaultStyle = "height: 100%"
                    }
                },
                //new("userselect"){
                //    ComponentFragments =
                //    [
                //        new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(UserSelect) }
                //    ],
                //    ComponentProperty = new()
                //    {
                //        Title = "用户选择"
                //    }
                //},
                //new("region"){
                //    ComponentFragments =
                //    [
                //        new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(Region) }
                //    ],
                //    ComponentProperty = new()
                //    {
                //        Title = "行政区划"
                //    }
                //},
                //new("gaodemap"){
                //    ComponentFragments =
                //    [
                //        new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(GaodeMap) }
                //    ],
                //    ComponentProperty = new()
                //    {
                //        Title = "高德地图"
                //    }
                //},
                //new("baidumap"){
                //    ComponentFragments =
                //    [
                //        new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(BaiduMap) }
                //    ],
                //    ComponentProperty = new()
                //    {
                //        Title = "百度地图"
                //    }
                //}
            ];
            return components;
        }
    }
}
