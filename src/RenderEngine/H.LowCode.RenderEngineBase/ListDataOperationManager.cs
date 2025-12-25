using System;
using System.Collections.Generic;
using System.Linq;

namespace H.LowCode.RenderEngineBase;

/// <summary>
/// List 数据操作管理器 - 平台层通用能力
/// 管理列表数据的增删改查和排序操作
/// </summary>
public class ListDataOperationManager
{
    private readonly Dictionary<string, IList<object>> _listDataStore = new();

    /// <summary>
    /// 注册列表数据
    /// </summary>
    public void RegisterListData(string listId, IList<object> data)
    {
        _listDataStore[listId] = data;
    }

    /// <summary>
    /// 获取列表数据
    /// </summary>
    public IList<object> GetListData(string listId)
    {
        return _listDataStore.TryGetValue(listId, out var data) ? data : new List<object>();
    }

    /// <summary>
    /// 上移项目
    /// </summary>
    public bool MoveUp(string listId, int index)
    {
        var list = GetListData(listId);
        if (index <= 0 || index >= list.Count)
            return false;

        var item = list[index];
        list.RemoveAt(index);
        list.Insert(index - 1, item);
        return true;
    }

    /// <summary>
    /// 下移项目
    /// </summary>
    public bool MoveDown(string listId, int index)
    {
        var list = GetListData(listId);
        if (index < 0 || index >= list.Count - 1)
            return false;

        var item = list[index];
        list.RemoveAt(index);
        list.Insert(index + 1, item);
        return true;
    }

    /// <summary>
    /// 删除项目
    /// </summary>
    public bool DeleteItem(string listId, int index)
    {
        var list = GetListData(listId);
        if (index < 0 || index >= list.Count)
            return false;

        list.RemoveAt(index);
        return true;
    }

    /// <summary>
    /// 添加项目
    /// </summary>
    public void AddItem(string listId, object item)
    {
        var list = GetListData(listId);
        list.Add(item);
    }

    /// <summary>
    /// 添加默认项目（根据列表现有结构创建）
    /// </summary>
    public void AddDefaultItem(string listId)
    {
        var list = GetListData(listId);
        
        // 创建一个默认的字典项
        var newItem = new Dictionary<string, object>
        {
            ["f_id"] = Guid.NewGuid().ToString(),
            ["f_title"] = "新问题",
            ["f_question_type"] = 1,
            ["f_description"] = "",
            ["f_is_required"] = false,
            ["f_order"] = list.Count + 1,
            ["f_options_json"] = "[]"
        };

        list.Add(newItem);
    }

    /// <summary>
    /// 复制项目
    /// </summary>
    public bool CopyItem(string listId, int index)
    {
        var list = GetListData(listId);
        if (index < 0 || index >= list.Count)
            return false;

        var sourceItem = list[index];
        object newItem;

        if (sourceItem is Dictionary<string, object> dict)
        {
            // 深拷贝字典
            var newDict = new Dictionary<string, object>(dict);
            newDict["f_id"] = Guid.NewGuid().ToString();
            if (newDict.ContainsKey("f_title"))
            {
                newDict["f_title"] = newDict["f_title"]?.ToString() + " (副本)";
            }
            if (newDict.ContainsKey("f_order"))
            {
                newDict["f_order"] = list.Count + 1;
            }
            newItem = newDict;
        }
        else
        {
            // 对于其他类型，直接添加引用（简单处理）
            newItem = sourceItem;
        }

        list.Insert(index + 1, newItem);
        return true;
    }

    /// <summary>
    /// 更新项目顺序字段
    /// </summary>
    public void UpdateOrderFields(string listId, string orderFieldName = "f_order")
    {
        var list = GetListData(listId);
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] is Dictionary<string, object> dict)
            {
                dict[orderFieldName] = i + 1;
            }
        }
    }
}
