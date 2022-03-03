using System;
using System.Collections.Generic;

namespace H.LowCode.BlazorExtensions
{
    public static class BlazorEventDispatcher
    {
        /// <summary>
        /// key 格式建议(小写)：{组件库名称}.{组件名称}.{事件名称}
        /// 如：designerengine.dragitem.onclick
        /// </summary>
        private static Dictionary<string, Action<object>> _actions;
        static BlazorEventDispatcher()
        {
            _actions = new Dictionary<string, Action<object>>();
        }

        public static void AddAction(string key, Action<object> action)
        {
            if (!_actions.ContainsKey(key))
            {
                _actions.Add(key, action);
            }
            else
            {
                _actions[key] += action;
                //throw new Exception($"event key{key} has existed");
            }
        }

        public static void RemoveAction(string key)
        {
            if (_actions.ContainsKey(key))
            {
                _actions.Remove(key);
            }
        }

        public static void Dispatch(string key, object value)
        {
            if (_actions.ContainsKey(key))
            {
                var act = _actions[key];
                act.Invoke(value);
            }

        }
    }
}
