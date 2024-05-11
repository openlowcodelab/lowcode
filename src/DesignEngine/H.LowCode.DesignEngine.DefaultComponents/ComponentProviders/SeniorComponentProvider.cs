using AntDesign;
using H.LowCode.DesignEngine.DefaultComponents.Components;
using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.DefaultComponents.ComponentProviders
{
    public class SeniorComponentProvider : IComponentProvider
    {
        public string Title { get; set; } = "高级组件";

        public IEnumerable<ComponentSchema> LoadComponent()
        {
            List<ComponentSchema> components =
            [
                new(ComponentType:"tree"){
                    IsHiddenTitle = true,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Tree<string>));
                        builder.CloseComponent();
                    },
                    ComponentProperty = new()
                    {
                        Title = "树"
                    }
                },
                new(ComponentType:"treeselect"){
                    IsHiddenTitle = true,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(TreeSelect<string, string>));
                        builder.CloseComponent();
                    },
                    ComponentProperty = new()
                    {
                        Title = "树选择器"
                    }
                },
                new(ComponentType:"tabs"){
                    IsHiddenTitle = true,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(TabsWrap));
                        builder.AddComponentParameter(1, "Component", component);
                        builder.AddComponentParameter(2, "ItemCount", 3);
                        builder.CloseComponent();
                    },
                    ComponentProperty = new()
                    {
                        Title = "标签页",
                        Style = "background-color: #ffffff00;",
                        ItemWidth = 24,
                        ItemHeight = 300,
                        CustomStyle = "height: auto"
                    }
                },
                new(ComponentType:"image"){
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Image));
                        builder.AddComponentParameter(1, "Width", "300px");
                        builder.AddComponentParameter(2, "Src", "https://www.baidu.com/img/PCtm_d9c8750bed0b3c7d089fa7d55720d6cf.png");
                        builder.CloseComponent();
                    },
                    ComponentProperty = new()
                    {
                        Title = "图片",
                        ItemHeight = 200
                    }
                },
                new(ComponentType:"list"){
                    IsHiddenTitle = true,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(AntList<string>));
                        builder.CloseComponent();
                    },
                    ComponentProperty = new()
                    {
                        Title = "List 列表"
                    }
                },
                new(ComponentType:"card"){
                    IsHiddenTitle = true,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(CardWrap));
                        builder.AddComponentParameter(1, "Component", component);
                        builder.CloseComponent();
                    },
                    ComponentProperty = new()
                    {
                        Title = "卡片",
                        CustomStyle = "height: auto"
                    }
                },
                new(ComponentType:"carousel"){
                    IsHiddenTitle = true,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(CarouselWrap));
                        builder.AddComponentParameter(1, "Component", component);
                        builder.AddComponentParameter(2, "ItemCount", 4);
                        builder.CloseComponent();
                    },
                    ComponentProperty = new()
                    {
                        Title = "轮播图",
                        ItemWidth = 24,
                        CustomStyle = "height:auto"
                    }
                },
                new(ComponentType:"descriptions"){
                    IsHiddenTitle = true,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(DescriptionsWrap));
                        builder.AddComponentParameter(1, "Component", component);
                        builder.AddComponentParameter(2, "ItemCount", 3);
                        builder.CloseComponent();
                    },
                    ComponentProperty = new()
                    {
                        Title = "描述列表",
                        CustomStyle = "height: auto"
                    }
                },
                new(ComponentType:"empty"){
                    IsHiddenTitle = true,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Empty));
                        builder.CloseComponent();
                    },
                    ComponentProperty = new()
                    {
                        Title = "空",
                        ItemHeight = 150,
                        CustomStyle = "height: auto"
                    }
                }
            ];
            return components;
        }
    }
}
