using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.LowCode.Application.Contracts;

public interface IFormDataAppService : IAppService
{
    /// <summary>
    /// 保存表单数据（按主键新增或更新），返回主键值
    /// </summary>
    Task<BaseOutput<string>> SaveAsync(FormDataDto dto);

    Task<BaseOutput<FormDataDto>> GetAsync(string appId, string pageId, string id);

    Task<BaseOutput<bool>> DeleteAsync(string appId, string pageId, string id);
}