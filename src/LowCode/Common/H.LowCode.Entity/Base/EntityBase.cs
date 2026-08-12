namespace H.LowCode.Entity;

public abstract class EntityBase
{
    protected EntityBase()
    {
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }

    public string ConcurrencyStamp { get; set; }

    public DateTime CreationTime { get; set; }

    public string? CreatorId { get; set; }

    public DateTime? ModificationTime { get; set; }

    public string? ModifierId { get; set; }
}
