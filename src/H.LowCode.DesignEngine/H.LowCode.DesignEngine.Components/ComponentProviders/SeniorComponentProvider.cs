using AntDesign;
using H.LowCode.DesignEngine.Components.PropertySchemas;
using H.LowCode.Metadata.Components;
using Newtonsoft.Json.Schema;

namespace H.LowCode.DesignEngine.Components.ComponentProviders
{
    public class SeniorComponentProvider : IComponentProvider
    {
        public string Title { get; set; } = "布局组件";

        public IEnumerable<ComponentSchema> LoadComponent()
        {
            List<ComponentSchema> components = new List<ComponentSchema>()
            {
                new ComponentSchema(){
                    ComponentJSchema = new JSchema(){ Title = "折叠面板", Type = JSchemaType.String },
                    ComponentType = "collapse",
                    ComponentPropertySchema = new CollapsePropertyModel(),
                    ComponentRenderFragment = (builder) =>
                    {
                        builder.OpenComponent(1, typeof(Collapse));
                        builder.CloseComponent();
                    }
                },
                new ComponentSchema(){
                    ComponentJSchema = new JSchema(){ Title = "上传" },
                    ComponentType = "upload",
                    ComponentPropertySchema = new UploadPropertyModel(),
                    ComponentRenderFragment = (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Upload));
                        builder.CloseComponent();
                    }
                }
            };
            return components;
        }
    }
}
