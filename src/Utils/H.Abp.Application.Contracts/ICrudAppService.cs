namespace H.Abp.Application.Contracts;

/// <summary>
/// CRUD 应用服务接口（与 ABP 的 ICrudAppService 保持相同签名）
/// </summary>
public interface ICrudAppService<TEntityDto, in TKey, in TGetListInput, in TCreateInput, in TUpdateInput> : IAppService
{
    Task<TEntityDto> GetAsync(TKey id);
    Task<PagedResultDto<TEntityDto>> GetListAsync(TGetListInput input);
    Task<TEntityDto> CreateAsync(TCreateInput input);
    Task<TEntityDto> UpdateAsync(TKey id, TUpdateInput input);
    Task DeleteAsync(TKey id);
}
