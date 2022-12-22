using AntDesign;
using H.LowCode.DesignEngine.Components.Components;
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
                    IsHiddenTitle = true,
                    RenderFragment = (builder) =>
                    {
                        builder.OpenComponent(0, typeof(GridContainer));
                        builder.AddAttribute(1, "Rows", 3);
                        builder.AddAttribute(2, "Cols", 2);
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema()
                    {
                        Title = "Grid 栅格",
                        Style = "background-color: #ffffff00;",
                        ItemWidth = 24,
                        ItemHeight = 200
                    }
                },
                new ComponentSchema(ComponentType:"layout"){
                    IsHiddenTitle = true,
                    RenderFragment = (builder) =>
                    {
                        //builder.OpenComponent(0, typeof());
                        //builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema()
                    {
                        Title = "Layout 布局",
                        Style = "background-color: #ffffff00",
                        ItemWidth = 24,
                        ItemHeight = 150
                    }
                }
            };
            return components;
        }
    }
}
