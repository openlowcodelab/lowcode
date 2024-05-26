using H.LowCode.DesignEngine.Abstraction;
using H.LowCode.DesignEngine.ExtensionComponents.Components;
using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.ExtensionComponents.ComponentProviders
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
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(LakexEditor));
                        builder.AddComponentParameter(1, "Component", component);
                        builder.CloseComponent();
                    },
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
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(MonacoEditor));
                        builder.AddComponentParameter(1, "Component", component);
                        builder.CloseComponent();
                    },
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
                }
                //new(component"userselect"){
                //    RenderFragment = (component) =>  (builder) => {
                //        builder.OpenComponent(0, typeof(UserSelect));
                //        builder.CloseComponent();
                //    },
                //    ComponentPropertySchema = new()
                //    {
                //        Title = "用户选择"
                //    }
                //},
                //new(component"region"){
                //    RenderFragment = (component) =>  (builder) => {
                //        builder.OpenComponent(0, typeof(Region));
                //        builder.CloseComponent();
                //    },
                //    ComponentPropertySchema = new()
                //    {
                //        Title = "行政区划"
                //    }
                //},
                //new(component"gaodemap"){
                //    RenderFragment = (component) =>  (builder) => {
                //        builder.OpenComponent(0, typeof(GaodeMap));
                //        builder.CloseComponent();
                //    },
                //    ComponentPropertySchema = new()
                //    {
                //        Title = "高德地图"
                //    }
                //},
                //new(component"baidumap"){
                //    RenderFragment = (component) =>  (builder) => {
                //        builder.OpenComponent(0, typeof(BaiduMap));
                //        builder.CloseComponent();
                //    },
                //    ComponentPropertySchema = new()
                //    {
                //        Title = "百度地图"
                //    }
                //}
            ];
            return components;
        }
    }
}
