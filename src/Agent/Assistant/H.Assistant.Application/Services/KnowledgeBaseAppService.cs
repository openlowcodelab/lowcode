using AutoMapper;
using H.Assistant.Application.Contracts;
using H.Assistant.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Assistant.Application;

public class KnowledgeBaseAppService : ApplicationService, IKnowledgeBaseAppService
{
    private readonly IRepository<KnowledgeBaseEntity, Guid> _knowledgeBaseRepository;
    private readonly IRepository<KnowledgeNodeEntity, Guid> _nodeRepository;
    private readonly IRepository<KnowledgeDocumentEntity, Guid> _documentRepository;
    private readonly IMapper _objectMapper;

    public KnowledgeBaseAppService(
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

    public async Task<List<KnowledgeBaseDto>> GetListAsync()
    {
        var queryable = await _knowledgeBaseRepository.GetQueryableAsync();
        var bases = await AsyncExecuter.ToListAsync(queryable.OrderBy(x => x.SortOrder));

        // 统计每个知识库的文档数量
        var nodeQueryable = await _nodeRepository.GetQueryableAsync();
        var docCounts = (await AsyncExecuter.ToListAsync(
                nodeQueryable.Where(x => x.OwnerType == OwnerTypes.Knowledge
                    && x.NodeType == "Document"
                    && x.KnowledgeBaseId != null)))
            .GroupBy(x => x.KnowledgeBaseId)
            .ToDictionary(g => g.Key!.Value, g => g.Count());

        var result = bases.Select(x =>
        {
            var dto = _objectMapper.Map<KnowledgeBaseEntity, KnowledgeBaseDto>(x);
            dto.DocumentCount = docCounts.GetValueOrDefault(x.Id);
            return dto;
        }).ToList();

        return result;
    }

    public async Task<KnowledgeBaseDto> GetAsync(Guid id)
    {
        var entity = await _knowledgeBaseRepository.FindAsync(id);
        if (entity == null)
        {
            throw new InvalidOperationException($"知识库 {id} 不存在");
        }

        return _objectMapper.Map<KnowledgeBaseEntity, KnowledgeBaseDto>(entity);
    }

    public async Task<KnowledgeBaseDto> CreateAsync(CreateKnowledgeBaseDto input)
    {
        await EnsureNameUniqueAsync(input.Name);

        var entity = _objectMapper.Map<KnowledgeBaseEntity>(input);
        await _knowledgeBaseRepository.InsertAsync(entity);

        return _objectMapper.Map<KnowledgeBaseEntity, KnowledgeBaseDto>(entity);
    }

    public async Task<KnowledgeBaseDto> UpdateAsync(Guid id, UpdateKnowledgeBaseDto input)
    {
        var entity = await _knowledgeBaseRepository.FindAsync(id);
        if (entity == null)
        {
            throw new InvalidOperationException($"知识库 {id} 不存在");
        }

        await EnsureNameUniqueAsync(input.Name, id);

        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.SortOrder = input.SortOrder;
        await _knowledgeBaseRepository.UpdateAsync(entity);

        return _objectMapper.Map<KnowledgeBaseEntity, KnowledgeBaseDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _knowledgeBaseRepository.FindAsync(id);
        if (entity == null)
        {
            throw new InvalidOperationException($"知识库 {id} 不存在");
        }

        // 删除知识库下的所有节点及其文档内容
        var nodeQueryable = await _nodeRepository.GetQueryableAsync();
        var nodes = await AsyncExecuter.ToListAsync(
            nodeQueryable.Where(x => x.KnowledgeBaseId == id));
        var nodeIds = nodes.Select(x => x.Id).ToList();

        var docQueryable = await _documentRepository.GetQueryableAsync();
        var docs = await AsyncExecuter.ToListAsync(
            docQueryable.Where(d => d.NodeId != null && nodeIds.Contains(d.NodeId.Value)));
        foreach (var doc in docs)
        {
            await _documentRepository.DeleteAsync(doc);
        }
        foreach (var node in nodes)
        {
            await _nodeRepository.DeleteAsync(node);
        }

        await _knowledgeBaseRepository.DeleteAsync(entity);
    }

    private async Task EnsureNameUniqueAsync(string name, Guid? excludeId = null)
    {
        var queryable = await _knowledgeBaseRepository.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(
            queryable.Where(x => x.Name == name && (!excludeId.HasValue || x.Id != excludeId.Value)));
        if (exists)
        {
            throw new InvalidOperationException($"知识库名称 {name} 已存在");
        }
    }
}
