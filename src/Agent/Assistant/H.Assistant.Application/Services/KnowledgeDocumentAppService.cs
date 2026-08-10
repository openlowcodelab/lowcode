using AutoMapper;
using H.Assistant.Application.Contracts;
using H.Assistant.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Assistant.Application;

public class KnowledgeDocumentAppService : ApplicationService, IKnowledgeDocumentAppService
{
    private readonly IRepository<KnowledgeBaseEntity, Guid> _knowledgeBaseRepository;
    private readonly IRepository<KnowledgeNodeEntity, Guid> _nodeRepository;
    private readonly IRepository<KnowledgeDocumentEntity, Guid> _documentRepository;
    private readonly IMapper _objectMapper;

    public KnowledgeDocumentAppService(
        IRepository<KnowledgeBaseEntity, Guid> knowledgeBaseRepository,
        IRepository<KnowledgeNodeEntity, Guid> nodeRepository,
        IRepository<KnowledgeDocumentEntity, Guid> documentRepository,
        IMapper objectMapper)
    {
        _knowledgeBaseRepository = knowledgeBaseRepository;
        _nodeRepository = nodeRepository;
        _documentRepository = documentRepository;
        _objectMapper = objectMapper;
    }

    #region Node (Tree Structure) Operations

    public async Task<List<KnowledgeNodeDto>> GetTreeAsync(Guid knowledgeBaseId)
    {
        var queryable = await _nodeRepository.GetQueryableAsync();
        var allNodes = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.OwnerType == OwnerTypes.Knowledge && x.KnowledgeBaseId == knowledgeBaseId)
                .OrderBy(x => x.SortOrder));

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

        return roots;
    }

    public async Task<KnowledgeNodeDto> CreateNodeAsync(CreateKnowledgeNodeDto input)
    {
        if (input.ParentId.HasValue)
        {
            var parent = await _nodeRepository.FindAsync(input.ParentId.Value);
            if (parent == null)
            {
                throw new InvalidOperationException($"父节点 {input.ParentId} 不存在");
            }

            // 子节点继承父节点所属知识库
            input.KnowledgeBaseId = parent.KnowledgeBaseId;
        }
        else if (!input.KnowledgeBaseId.HasValue)
        {
            throw new InvalidOperationException("根节点必须指定所属知识库");
        }
        else if (await _knowledgeBaseRepository.FindAsync(input.KnowledgeBaseId.Value) == null)
        {
            throw new InvalidOperationException($"知识库 {input.KnowledgeBaseId} 不存在");
        }

        var entity = _objectMapper.Map<KnowledgeNodeEntity>(input);
        entity.OwnerType = OwnerTypes.Knowledge;
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

        return _objectMapper.Map<KnowledgeNodeEntity, KnowledgeNodeDto>(entity);
    }

    public async Task<KnowledgeNodeDto> UpdateNodeAsync(Guid nodeId, UpdateKnowledgeNodeDto input)
    {
        var entity = await _nodeRepository.FindAsync(nodeId);
        if (entity == null)
        {
            throw new InvalidOperationException($"节点 {nodeId} 不存在");
        }
        if (entity.OwnerType != OwnerTypes.Knowledge)
        {
            throw new InvalidOperationException($"节点 {nodeId} 不属于知识库");
        }

        entity.Title = input.Title;
        entity.SortOrder = input.SortOrder;

        if (!string.IsNullOrEmpty(input.NodeType))
        {
            var oldType = entity.NodeType;
            entity.NodeType = input.NodeType;

            // If switching from Document to Directory, remove document content
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
            // If switching from Directory to Document, create empty document content
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

        return _objectMapper.Map<KnowledgeNodeEntity, KnowledgeNodeDto>(entity);
    }

    public async Task DeleteNodeAsync(Guid nodeId)
    {
        var entity = await _nodeRepository.FindAsync(nodeId);
        if (entity == null)
        {
            throw new InvalidOperationException($"节点 {nodeId} 不存在");
        }
        if (entity.OwnerType != OwnerTypes.Knowledge)
        {
            throw new InvalidOperationException($"节点 {nodeId} 不属于知识库");
        }

        // Recursively collect all descendant nodes (self-ref FK is NoAction)
        var queryable = await _nodeRepository.GetQueryableAsync();
        var allNodes = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.OwnerType == OwnerTypes.Knowledge));
        var descendants = new List<KnowledgeNodeEntity>();
        CollectDescendants(allNodes, nodeId, descendants);

        // Delete descendants bottom-up, then the node itself
        // KnowledgeDocument cascade delete handles document content
        foreach (var desc in descendants)
        {
            await _nodeRepository.DeleteAsync(desc);
        }
        await _nodeRepository.DeleteAsync(entity);
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

    public async Task<KnowledgeDocumentDto?> GetDocumentAsync(Guid nodeId)
    {
        var queryable = await _documentRepository.GetQueryableAsync();
        var doc = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(d => d.NodeId == nodeId));

        if (doc == null)
        {
            return null;
        }

        return _objectMapper.Map<KnowledgeDocumentEntity, KnowledgeDocumentDto>(doc);
    }

    public async Task<KnowledgeDocumentDto> SaveDocumentAsync(Guid nodeId, SaveKnowledgeDocumentDto input)
    {
        var queryable = await _documentRepository.GetQueryableAsync();
        var doc = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(d => d.NodeId == nodeId));

        if (doc == null)
        {
            // Verify the node exists, is a Document type, and belongs to Knowledge
            var node = await _nodeRepository.FindAsync(nodeId);
            if (node == null || node.NodeType != "Document")
            {
                throw new InvalidOperationException($"节点 {nodeId} 不存在或不是文档类型");
            }
            if (node.OwnerType != OwnerTypes.Knowledge)
            {
                throw new InvalidOperationException($"节点 {nodeId} 不属于知识库");
            }

            // Create new document content
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

        return _objectMapper.Map<KnowledgeDocumentEntity, KnowledgeDocumentDto>(doc);
    }

    #endregion
}
