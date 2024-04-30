using H.LowCode.DesignEngine.DefaultComponents.Components;
using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.DefaultComponents.ComponentProviders
{
    public class LayoutComponentProvider : IComponentProvider
    {
        public string Title { get; set; } = "布局组件";

        public IEnumerable<ComponentSchema> LoadComponent()
        {
            List<ComponentSchema> components =
            [
                new(ComponentType:"grid"){
                    IsHiddenTitle = true,
                    ComponentCategory = ComponentCategory.Container,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(GridWrap));
                        builder.AddComponentParameter(1, "Component", component);
                        builder.AddComponentParameter(2, "Rows", 2);
                        builder.AddComponentParameter(3, "Cols", 2);
                        builder.CloseComponent();
                    },
                    ComponentProperty = new()
                    {
                        Title = "Grid 栅格",
                        Style = "background-color: #ffffff00; height: auto;",
                        ItemWidth = 24
                    }
                },
                new(ComponentType:"layout"){
                    IsHiddenTitle = true,
                    ComponentCategory = ComponentCategory.Container,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(LayoutWrap));
                        builder.AddComponentParameter(1, "Component", component);
                        builder.CloseComponent();
                    },
                    ComponentProperty = new()
                    {
                        Title = "Layout 布局",
                        Style = "background-color: #ffffff00",
                        ItemWidth = 24,
                        ItemHeight = 150,
                        CustomStyle = "height: 100%"
                    }
                },
                new(ComponentType:"flex"){
                    IsHiddenTitle = true,
                    ComponentCategory = ComponentCategory.Container,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(FlexWrap));
                        builder.AddComponentParameter(1, "Component", component);
                        builder.CloseComponent();
                    },
                    ComponentProperty = new()
                    {
                        Title = "Flex 布局",
                        Style = "background-color: #ffffff00; height: auto;",
                        ItemWidth = 12,
                        ItemHeight = 150,
                        CustomStyle = "height: auto"
                    }
                }
            ];
            return components;
        }
    }
}
