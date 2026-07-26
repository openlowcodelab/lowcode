using H.LowCode.MetaSchema.DesignEngine;

namespace H.LowCode.DesignEngine.Domain.Repositories;

public interface IAppMemberRepository
{
    Task<List<AppMemberSchema>> GetListAsync(string appId);

    Task SaveAsync(AppMemberSchema member);

    Task DeleteAsync(string appId, string id);
}

public interface IAppRoleRepository
{
    Task<List<AppRoleSchema>> GetListAsync(string appId);

    Task SaveAsync(AppRoleSchema role);

    Task DeleteAsync(string appId, string key);
}
