using H.Abp.Application.Contracts;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Base;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IAppRbacAppService : IAppService
{
    Task<BaseOutput<List<AppMemberSchema>>> GetMembersAsync(string appId);

    Task<BaseOutput<bool>> SaveMemberAsync(AppMemberSchema member);

    Task<BaseOutput<bool>> DeleteMemberAsync(string appId, string id);

    Task<BaseOutput<List<AppRoleSchema>>> GetRolesAsync(string appId);

    Task<BaseOutput<bool>> SaveRoleAsync(AppRoleSchema role);

    Task<BaseOutput<bool>> DeleteRoleAsync(string appId, string key);
}
