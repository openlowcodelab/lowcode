using AntDesign;
using H.LowCode.DesignEngine.CustomComponents.Components;
using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.CustomComponents.ComponentProviders
{
    public class CustomComponentProvider : IComponentProvider
    {
        public string Title { get; set; } = "自定义组件";

        public IEnumerable<ComponentSchema> LoadComponent()
        {
            List<ComponentSchema> components =
            [
                new(ComponentType:"lakexeditor"){
                    SupportProperties = ["MaximumLength", "MinimumLength"],
                    RenderFragment = (componentSchema) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(LakexEditor));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new()
                    {
                        Title = "LakexEditor"
                    }
                },
                new(ComponentType:"userselect"){
                    RenderFragment = (componentSchema) =>  (builder) => {
                        builder.OpenComponent(0, typeof(UserSelect));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new()
                    {
                        Title = "用户选择"
                    }
                },
                new(ComponentType:"region"){
                    RenderFragment = (componentSchema) =>  (builder) => {
                        builder.OpenComponent(0, typeof(Region));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new()
                    {
                        Title = "行政区划"
                    }
                },
                new(ComponentType:"gaodemap"){
                    RenderFragment = (componentSchema) =>  (builder) => {
                        builder.OpenComponent(0, typeof(GaodeMap));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new()
                    {
                        Title = "高德地图"
                    }
                },
                new(ComponentType:"baidumap"){
                    RenderFragment = (componentSchema) =>  (builder) => {
                        builder.OpenComponent(0, typeof(BaiduMap));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new()
                    {
                        Title = "百度地图"
                    }
                }
            ];
            return components;
        }
    }
}
