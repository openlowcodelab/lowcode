namespace H.Abstractions;

/// <summary>
/// 带审计信息的实体 DTO 基类（与 ABP 的 FullAuditedEntityDto 保持相同 JSON 序列化结构）
/// </summary>
public abstract class FullAuditedEntityDto<TKey> : AuditedEntityDto<TKey>
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletionTime { get; set; }
    public Guid? DeleterId { get; set; }
}
