using H.LowCode.DesignEngine.Abstraction;
using H.LowCode.MetaSchema;
using H.LowCode.ComponentParts.BasicComponents;

namespace H.LowCode.ComponentParts.ExtensionComponents;

public class ExtensionPageTemplateProvider : IPageTemplateProvider
{
    public string Title { get; set; } = "页面模板";

    public IEnumerable<ComponentSchema> LoadPageTemplate()
    {
        List<ComponentSchema> components =
        [
            new("formtemplate"){
                IsHiddenTitle = true,
                IsTemplate = true,
                ComponentProperty = new()
                {
                    Title = "基础表单",
                    ComponentValueType = ComponentValueTypeEnum.None
                },
                ComponentStyle = new()
                {
                    ItemWidth = 24,
                    DefaultStyle = "height:100%"
                }
            },
            new("tabletemplate"){
                IsHiddenTitle = true,
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcTable).GetFullNameWithAssemblyName() }
                ],
                ComponentProperty = new()
                {
                    Title = "基础列表",
                    ComponentValueType = ComponentValueTypeEnum.None
                },
                ComponentStyle = new()
                {
                    ItemWidth = 24,
                    DefaultStyle = "height:100%"
                }
            }
        ];
        return components;
    }
}
