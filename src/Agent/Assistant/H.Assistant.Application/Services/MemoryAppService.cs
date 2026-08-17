using AutoMapper;
using H.Assistant.Application.Contracts;
using H.Assistant.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Assistant.Application;

public class MemoryAppService : ApplicationService, IMemoryAppService
{
    private readonly IRepository<KnowledgeNodeEntity, Guid> _nodeRepository;
    private readonly IRepository<KnowledgeDocumentEntity, Guid> _documentRepository;
    private readonly IMapper _objectMapper;

    public MemoryAppService(
        IRepository<KnowledgeNodeEntity, Guid> nodeRepository,
        IRepository<KnowledgeDocumentEntity, Guid> documentRepository,
        IMapper objectMapper)
    {
        _nodeRepository = nodeRepository;
        _documentRepository = documentRepository;
        _objectMapper = objectMapper;
    }

    #region Node (Tree Structure) Operations

    public async Task<BaseOutput<List<KnowledgeNodeDto>>> GetTreeAsync()
    {
        var queryable = await _nodeRepository.GetQueryableAsync();
        var allNodes = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.OwnerType == OwnerTypes.Memory).OrderBy(x => x.SortOrder));

        var allDtos = allNodes.Select(x => _objectMapper.Map<KnowledgeNodeEntity, KnowledgeNodeDto>(x)).ToList();

        // Build tree structure in memory
        var lookup = allDtos.ToLookup(x => x.ParentId);

        void AssignChildren(List<KnowledgeNodeDto> nodes)
        {
            foreach (var node in nodes)
            {
                var children = lookup[node.Id].ToList();
                node.Children = children;
                AssignChildren(children);
            }
        }

        var roots = lookup[null].ToList();
        AssignChildren(roots);

        return new() { Data = roots };
    }

    public async Task<BaseOutput<KnowledgeNodeDto>> CreateNodeAsync(CreateKnowledgeNodeDto input)
    {
        if (input.ParentId.HasValue)
        {
            var parent = await _nodeRepository.FindAsync(input.ParentId.Value);
            if (parent == null)
            {
                throw new InvalidOperationException($"父节�?{input.ParentId} 不存�?);
            }
            if (parent.OwnerType != OwnerTypes.Memory)
            {
                throw new InvalidOperationException($"父节�?{input.ParentId} 不属于记�?);
            }
        }

        var entity = _objectMapper.Map<KnowledgeNodeEntity>(input);
        entity.OwnerType = OwnerTypes.Memory;
        await _nodeRepository.InsertAsync(entity);

        // If creating a Document node, also create an empty document content
        if (entity.NodeType == "Document")
        {
            var document = new KnowledgeDocumentEntity
            {
                NodeId = entity.Id,
                Content = string.Empty
            };
            await _documentRepository.InsertAsync(document);
        }

        return new() { Data = _objectMapper.Map<KnowledgeNodeEntity, KnowledgeNodeDto>(entity) };
    }

    public async Task<BaseOutput<KnowledgeNodeDto>> UpdateNodeAsync(Guid nodeId, UpdateKnowledgeNodeDto input)
    {
        var entity = await _nodeRepository.FindAsync(nodeId);
        if (entity == null)
        {
            throw new InvalidOperationException($"节点 {nodeId} 不存�?);
        }
        if (entity.OwnerType != OwnerTypes.Memory)
        {
            throw new InvalidOperationException($"节点 {nodeId} 不属于记�?);
        }

        entity.Title = input.Title;
        entity.SortOrder = input.SortOrder;

        if (!string.IsNullOrEmpty(input.NodeType))
        {
            var oldType = entity.NodeType;
            entity.NodeType = input.NodeType;

            if (oldType == "Document" && input.NodeType == "Directory")
            {
                var queryable = await _documentRepository.GetQueryableAsync();
                var doc = await AsyncExecuter.FirstOrDefaultAsync(
                    queryable.Where(d => d.NodeId == nodeId));
                if (doc != null)
                {
                    await _documentRepository.DeleteAsync(doc);
                }
            }
            else if (oldType == "Directory" && input.NodeType == "Document")
            {
                var document = new KnowledgeDocumentEntity
                {
                    NodeId = nodeId,
                    Content = string.Empty
                };
                await _documentRepository.InsertAsync(document);
            }
        }

        await _nodeRepository.UpdateAsync(entity);

        return new() { Data = _objectMapper.Map<KnowledgeNodeEntity, KnowledgeNodeDto>(entity) };
    }

    public async Task<BaseOutput> DeleteNodeAsync(Guid nodeId)
    {
        var entity = await _nodeRepository.FindAsync(nodeId);
        if (entity == null)
        {
            throw new InvalidOperationException($"节点 {nodeId} 不存�?);
        }
        if (entity.OwnerType != OwnerTypes.Memory)
        {
            throw new InvalidOperationException($"节点 {nodeId} 不属于记�?);
        }

        // Recursively collect all descendant nodes (self-ref FK is NoAction)
        var queryable = await _nodeRepository.GetQueryableAsync();
        var allNodes = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.OwnerType == OwnerTypes.Memory));
        var descendants = new List<KnowledgeNodeEntity>();
        CollectDescendants(allNodes, nodeId, descendants);

        foreach (var desc in descendants)
        {
            await _nodeRepository.DeleteAsync(desc);
        }
        await _nodeRepository.DeleteAsync(entity);

        return new();
    }

    private static void CollectDescendants(List<KnowledgeNodeEntity> allNodes, Guid parentId, List<KnowledgeNodeEntity> result)
    {
        foreach (var node in allNodes)
        {
            if (node.ParentId == parentId)
            {
                CollectDescendants(allNodes, node.Id, result);
                result.Add(node);
            }
        }
    }

    #endregion

    #region Document Content Operations

    public async Task<BaseOutput<KnowledgeDocumentDto?>> GetDocumentAsync(Guid nodeId)
    {
        var queryable = await _documentRepository.GetQueryableAsync();
        var doc = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(d => d.NodeId == nodeId));

        if (doc == null)
        {
            return new() { Data = null };
        }

        return new() { Data = _objectMapper.Map<KnowledgeDocumentEntity, KnowledgeDocumentDto>(doc) };
    }

    public async Task<BaseOutput<KnowledgeDocumentDto>> SaveDocumentAsync(Guid nodeId, SaveKnowledgeDocumentDto input)
    {
        var queryable = await _documentRepository.GetQueryableAsync();
        var doc = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(d => d.NodeId == nodeId));

        if (doc == null)
        {
            var node = await _nodeRepository.FindAsync(nodeId);
            if (node == null || node.NodeType != "Document")
            {
                throw new InvalidOperationException($"节点 {nodeId} 不存在或不是文档类型");
            }
            if (node.OwnerType != OwnerTypes.Memory)
            {
                throw new InvalidOperationException($"节点 {nodeId} 不属于记�?);
            }

            doc = new KnowledgeDocumentEntity
            {
                NodeId = nodeId,
                Content = input.Content
            };
            await _documentRepository.InsertAsync(doc);
        }
        else
        {
            doc.Content = input.Content;
            await _documentRepository.UpdateAsync(doc);
        }

        return new() { Data = _objectMapper.Map<KnowledgeDocumentEntity, KnowledgeDocumentDto>(doc) };
    }

    #endregion

    #region Memory-specific Operations

    public async Task<BaseOutput<KnowledgeNodeDto>> CreateMemoryEntryAsync(CreateMemoryEntryDto input)
    {
        var category = input.Category?.Trim() ?? "其他";

        // Find or create the category directory node
        var queryable = await _nodeRepository.GetQueryableAsync();
        var categoryNode = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(x => x.OwnerType == OwnerTypes.Memory
                                 && x.NodeType == "Directory"
                                 && x.ParentId == null
                                 && x.Title == category));

        if (categoryNode == null)
        {
            categoryNode = new KnowledgeNodeEntity
            {
                Title = category,
                NodeType = "Directory",
                OwnerType = OwnerTypes.Memory,
                ParentId = null
            };
            await _nodeRepository.InsertAsync(categoryNode);
        }

        // Create a Document node under the category directory
        var memoryNode = new KnowledgeNodeEntity
        {
            Title = input.Title.Trim(),
            NodeType = "Document",
            OwnerType = OwnerTypes.Memory,
            ParentId = categoryNode.Id
        };
        await _nodeRepository.InsertAsync(memoryNode);

        // Create document content
        var document = new KnowledgeDocumentEntity
        {
            NodeId = memoryNode.Id,
            Content = input.Content.Trim()
        };
        await _documentRepository.InsertAsync(document);

        return new() { Data = _objectMapper.Map<KnowledgeNodeEntity, KnowledgeNodeDto>(memoryNode) };
    }

    #endregion
}
