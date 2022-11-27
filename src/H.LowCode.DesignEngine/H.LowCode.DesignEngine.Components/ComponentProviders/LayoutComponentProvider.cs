using AntDesign;
using H.LowCode.DesignEngine.Components.PropertySchemas;
using H.LowCode.Schema;
using Newtonsoft.Json.Schema;

namespace H.LowCode.DesignEngine.Components.ComponentProviders
{
    public class LayoutComponentProvider : IComponentProvider
    {
        public string Title { get; set; } = "布局组件";

        public IEnumerable<ComponentSchema> LoadComponent()
        {
            List<ComponentSchema> components = new List<ComponentSchema>()
            {
                new ComponentSchema(){
                    ComponentJsonSchema = new JSchema(){ Title = "Grid 栅格" },
                    ComponentType = "grid",
                    Style = "background-color: #ffffff00",
                    ComponentPropertySchema = new GridPropertyModel(){ ItemWidth = 24 },
                    ComponentRenderFragment = (builder) =>
                    {
                        //builder.OpenComponent(0, typeof());
                        //builder.CloseComponent();
                    }
                },
                new ComponentSchema(){
                    ComponentJsonSchema = new JSchema(){ Title = "Layout 布局" },
                    ComponentType = "layout",
                    Style = "background-color: #ffffff00",
                    ComponentPropertySchema = new LayoutPropertyModel(){ ItemWidth = 24 },
                    ComponentRenderFragment = (builder) =>
                    {
                        //builder.OpenComponent(0, typeof());
                        //builder.CloseComponent();
                    }
                },
                new ComponentSchema(){
                    ComponentJsonSchema = new JSchema(){ Title = "表格布局" },
                    ComponentType = "table",
                    Style = "background-color: #ffffff00",
                    ComponentPropertySchema = new LayoutPropertyModel(){ ItemWidth = 24 },
                    ComponentRenderFragment = (builder) =>
                    {
                        //builder.OpenComponent(0, typeof());
                        //builder.CloseComponent();
                    }
                },
                new ComponentSchema(){
                    ComponentJsonSchema = new JSchema(){ Title = "分割线", Type = JSchemaType.String },
                    ComponentType = "divider",
                    ComponentPropertySchema = new DividerPropertyModel(){ ItemWidth = 24 },
                    ComponentRenderFragment = (builder) =>
                    {
                        builder.OpenComponent(1, typeof(Divider));
                        builder.CloseComponent();
                    }
                }
            };
            return components;
        }
    }
}
