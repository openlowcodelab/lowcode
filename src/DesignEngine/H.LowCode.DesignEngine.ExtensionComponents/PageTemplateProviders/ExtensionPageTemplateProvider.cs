using H.LowCode.DesignEngine.Abstraction;
using H.LowCode.DesignEngine.ExtensionComponents.PageTemplates;
using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.ExtensionComponents.ComponentProviders
{
    public class ExtensionPageTemplateProvider : IPageTemplateProvider
    {
        public string Title { get; set; } = "页面模板";

        public IEnumerable<ComponentSchema> LoadPageTemplate()
        {
            List<ComponentSchema> components =
            [
                new("formtemplate"){
                    IsHiddenTitle = true,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(FormTemplate));
                        builder.AddComponentParameter(1, "Component", component);
                        builder.CloseComponent();
                    },
                    ComponentProperty = new()
                    {
                        Title = "表单页面",
                        ComponentValueType = ComponentValueType.None
                    }
                },
                new("tabletemplate"){
                    IsHiddenTitle = true,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(TableTemplate));
                        builder.AddComponentParameter(1, "Component", component);
                        builder.CloseComponent();
                    },
                    ComponentProperty = new()
                    {
                        Title = "列表页面",
                        ComponentValueType = ComponentValueType.None
                    }
                }
            ];
            return components;
        }
    }
}
