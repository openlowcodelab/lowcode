using AntDesign;
using H.LowCode.DesignEngine.Components.Custom.PropertySchemas;
using H.LowCode.Metadata.Components;
using Newtonsoft.Json.Schema;

namespace H.LowCode.DesignEngine.Components.Custom.ComponentProviders
{
    public class CustomComponentProvider : IComponentProvider
    {
        public string Title { get; set; } = "自定义组件";

        public IEnumerable<ComponentSchema> LoadComponent()
        {
            List<ComponentSchema> components = new List<ComponentSchema>()
            {
                new ComponentSchema(){
                    ComponentJSchema = new JSchema(){ Title = "HTML" },
                    ComponentType = "html",
                    ComponentPropertySchema = new HtmlPropertyModel(),
                    ComponentRenderFragment = (builder) =>
                    {
                        builder.OpenComponent(0, typeof(TextArea));
                        builder.AddAttribute(1, "style", "height:55px;");
                        builder.CloseComponent();
                    }
                },
                new ComponentSchema(){
                    ComponentJSchema = new JSchema(){ Title = "用户选择" },
                    ComponentType = "userselect",
                    ComponentRenderFragment = (builder) => {
                        builder.OpenComponent(0, typeof(Input<string>));
                        builder.CloseComponent();
                    }
                },
                new ComponentSchema(){
                    ComponentJSchema = new JSchema(){ Title = "行政区划" },
                    ComponentType = "region",
                    ComponentRenderFragment = (builder) => {
                        builder.OpenComponent(0, typeof(Input<string>));
                        builder.CloseComponent();
                    }
                },
                new ComponentSchema(){
                    ComponentJSchema = new JSchema(){ Title = "地图" },
                    ComponentType = "map",
                    ComponentRenderFragment = (builder) => {
                        builder.OpenComponent(0, typeof(Input<string>));
                        builder.CloseComponent();
                    }
                }
            };
            return components;
        }
    }
}
