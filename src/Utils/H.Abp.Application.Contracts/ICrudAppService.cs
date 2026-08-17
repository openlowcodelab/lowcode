using H.Util.Base;

namespace H.Abp.Application.Contracts;

/// <summary>
/// CRUD 应用服务接口（与 ABP 的 ICrudAppService 保持相同签名）
/// </summary>
public interface ICrudAppService<TEntityDto, in TKey, in TGetListInput, in TCreateInput, in TUpdateInput> : IAppService
{
    Task<BaseOutput<TEntityDto>> GetAsync(TKey id);
    Task<BaseOutput<PagedResultDto<TEntityDto>>> GetListAsync(TGetListInput input);
    Task<BaseOutput<TEntityDto>> CreateAsync(TCreateInput input);
    Task<BaseOutput<TEntityDto>> UpdateAsync(TKey id, TUpdateInput input);
    Task<BaseOutput> DeleteAsync(TKey id);
}
