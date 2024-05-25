using H.LowCode.DesignEngine.DefaultComponents.Components;
using H.LowCode.MetaSchema;
using H.LowCode.DesignEngine.Abstraction;

namespace H.LowCode.DesignEngine.DefaultComponents.ComponentProviders
{
    public class LayoutComponentProvider : IComponentProvider
    {
        public string Title { get; set; } = "布局组件";

        public IEnumerable<ComponentSchema> LoadComponent()
        {
            List<ComponentSchema> components =
            [
                new("grid"){
                    IsHiddenTitle = true,
                    IsContainerComponent = true,
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
                        Title = "Grid 栅格"
                    },
                    ComponentStyle = new()
                    {
                        DefaultStyle = "background-color: #ffffff00; height: auto;",
                        ItemWidth = 24
                    }
                },
                new("layout"){
                    IsHiddenTitle = true,
                    IsContainerComponent = true,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(LayoutWrap));
                        builder.AddComponentParameter(1, "Component", component);
                        builder.CloseComponent();
                    },
                    ComponentProperty = new()
                    {
                        Title = "Layout 布局"
                    },
                    ComponentStyle = new()
                    {
                        DefaultStyle = "background-color: #ffffff00; height: 100%",
                        ItemWidth = 24,
                        ItemHeight = 150
                    }
                },
                new("flex"){
                    IsHiddenTitle = true,
                    IsContainerComponent = true,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(FlexWrap));
                        builder.AddComponentParameter(1, "Component", component);
                        builder.CloseComponent();
                    },
                    ComponentProperty = new()
                    {
                        Title = "Flex 布局"
                    },
                    ComponentStyle = new()
                    {
                        DefaultStyle = "background-color: #ffffff00; height: auto;",
                        ItemWidth = 24,
                        ItemHeight = 150
                    }
                }
            ];
            return components;
        }
    }
}
