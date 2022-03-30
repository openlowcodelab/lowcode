using H.LowCode.DesignEngine.PropertyModels;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.DesignEngine.DesignPanel
{
    public class DragDropModel
    {
        public DragDropModel()
        {
        }

        public JSchema ComponentJSchema { get; set; }

        public RenderFragment DropRenderFragment { get; set; }

        public string Name { get; set; }

        public string ComponentType { get; set; }

        public bool IsSelected { get; internal set; }

        public double Opacity { get; internal set; } = 1;

        public string ModelStyle { get; internal set; }

        /// <summary>
        /// 是否由组件面板拖拽而来
        /// </summary>
        public bool IsDropModel { get; set; }

        /// <summary>
        /// 组件属性
        /// </summary>
        public ComponentPropertyModelBase ComponentPropertyModel { get; internal set; }

        public DragDropModel Clone()
        {
            if(ComponentPropertyModel == null)
                throw new NullReferenceException($"the property [{nameof(ComponentPropertyModel)}] is null");

            DragDropModel clone = new()
            {
                ComponentJSchema = ComponentJSchema,
                DropRenderFragment = DropRenderFragment,
                ComponentType = ComponentType,
                ComponentPropertyModel = ComponentPropertyModel
            };
            return clone;
        }
    }
}
