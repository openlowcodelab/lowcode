namespace H.Abstractions;

public abstract class EntityDto<TKey>
{
    public TKey Id { get; set; } = default!;
}
