using H.Abstractions;
using H.LowCode.MetaSchema.DesignEngine;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IAppRbacAppService : IAppService
{
    Task<List<AppMemberSchema>> GetMembersAsync(string appId);

    Task<bool> SaveMemberAsync(AppMemberSchema member);

    Task<bool> DeleteMemberAsync(string appId, string id);

    Task<List<AppRoleSchema>> GetRolesAsync(string appId);

    Task<bool> SaveRoleAsync(AppRoleSchema role);

    Task<bool> DeleteRoleAsync(string appId, string key);
}
