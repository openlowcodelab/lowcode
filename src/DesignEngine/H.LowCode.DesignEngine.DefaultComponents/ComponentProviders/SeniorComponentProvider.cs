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
                new("tree"){
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
                new("treeselect"){
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
                new("tabs"){
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
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Image));
                        builder.AddComponentParameter(1, "Width", "300px");
                        builder.AddComponentParameter(2, "Src", "https://www.baidu.com/img/PCtm_d9c8750bed0b3c7d089fa7d55720d6cf.png");
                        builder.CloseComponent();
                    },
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
                new("card"){
                    IsHiddenTitle = true,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(CardWrap));
                        builder.AddComponentParameter(1, "Component", component);
                        builder.CloseComponent();
                    },
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
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(TableWrap));
                        builder.AddComponentParameter(1, "Component", component);
                        builder.CloseComponent();
                    },
                    ComponentProperty = new()
                    {
                        Title = "表格"
                    },
                    ComponentStyle = new()
                    {
                        DefaultStyle = "height: auto",
                        ItemWidth = 24
                    }
                },
                new("carousel"){
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
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(DescriptionsWrap));
                        builder.AddComponentParameter(1, "Component", component);
                        builder.AddComponentParameter(2, "ItemCount", 3);
                        builder.CloseComponent();
                    },
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
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Empty));
                        builder.CloseComponent();
                    },
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
}
