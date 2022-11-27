using AntDesign;
using H.LowCode.Schema;

namespace H.LowCode.DesignEngine.Components.ComponentProviders
{
    public class SeniorComponentProvider : IComponentProvider
    {
        public string Title { get; set; } = "高级组件";

        public IEnumerable<ComponentSchema> LoadComponent()
        {
            List<ComponentSchema> components = new List<ComponentSchema>()
            {
                new ComponentSchema(ComponentType:"collapse"){
                    ComponentRenderFragment = (builder) =>
                    {
                        builder.OpenComponent(1, typeof(Collapse));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema()
                    {
                        Title = "折叠面板",
                        ComponentValueType = ComponentValueType.String
                    }
                },
                new ComponentSchema(ComponentType:"upload"){
                    ComponentRenderFragment = (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Upload));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema()
                    {
                        Title = "上传"
                    }
                }
            };
            return components;
        }
    }
}
