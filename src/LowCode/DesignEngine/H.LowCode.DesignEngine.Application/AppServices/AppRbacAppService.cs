using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Base;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class AppRbacAppService : ApplicationService, IAppRbacAppService
{
    private IAppMemberRepository _memberRepository => LazyServiceProvider.GetRequiredService<IAppMemberRepository>();
    private IAppRoleRepository _roleRepository => LazyServiceProvider.GetRequiredService<IAppRoleRepository>();

    public async Task<BaseOutput<List<AppMemberSchema>>> GetMembersAsync(string appId)
    {
        return new(await _memberRepository.GetListAsync(appId));
    }

    public async Task<BaseOutput<bool>> SaveMemberAsync(AppMemberSchema member)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentException.ThrowIfNullOrEmpty(member.AppId);

        await _memberRepository.SaveAsync(member);
        return new(true);
    }

    public async Task<BaseOutput<bool>> DeleteMemberAsync(string appId, string id)
    {
        await _memberRepository.DeleteAsync(appId, id);
        return new(true);
    }

    public async Task<BaseOutput<List<AppRoleSchema>>> GetRolesAsync(string appId)
    {
        var roles = await _roleRepository.GetListAsync(appId);

        //无角色时提供内置默认角色（不落盘，仅返回，保存后落盘）
        if (roles.Count == 0)
        {
            roles.Add(new AppRoleSchema { AppId = appId, Key = "admin", Name = "管理员", Description = "拥有应用全部权限", IsBuiltin = true, Permissions = ["app.manage", "page.view", "data.read", "data.write"] });
            roles.Add(new AppRoleSchema { AppId = appId, Key = "member", Name = "普通成员", Description = "可查看页面与读写数据", IsBuiltin = true, Permissions = ["page.view", "data.read", "data.write"] });
            roles.Add(new AppRoleSchema { AppId = appId, Key = "viewer", Name = "只读访客", Description = "仅可查看", IsBuiltin = true, Permissions = ["page.view", "data.read"] });
        }

        return new(roles);
    }

    public async Task<BaseOutput<bool>> SaveRoleAsync(AppRoleSchema role)
    {
        ArgumentNullException.ThrowIfNull(role);
        ArgumentException.ThrowIfNullOrEmpty(role.AppId);
        ArgumentException.ThrowIfNullOrEmpty(role.Key);

        await _roleRepository.SaveAsync(role);
        return new(true);
    }

    public async Task<BaseOutput<bool>> DeleteRoleAsync(string appId, string key)
    {
        var roles = await _roleRepository.GetListAsync(appId);
        var role = roles.FirstOrDefault(t => t.Key == key);
        if (role is { IsBuiltin: true })
            throw new BusinessException("内置角色不可删除");

        await _roleRepository.DeleteAsync(appId, key);
        return new(true);
    }
}
