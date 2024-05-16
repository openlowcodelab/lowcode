using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.Services
{
    /// <summary>
    /// 拖拽状态服务
    /// </summary>
    internal class DragDropStateService
    {
        /// <summary>
        /// 最后选中对象
        /// （当 DropItem 失去焦点时，即页面上没有任何项被选中，LastSelectedModel 仍有值）
        /// </summary>
        public ComponentSchema LastSelectedComponent { get; set; }

        /// <summary>
        /// 当前被拖拽对象
        /// </summary>
        public ComponentSchema CurrentDragComponent { get; set; }

        /// <summary>
        /// 最后一次拖拽到上面的对象
        /// </summary>
        public ComponentSchema LastDragOverComponent { get; set; }

        /// <summary>
        /// 最后一次拖拽到上面的对象的时间
        /// </summary>
        public DateTime LastDragOverTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后一次拖拽到上面的组件
        /// </summary>
        public ComponentSchema LastDropComponent { get; set; }

        #region 全局最终持久化数据
        /// <summary>
        /// 根 ComponentSchema
        /// </summary>
        public ComponentSchema RootComponent { get; set; }

        public PagePropertySchema GlobalPageProperty { get; set; } = new PagePropertySchema();
        #endregion

        #region method
        public ComponentSchema FindComponentById(Guid componentId)
        {
            if(componentId == RootComponent.Id)
                return RootComponent;

            return FindComponentByIdRecursive(componentId, RootComponent.Childrens);
        }

        private ComponentSchema FindComponentByIdRecursive(Guid componentId, IList<ComponentSchema> childrens)
        {
            foreach (var component in childrens)
            {
                if (component.Id == componentId) return component;

                var result = FindComponentByIdRecursive(componentId, component.Childrens);

                if (result != null) return result;
            }
            return null;
        }

        public void ResetComponent()
        {
            LastSelectedComponent = default;
            CurrentDragComponent = default;
            LastDragOverComponent = default;
        }

        public void ResetDragStyle()
        {
            CurrentDragComponent.Opacity = 1;
            LastDragOverComponent.DragEffectStyle = string.Empty;
            //LastDragOverComponent.RefreshState();
        }
        #endregion
    }
}
