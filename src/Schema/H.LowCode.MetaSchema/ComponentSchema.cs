using H.Extensions.System;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema
{
    public class ComponentSchema : BaseMetaSchema
    {
        public ComponentSchema(string componentName)
        {
            ComponentName = componentName;
        }

        #region 公共属性
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 组件名称
        /// </summary>
        [JsonPropertyName("n")]
        public string ComponentName { get; }

        [JsonPropertyName("iscontainer")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsContainerComponent { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsTemplate { get; set; }

        [JsonPropertyName("pid")]
        public Guid ParentId { get; set; }
        #endregion

        #region 普通组件属性 (容器组件不需要)
        /// <summary>
        /// 是否隐藏标题
        /// </summary>
        [JsonPropertyName("hide")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsHiddenTitle { get; set; }

        /// <summary>
        /// 组件渲染片段
        /// </summary>
        [JsonPropertyName("fragment")]
        public IList<ComponentFragmentSchema> ComponentFragments { get; set; }

        /// <summary>
        /// 组件属性
        /// </summary>
        [JsonPropertyName("compprop")]
        public ComponentPropertySchema ComponentProperty { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("event")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public ComponentEventSchema ComponentEvent { get; set; } = new();
        #endregion

        /// <summary>
        /// 组件样式
        /// </summary>
        [JsonPropertyName("style")]
        public ComponentStyleSchema ComponentStyle { get; set; } = new();

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("childs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public IList<ComponentSchema> Childrens { get; set; } = [];

        [JsonPropertyName("ds")]
        public DataSourceSchema DataSource { get; set; }

        #region JsonIgnore Attributes
        [JsonIgnore]
        public bool IsSelected { get; set; }

        [JsonIgnore]
        public double Opacity { get; set; } = 1;

        [JsonIgnore]
        public string DragEffectStyle { get; set; }

        /// <summary>
        /// 是否由组件面板拖拽而来
        /// </summary>
        [JsonIgnore]
        public bool IsDroppedFromComponentPanel { get; set; }

        /// <summary>
        /// 拖拽到后面（true：后面  false：前面）
        /// </summary>
        [JsonIgnore]
        public bool IsDropedAfter { get; set; }

        [JsonIgnore]
        public Action Refresh { get; set; }
        #endregion

        public ComponentSchema CopyNew()
        {
            ComponentSchema newComponent = ObjectExtension.DeepClone(this);

            //Copy全新对象, Id 重新生成
            newComponent.Id = Guid.NewGuid();
            newComponent.ParentId = Guid.Empty;
            newComponent.IsSelected = false;

            //手动赋值无法序列化属性
            newComponent.Refresh = Refresh;

            //1.子节点 ParentId 重新赋值; 2.重新赋值序列化过程中丢失的 RenderFragment、Refresh 值
            CopyNewRecursive(newComponent, this);

            return newComponent;
        }

        public void RefreshState()
        {
            Refresh?.Invoke();

            //递归刷新子节点
            //foreach(var child in Childrens)
            //{
            //    child.RefreshState();
            //}
        }

        #region private
        private static void CopyNewRecursive(ComponentSchema newComponent, ComponentSchema oldComponent)
        {
            for (var i = 0; i < newComponent.Childrens.Count; i++)
            {
                var child = newComponent.Childrens[i];
                child.Id = Guid.NewGuid();
                child.ParentId = newComponent.Id;

                child.Refresh = oldComponent.Childrens[i].Refresh;

                CopyNewRecursive(child, oldComponent.Childrens[i]);
            }
        }
        #endregion
    }
}