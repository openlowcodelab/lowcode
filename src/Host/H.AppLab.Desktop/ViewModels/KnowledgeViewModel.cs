using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using H.Assistant.Application.Contracts;
using H.AppLab.Desktop.Services;

namespace H.AppLab.Desktop.ViewModels;

/// <summary>
/// 知识中心页 ViewModel（对应 Web 端 Knowledge.razor，知识库/记忆双 Tab）
/// </summary>
public partial class KnowledgeViewModel : ObservableObject
{
    private bool _initialized;

    public KnowledgeSectionViewModel KnowledgeSection { get; }
    public KnowledgeSectionViewModel MemorySection { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsKnowledgeTab))]
    [NotifyPropertyChangedFor(nameof(IsMemoryTab))]
    private string activeTab = "knowledge";

    public bool IsKnowledgeTab => ActiveTab == "knowledge";
    public bool IsMemoryTab => ActiveTab == "memory";

    public KnowledgeViewModel(IKnowledgeDocumentAppService knowledgeDocumentAppService,
        IMemoryAppService memoryAppService, ToastService toast)
    {
        // 知识库与记忆共用同一套节点/文档操作，方法签名一致，用适配器委托复用逻辑
        KnowledgeSection = new KnowledgeSectionViewModel(toast, new KnowledgeServiceAdapter(
            knowledgeDocumentAppService.GetTreeAsync,
            knowledgeDocumentAppService.CreateNodeAsync,
            knowledgeDocumentAppService.UpdateNodeAsync,
            knowledgeDocumentAppService.DeleteNodeAsync,
            knowledgeDocumentAppService.GetDocumentAsync,
            knowledgeDocumentAppService.SaveDocumentAsync))
        {
            LoadFailedMessage = "加载知识库失败",
            RootDirectoryTitle = "新建目录",
            RootDocumentTitle = "新建文档",
            ChildDocumentTitle = "新建文档",
            DocumentButtonText = "+ 文档",
            EmptyText = "暂无文档",
            EmptyHint = "点击上方按钮创建"
        };

        MemorySection = new KnowledgeSectionViewModel(toast, new KnowledgeServiceAdapter(
            memoryAppService.GetTreeAsync,
            memoryAppService.CreateNodeAsync,
            memoryAppService.UpdateNodeAsync,
            memoryAppService.DeleteNodeAsync,
            memoryAppService.GetDocumentAsync,
            memoryAppService.SaveDocumentAsync))
        {
            LoadFailedMessage = "加载记忆失败",
            RootDirectoryTitle = "新建分类",
            RootDocumentTitle = "新建记忆",
            ChildDocumentTitle = "新建记忆",
            DocumentButtonText = "+ 记忆",
            EmptyText = "暂无记忆",
            EmptyHint = "对话后将自动提取"
        };
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }
        _initialized = true;
        await KnowledgeSection.LoadTreeAsync();
        await MemorySection.LoadTreeAsync();
    }

    [RelayCommand]
    private void SwitchTab(string tab)
    {
        ActiveTab = tab;
    }
}

/// <summary>
/// 知识树服务适配器（IKnowledgeDocumentAppService / IMemoryAppService 方法签名一致）
/// </summary>
public record KnowledgeServiceAdapter(
    Func<Task<List<KnowledgeNodeDto>>> GetTreeAsync,
    Func<CreateKnowledgeNodeDto, Task<KnowledgeNodeDto>> CreateNodeAsync,
    Func<Guid, UpdateKnowledgeNodeDto, Task<KnowledgeNodeDto>> UpdateNodeAsync,
    Func<Guid, Task> DeleteNodeAsync,
    Func<Guid, Task<KnowledgeDocumentDto?>> GetDocumentAsync,
    Func<Guid, SaveKnowledgeDocumentDto, Task<KnowledgeDocumentDto>> SaveDocumentAsync);

/// <summary>
/// 知识树 + 内容面板区块 ViewModel（知识库 / 记忆各一个实例）
/// </summary>
public partial class KnowledgeSectionViewModel : ObservableObject
{
    private readonly ToastService _toast;
    private readonly KnowledgeServiceAdapter _service;
    private readonly Dictionary<Guid, KnowledgeNodeDto> _flatMap = [];
    private readonly Dictionary<Guid, bool> _expandedState = [];

    public string LoadFailedMessage { get; init; } = "加载失败";
    public string RootDirectoryTitle { get; init; } = "新建目录";
    public string RootDocumentTitle { get; init; } = "新建文档";
    public string ChildDocumentTitle { get; init; } = "新建文档";
    public string DocumentButtonText { get; init; } = "+ 文档";
    public string EmptyText { get; init; } = "暂无文档";
    public string EmptyHint { get; init; } = "点击上方按钮创建";

    public ObservableCollection<KnowledgeNodeItem> TreeNodes { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNodes))]
    private bool treeLoaded;

    public bool HasNodes => TreeNodes.Count > 0;

    [ObservableProperty]
    private Guid? selectedNodeId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    private string selectedTitle = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasContent))]
    [NotifyPropertyChangedFor(nameof(ShowEmptyContentHint))]
    private string? selectedContent;

    [ObservableProperty]
    private bool selectedIsDocument;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyContentHint))]
    private bool isEditing;

    [ObservableProperty]
    private bool isEditingTitle;

    [ObservableProperty]
    private string editingContent = string.Empty;

    [ObservableProperty]
    private string editingTitle = string.Empty;

    public bool HasSelection => !string.IsNullOrEmpty(SelectedTitle);
    public bool HasContent => !string.IsNullOrEmpty(SelectedContent);
    public bool ShowEmptyContentHint => !IsEditing && string.IsNullOrEmpty(SelectedContent);

    public KnowledgeSectionViewModel(ToastService toast, KnowledgeServiceAdapter service)
    {
        _toast = toast;
        _service = service;
    }

    public async Task LoadTreeAsync()
    {
        try
        {
            var tree = await _service.GetTreeAsync();
            _flatMap.Clear();
            SaveExpandedState(TreeNodes);
            TreeNodes.Clear();
            foreach (var node in tree)
            {
                TreeNodes.Add(BuildNodeItem(node));
            }
        }
        catch (Exception ex)
        {
            _toast.Show($"{LoadFailedMessage}: {ex.Message}", "error");
        }
        finally
        {
            TreeLoaded = true;
            OnPropertyChanged(nameof(HasNodes));
        }
    }

    private void SaveExpandedState(IEnumerable<KnowledgeNodeItem> nodes)
    {
        foreach (var node in nodes)
        {
            _expandedState[node.Id] = node.IsExpanded;
            SaveExpandedState(node.Children);
        }
    }

    private KnowledgeNodeItem BuildNodeItem(KnowledgeNodeDto dto)
    {
        _flatMap[dto.Id] = dto;
        var item = new KnowledgeNodeItem(dto, this)
        {
            IsExpanded = _expandedState.GetValueOrDefault(dto.Id),
            IsSelected = dto.Id == SelectedNodeId
        };
        foreach (var child in dto.Children)
        {
            item.Children.Add(BuildNodeItem(child));
        }
        return item;
    }

    private void UpdateSelection(IEnumerable<KnowledgeNodeItem> nodes)
    {
        foreach (var node in nodes)
        {
            node.IsSelected = node.Id == SelectedNodeId;
            UpdateSelection(node.Children);
        }
    }

    [RelayCommand]
    private async Task SelectNodeAsync(KnowledgeNodeItem node)
    {
        SelectedNodeId = node.Id;
        UpdateSelection(TreeNodes);
        IsEditing = false;
        IsEditingTitle = false;

        if (_flatMap.TryGetValue(node.Id, out var dto))
        {
            SelectedTitle = dto.Title;
            SelectedIsDocument = dto.NodeType == "Document";

            if (SelectedIsDocument)
            {
                try
                {
                    var doc = await _service.GetDocumentAsync(node.Id);
                    SelectedContent = doc?.Content;
                }
                catch
                {
                    SelectedContent = null;
                }
            }
            else
            {
                SelectedContent = null;
            }
        }
    }

    [RelayCommand]
    private async Task CreateRootNodeAsync(string nodeType)
    {
        try
        {
            await _service.CreateNodeAsync(new CreateKnowledgeNodeDto
            {
                Title = nodeType == "Directory" ? RootDirectoryTitle : RootDocumentTitle,
                NodeType = nodeType
            });
            await LoadTreeAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"创建失败: {ex.Message}", "error");
        }
    }

    [RelayCommand]
    private async Task CreateChildDocumentAsync(KnowledgeNodeItem parent)
    {
        parent.IsMenuOpen = false;
        try
        {
            await _service.CreateNodeAsync(new CreateKnowledgeNodeDto
            {
                ParentId = parent.Id,
                Title = ChildDocumentTitle,
                NodeType = "Document"
            });
            _expandedState[parent.Id] = true;
            await LoadTreeAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"创建失败: {ex.Message}", "error");
        }
    }

    [RelayCommand]
    private async Task CreateChildDirectoryAsync(KnowledgeNodeItem parent)
    {
        parent.IsMenuOpen = false;
        try
        {
            await _service.CreateNodeAsync(new CreateKnowledgeNodeDto
            {
                ParentId = parent.Id,
                Title = "新建子目录",
                NodeType = "Directory"
            });
            _expandedState[parent.Id] = true;
            await LoadTreeAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"创建失败: {ex.Message}", "error");
        }
    }

    [RelayCommand]
    private async Task DeleteNodeAsync(KnowledgeNodeItem node)
    {
        node.IsMenuOpen = false;
        try
        {
            await _service.DeleteNodeAsync(node.Id);
            if (SelectedNodeId == node.Id)
            {
                SelectedNodeId = null;
                SelectedTitle = string.Empty;
                SelectedContent = null;
                SelectedIsDocument = false;
            }
            await LoadTreeAsync();
            _toast.Show("已删除", "success");
        }
        catch (Exception ex)
        {
            _toast.Show($"删除失败: {ex.Message}", "error");
        }
    }

    public async Task RenameNodeAsync(Guid nodeId, string newTitle)
    {
        try
        {
            if (_flatMap.TryGetValue(nodeId, out var node))
            {
                await _service.UpdateNodeAsync(nodeId, new UpdateKnowledgeNodeDto
                {
                    Title = newTitle,
                    SortOrder = node.SortOrder
                });
                await LoadTreeAsync();

                if (SelectedNodeId == nodeId)
                {
                    SelectedTitle = newTitle;
                }
            }
        }
        catch (Exception ex)
        {
            _toast.Show($"重命名失败: {ex.Message}", "error");
        }
    }

    #region 内容面板（对应 ContentPanel）

    [RelayCommand]
    private void StartEdit()
    {
        if (!SelectedIsDocument)
        {
            return;
        }
        EditingContent = SelectedContent ?? string.Empty;
        IsEditing = true;
    }

    [RelayCommand]
    private void TogglePreview()
    {
        IsEditing = false;
    }

    [RelayCommand]
    private void StartTitleEdit()
    {
        if (!SelectedIsDocument)
        {
            return;
        }
        EditingTitle = SelectedTitle;
        IsEditingTitle = true;
    }

    [RelayCommand]
    private async Task CommitTitleEditAsync()
    {
        if (IsEditingTitle && !string.IsNullOrWhiteSpace(EditingTitle) && SelectedNodeId.HasValue)
        {
            await RenameNodeAsync(SelectedNodeId.Value, EditingTitle.Trim());
        }
        IsEditingTitle = false;
    }

    [RelayCommand]
    private void CancelTitleEdit()
    {
        IsEditingTitle = false;
    }

    [RelayCommand]
    private async Task SaveContentAsync()
    {
        if (!SelectedNodeId.HasValue)
        {
            return;
        }

        var title = IsEditingTitle ? EditingTitle.Trim() : SelectedTitle;
        try
        {
            // 标题有变化时先更新节点
            if (_flatMap.TryGetValue(SelectedNodeId.Value, out var node) && node.Title != title && !string.IsNullOrWhiteSpace(title))
            {
                await _service.UpdateNodeAsync(SelectedNodeId.Value, new UpdateKnowledgeNodeDto
                {
                    Title = title,
                    SortOrder = node.SortOrder
                });
                SelectedTitle = title;
            }

            await _service.SaveDocumentAsync(SelectedNodeId.Value, new SaveKnowledgeDocumentDto
            {
                Content = EditingContent
            });
            SelectedContent = EditingContent;
            await LoadTreeAsync();
            _toast.Show("已保存", "success");
        }
        catch (Exception ex)
        {
            _toast.Show($"保存失败: {ex.Message}", "error");
        }
        finally
        {
            IsEditing = false;
            IsEditingTitle = false;
        }
    }

    #endregion
}

/// <summary>
/// 知识树节点项（递归）
/// </summary>
public partial class KnowledgeNodeItem : ObservableObject
{
    public KnowledgeNodeItem(KnowledgeNodeDto dto, KnowledgeSectionViewModel section)
    {
        Dto = dto;
        Section = section;
    }

    public KnowledgeNodeDto Dto { get; }

    /// <summary>所属区块（供模板内命令绑定）</summary>
    public KnowledgeSectionViewModel Section { get; }

    public ObservableCollection<KnowledgeNodeItem> Children { get; } = [];

    public Guid Id => Dto.Id;
    public string Title => Dto.Title;
    public bool IsDirectory => Dto.NodeType == "Directory";
    public bool HasChildren => Dto.Children.Count > 0;
    public bool ShowArrow => IsDirectory && HasChildren;

    [ObservableProperty]
    private bool isExpanded;

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private bool isMenuOpen;

    [ObservableProperty]
    private bool isRenaming;

    [ObservableProperty]
    private string renamingTitle = string.Empty;

    [RelayCommand]
    private async Task ClickAsync()
    {
        await Section.SelectNodeCommand.ExecuteAsync(this);
        if (IsDirectory)
        {
            IsExpanded = !IsExpanded;
        }
    }

    [RelayCommand]
    private void ToggleExpand()
    {
        IsExpanded = !IsExpanded;
    }

    [RelayCommand]
    private void ToggleMenu()
    {
        IsMenuOpen = !IsMenuOpen;
    }

    [RelayCommand]
    private void StartRename()
    {
        IsMenuOpen = false;
        RenamingTitle = Title;
        IsRenaming = true;
    }

    [RelayCommand]
    private async Task CommitRenameAsync()
    {
        if (IsRenaming && !string.IsNullOrWhiteSpace(RenamingTitle))
        {
            var newTitle = RenamingTitle.Trim();
            IsRenaming = false;
            if (newTitle != Title)
            {
                await Section.RenameNodeAsync(Id, newTitle);
                return;
            }
        }
        IsRenaming = false;
    }

    [RelayCommand]
    private void CancelRename()
    {
        IsRenaming = false;
    }
}
