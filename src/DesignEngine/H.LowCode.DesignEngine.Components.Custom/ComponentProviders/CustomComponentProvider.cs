using AntDesign;
using H.LowCode.Metadata;

namespace H.LowCode.DesignEngine.Components.Custom.ComponentProviders
{
    public class CustomComponentProvider : IComponentProvider
    {
        public string Title { get; set; } = "自定义组件";

        public IEnumerable<ComponentSchema> LoadComponent()
        {
            List<ComponentSchema> components = new List<ComponentSchema>()
            {
                new ComponentSchema(ComponentType:"html"){
                    SupportProperties = new List<string>(){ "MaximumLength", "MinimumLength" },
                    RenderFragment = (componentSchema) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(TextArea));
                        builder.AddAttribute(1, "style", "height:55px;");
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema()
                    {
                        Title = "HTML"
                    }
                },
                new ComponentSchema(ComponentType:"userselect"){
                    RenderFragment = (componentSchema) =>  (builder) => {
                        builder.OpenComponent(0, typeof(Input<string>));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema()
                    {
                        Title = "用户选择"
                    }
                },
                new ComponentSchema(ComponentType:"region"){
                    RenderFragment = (componentSchema) =>  (builder) => {
                        builder.OpenComponent(0, typeof(Input<string>));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema()
                    {
                        Title = "行政区划"
                    }
                },
                new ComponentSchema(ComponentType:"map"){
                    RenderFragment = (componentSchema) =>  (builder) => {
                        builder.OpenComponent(0, typeof(Input<string>));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema()
                    {
                        Title = "地图"
                    }
                }
            };
            return components;
        }
    }
}
