namespace H.LowCode.RenderEngineBase;

/// <summary>
/// List 数据操作管理器 - 平台层通用能力
/// 管理列表数据的增删改查和排序操作
/// </summary>
public class ListDataOperationManager
{
    /// <summary>
    /// 平台约定的主键字段名
    /// </summary>
    public const string PrimaryKeyFieldName = "f_id";

    private readonly Dictionary<string, IList<object>> _listDataStore = new();

    /// <summary>
    /// 列表数据变更通知（用户操作引发的增删移/复制）
    /// </summary>
    public event Action? OnChange;

    private void NotifyChange()
    {
        OnChange?.Invoke();
    }

    /// <summary>
    /// 数据来源于数据库的列表（保存时需同步删除被移除的行）
    /// </summary>
    private readonly HashSet<string> _dbSyncedListIds = [];

    /// <summary>
    /// 编辑过程中被删除的行主键（保存时同步删除）
    /// </summary>
    private readonly Dictionary<string, List<string>> _deletedIdsStore = new();

    /// <summary>
    /// 注册列表数据
    /// </summary>
    /// <param name="fromDatabase">数据是否来源于数据库（是则保存时同步删除被移除的行）</param>
    public void RegisterListData(string listId, IList<object> data, bool fromDatabase = false)
    {
        _listDataStore[listId] = data;

        if (fromDatabase)
        {
            _dbSyncedListIds.Add(listId);
            _deletedIdsStore[listId] = new List<string>();
        }
    }

    /// <summary>
    /// 获取列表数据
    /// </summary>
    public IList<object> GetListData(string listId)
    {
        return _listDataStore.TryGetValue(listId, out var data) ? data : new List<object>();
    }

    /// <summary>
    /// 获取列表数据（未注册时自动注册空列表）
    /// </summary>
    private IList<object> GetOrRegisterList(string listId)
    {
        if (!_listDataStore.TryGetValue(listId, out var data))
        {
            data = new List<object>();
            _listDataStore[listId] = data;
        }
        return data;
    }

    /// <summary>
    /// 列表数据是否已注册
    /// </summary>
    public bool HasListData(string listId)
    {
        return _listDataStore.ContainsKey(listId);
    }

    /// <summary>
    /// 列表数据是否来源于数据库
    /// </summary>
    public bool IsDbSynced(string listId)
    {
        return _dbSyncedListIds.Contains(listId);
    }

    /// <summary>
    /// 获取并清空编辑过程中被删除的行主键
    /// </summary>
    public IList<string> GetAndClearDeletedIds(string listId)
    {
        if (!_deletedIdsStore.TryGetValue(listId, out var deletedIds) || deletedIds.Count == 0)
            return new List<string>();

        var result = deletedIds.ToList();
        deletedIds.Clear();
        return result;
    }

    /// <summary>
    /// 移除列表数据（页面卸载等场景）
    /// </summary>
    public void RemoveListData(string listId)
    {
        _listDataStore.Remove(listId);
        _dbSyncedListIds.Remove(listId);
        _deletedIdsStore.Remove(listId);
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
        NotifyChange();
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
        NotifyChange();
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

        // 记录被删除行的主键，保存时同步删除数据库记录
        if (_dbSyncedListIds.Contains(listId)
            && GetItemPrimaryKey(list[index]) is string pk
            && !string.IsNullOrEmpty(pk))
        {
            if (!_deletedIdsStore.TryGetValue(listId, out var deletedIds))
            {
                deletedIds = new List<string>();
                _deletedIdsStore[listId] = deletedIds;
            }
            deletedIds.Add(pk);
        }

        list.RemoveAt(index);
        NotifyChange();
        return true;
    }

    /// <summary>
    /// 添加项目
    /// </summary>
    public void AddItem(string listId, object item)
    {
        var list = GetOrRegisterList(listId);
        list.Add(item);
        NotifyChange();
    }

    /// <summary>
    /// 添加默认项目（生成带主键的空行）
    /// </summary>
    public void AddDefaultItem(string listId)
    {
        var list = GetOrRegisterList(listId);

        var newItem = new Dictionary<string, object>
        {
            [PrimaryKeyFieldName] = Guid.NewGuid().ToString()
        };

        list.Add(newItem);
        NotifyChange();
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
            // 深拷贝字典并重新生成主键
            var newDict = new Dictionary<string, object>(dict);
            newDict[PrimaryKeyFieldName] = Guid.NewGuid().ToString();
            newItem = newDict;
        }
        else
        {
            // 对于其他类型，直接添加引用（简单处理）
            newItem = sourceItem;
        }

        list.Insert(index + 1, newItem);
        NotifyChange();
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

    /// <summary>
    /// 获取行数据主键值
    /// </summary>
    public static object? GetItemPrimaryKey(object? item)
    {
        return LowCodeExpressionResolver.GetMemberValue(item, PrimaryKeyFieldName);
    }
}
