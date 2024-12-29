//using H.LowCode.DesignEngine.Abstraction;
//using H.LowCode.MetaSchema;
//using H.LowCode.Components.AntBlazor;
//using H.LowCode.PartsMetaSchema;

//namespace H.LowCode.Components.Extension;

//public class ExtensionPageTemplateProvider : IPageTemplateProvider
//{
//    public string Label { get; set; } = "页面模板";

//    public IEnumerable<ComponentSchema> LoadPageTemplate()
//    {
//        List<ComponentSchema> components =
//        [
//            new("formtemplate"){
//                IsHiddenTitle = true,
//                IsTemplate = true,
//                Property = new()
//                {
//                    Label = "基础表单",
//                    ComponentValueType = ComponentValueTypeEnum.None
//                },
//                Style = new()
//                {
//                    ItemWidth = 24,
//                    DefaultStyle = "height:100%"
//                }
//            },
//            new("tabletemplate"){
//                IsHiddenTitle = true,
//                ComponentFragments =
//                [
//                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcTable).GetFullNameWithAssemblyName() }
//                ],
//                Property = new()
//                {
//                    Label = "基础列表",
//                    ComponentValueType = ComponentValueTypeEnum.None
//                },
//                Style = new()
//                {
//                    ItemWidth = 24,
//                    DefaultStyle = "height:100%"
//                }
//            }
//        ];
//        return components;
//    }

//    public IEnumerable<ComponentPartsSchema> LoadPageTemplates()
//    {
//        return [];
//    }
//}
