using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Text;
using System.Threading;

namespace H.LowCode.DesignerEngine.DesignPanel
{
    public partial class DragDropZone
    {
        #region Parameter
        [Parameter]
        public Action<DragDropItem> DragEnd { get; set; }

        [Parameter]
        public EventCallback<DragDropItem> OnItemDrop { get; set; }

        [Parameter]
        public Action OnDragDropZoneClick { get; set; }

        [Parameter]
        public IList<DragDropItem> Items { get; set; }

        [Parameter]
        public RenderFragment<DragDropItem> ChildContent { get; set; }

        [Parameter]
        public string Style { get; set; }
        #endregion

        #region Event
        protected override bool ShouldRender()
        {
            return DragDropService.ShouldRender;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        public void OnDragEnd()
        {
            if (DragEnd != null)
            {
                DragEnd(DragDropService.CurrentDragItem);
            }

            DragDropService.Reset();
        }

        public void OnDragEnter(DragDropItem item)
        {
            var currentDragItem = DragDropService.CurrentDragItem;
            if (item.Equals(currentDragItem))
                return;
            
            DragDropService.DragTargetItem = item;
            //SwapItem(DragDropService.DragTargetItem, currentDragItem);

            DragDropService.ShouldRender = true;
            StateHasChanged();
            DragDropService.ShouldRender = false;
        }

        public void OnDragLeave()
        {
            DragDropService.DragTargetItem = default;
            DragDropService.ShouldRender = true;
            StateHasChanged();
            DragDropService.ShouldRender = false;
        }

        public void OnDragStart(DragDropItem item)
        {
            DragDropService.ShouldRender = true;
            DragDropService.CurrentDragItem = item;
            DragDropService.Items = Items;
            StateHasChanged();
            DragDropService.ShouldRender = false;
        }
        #endregion

        private bool IsDropAllowed()
        {
            return true;
        }

        private void OnDrop()
        {
            DragDropService.ShouldRender = true;
            if (!IsDropAllowed())
            {
                DragDropService.Reset();
                return;
            }

            var currentDragItem = DragDropService.CurrentDragItem;

            //源拖拽区才新增，目标拖拽区只移动
            if (currentDragItem.IsDropItem == false)
            {
                Items.Add(currentDragItem);
            }
            else
            {
                SwapItem(DragDropService.DragTargetItem, currentDragItem);
            }

            DragDropService.Reset();
            StateHasChanged();
            OnItemDrop.InvokeAsync(currentDragItem);
        }

        private void OnClick()
        {
            OnDragDropZoneClick.Invoke();
        }

        private void SwapItem(DragDropItem dragOverItem, DragDropItem currentDragItem)
        {
            if (dragOverItem == null)
                return;

            var indexDraggedOverItem = Items.IndexOf(dragOverItem);
            var indexActiveItem = Items.IndexOf(currentDragItem);
            if (indexDraggedOverItem > -1)
            {
                if (indexDraggedOverItem == indexActiveItem)
                    return;
                //交换位置
                DragDropItem tmp = Items[indexDraggedOverItem];
                Items[indexDraggedOverItem] = Items[indexActiveItem];
                Items[indexActiveItem] = tmp;
            }
        }

        public void Dispose()
        {
        }
    }
}