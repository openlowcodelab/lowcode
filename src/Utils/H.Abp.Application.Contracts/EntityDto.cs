namespace H.Abp.Application.Contracts;

public abstract class EntityDto<TKey>
{
    public TKey Id { get; set; } = default!;
}
