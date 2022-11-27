using AntDesign;
using H.LowCode.Schema;

namespace H.LowCode.DesignEngine.Components.ComponentProviders
{
    public class LayoutComponentProvider : IComponentProvider
    {
        public string Title { get; set; } = "布局组件";

        public IEnumerable<ComponentSchema> LoadComponent()
        {
            List<ComponentSchema> components = new List<ComponentSchema>()
            {
                new ComponentSchema(ComponentType:"grid"){
                    ComponentRenderFragment = (builder) =>
                    {
                        //builder.OpenComponent(0, typeof());
                        //builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema()
                    {
                        Title = "Grid 栅格",
                        Style = "background-color: #ffffff00",
                        ItemWidth = 24
                    }
                },
                new ComponentSchema(ComponentType:"layout"){
                    ComponentRenderFragment = (builder) =>
                    {
                        //builder.OpenComponent(0, typeof());
                        //builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema()
                    {
                        Title = "Layout 布局",
                        Style = "background-color: #ffffff00",
                        ItemWidth = 24
                    }
                },
                new ComponentSchema(ComponentType:"table"){
                    ComponentRenderFragment = (builder) =>
                    {
                        //builder.OpenComponent(0, typeof());
                        //builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema()
                    {
                        Title = "表格布局",
                        Style = "background-color: #ffffff00",
                        ItemWidth = 24
                    }
                },
                new ComponentSchema(ComponentType:"divider"){
                    ComponentRenderFragment = (builder) =>
                    {
                        builder.OpenComponent(1, typeof(Divider));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema()
                    {
                        Title = "分割线",
                        ComponentValueType = ComponentValueType.String,
                        ItemWidth = 24
                    }
                }
            };
            return components;
        }
    }
}
