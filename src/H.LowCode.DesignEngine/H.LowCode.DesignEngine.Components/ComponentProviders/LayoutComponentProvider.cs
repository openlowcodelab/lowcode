using AntDesign;
using H.LowCode.DesignEngine.Components.PropertySchemas;
using H.LowCode.Metadata.Components;
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
                    ComponentPropertySchema = new GridPropertyModel(),
                    ComponentRenderFragment = (builder) =>
                    {
                        //builder.OpenComponent(0, typeof());
                        //builder.CloseComponent();
                    }
                },
                new ComponentSchema(){
                    ComponentJsonSchema = new JSchema(){ Title = "Layout 布局" },
                    ComponentType = "layout",
                    ComponentPropertySchema = new LayoutPropertyModel(),
                    ComponentRenderFragment = (builder) =>
                    {
                        //builder.OpenComponent(0, typeof());
                        //builder.CloseComponent();
                    }
                },
                new ComponentSchema(){
                    ComponentJsonSchema = new JSchema(){ Title = "表格布局" },
                    ComponentType = "table",
                    ComponentPropertySchema = new LayoutPropertyModel(),
                    ComponentRenderFragment = (builder) =>
                    {
                        //builder.OpenComponent(0, typeof());
                        //builder.CloseComponent();
                    }
                },
                new ComponentSchema(){
                    ComponentJsonSchema = new JSchema(){ Title = "分割线", Type = JSchemaType.String },
                    ComponentType = "divider",
                    ComponentPropertySchema = new DividerPropertyModel(),
                    ComponentRenderFragment = (builder) =>
                    {
                        builder.OpenComponent(1, typeof(Divider));
                        builder.CloseComponent();
                    }
                },
                new ComponentSchema(){
                    ComponentJsonSchema = new JSchema(){ Title = "间距", Type = JSchemaType.String },
                    ComponentType = "space",
                    ComponentPropertySchema = new SpacePropertyModel(),
                    ComponentRenderFragment = (builder) =>
                    {
                        builder.OpenComponent(1, typeof(Space));
                        builder.CloseComponent();
                    }
                }
            };
            return components;
        }
    }
}
