using AntDesign;
using H.LowCode.MetaSchema;
using H.LowCode.DesignEngine.Abstraction;
using System.Text.Json;

namespace H.LowCode.ComponentParts.BasicComponents;

public class SeniorComponentProvider : IComponentProvider
{
    public string Title { get; set; } = "高级组件";

    public IEnumerable<ComponentSchema> LoadComponent()
    {
        List<ComponentSchema> components =
        [
            new("tree"){
                IsHiddenTitle = true,
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(Tree<string>).GetFullNameWithAssemblyName() }
                ],
                ComponentProperty = new()
                {
                    Title = "树"
                }
            },
            new("treeselect"){
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(TreeSelect<string, string>).GetFullNameWithAssemblyName() }
                ],
                ComponentProperty = new()
                {
                    Title = "树选择器"
                }
            },
            new("tabs"){
                IsHiddenTitle = true,
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcTabs).GetFullNameWithAssemblyName() },
                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueType.String, StringValue = "{Self}" },
                    new(){ Index = 2, FragmentEnum = FragmentEnum.Attribute, Name = "ItemCount", ValueType = ComponentValueType.Integer, IntValue = 3 },
                ],
                ComponentProperty = new()
                {
                    Title = "标签页"
                },
                ComponentStyle = new()
                {
                    DefaultStyle = "background-color: #ffffff00; height: auto",
                    ItemWidth = 24,
                    ItemHeight = 300
                }
            },
            new("image"){
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(Image).GetFullNameWithAssemblyName() },
                    new(){ Index = 1, FragmentEnum = FragmentEnum.Attribute, Name = "Width", ValueType = ComponentValueType.String, StringValue = "300px" },
                    new(){ Index = 2, FragmentEnum = FragmentEnum.Attribute, Name = "Src", ValueType = ComponentValueType.String, StringValue = "https://www.baidu.com/img/PCtm_d9c8750bed0b3c7d089fa7d55720d6cf.png" },
                ],
                ComponentProperty = new()
                {
                    Title = "图片"
                },
                ComponentStyle = new()
                {
                    ItemHeight = 200
                }
            },
            new("list"){
                IsHiddenTitle = true,
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(AntList<string>).GetFullNameWithAssemblyName() }
                ],
                ComponentProperty = new()
                {
                    Title = "List 列表"
                }
            },
            new("card"){
                IsHiddenTitle = true,
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcCard).GetFullNameWithAssemblyName() },
                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueType.String, StringValue = "{Self}" }
                ],
                ComponentProperty = new()
                {
                    Title = "卡片"
                },
                ComponentStyle = new()
                {
                    DefaultStyle = "height: auto"
                }
            },
            new("table"){
                IsHiddenTitle = true,
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcTable).GetFullNameWithAssemblyName() },
                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueType.String, StringValue = "{Self}" }
                ],
                ComponentProperty = new()
                {
                    Title = "表格",
                    ExtensionProperties =
                    {
                        { "tablecolumn", new() { Label="表格列", IsShowLabel = false, StringValue = string.Empty, SettingItemType = PropertyItemTypeEnum.Table } }
                    }
                },
                ComponentStyle = new()
                {
                    DefaultStyle = "height: auto",
                    ItemWidth = 24
                }
            },
            new("carousel"){
                IsHiddenTitle = true,
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcCarousel).GetFullNameWithAssemblyName() },
                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueType.String, StringValue = "{Self}" },
                    new(){ Index = 2, FragmentEnum = FragmentEnum.Attribute, Name = "ItemCount", ValueType = ComponentValueType.Integer, IntValue = 4 }
                ],
                ComponentProperty = new()
                {
                    Title = "轮播图"
                },
                ComponentStyle = new()
                {
                    DefaultStyle = "height:auto",
                    ItemWidth = 24
                }
            },
            new("descriptions"){
                IsHiddenTitle = true,
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcDescriptions).GetFullNameWithAssemblyName() },
                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Component", ValueType = ComponentValueType.String, StringValue = "{Self}" },
                    new(){ Index = 2, FragmentEnum = FragmentEnum.Attribute, Name = "ItemCount", ValueType = ComponentValueType.Integer, IntValue = 3 }
                ],
                ComponentProperty = new()
                {
                    Title = "描述列表"
                },
                ComponentStyle = new()
                {
                    DefaultStyle = "height: auto"
                }
            },
            new("empty"){
                IsHiddenTitle = true,
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(Empty).GetFullNameWithAssemblyName() }
                ],
                ComponentProperty = new()
                {
                    Title = "空"
                },
                ComponentStyle = new()
                {
                    DefaultStyle = "height: auto",
                    ItemHeight = 150
                }
            }
        ];
        return components;
    }
}
