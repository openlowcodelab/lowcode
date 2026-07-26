using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.Extensions.Options;
using System.Text;

namespace H.LowCode.DesignEngine.Repository.JsonFile;

public class AppMemberFileRepository : FileRepositoryBase, IAppMemberRepository
{
    private static readonly string memberFileName_Format = @"{0}\{1}\member\{2}.json";

    public AppMemberFileRepository(IOptions<MetaOption> metaOption) : base(metaOption)
    {
    }

    public async Task<List<AppMemberSchema>> GetListAsync(string appId)
    {
        List<AppMemberSchema> list = [];

        var folder = Path.Combine(_metaBaseDir, appId, "member");
        if (!Directory.Exists(folder))
            return list;

        foreach (var fileName in Directory.GetFiles(folder, "*.json"))
        {
            var json = ReadAllText(fileName);
            list.Add(json.FromJson<AppMemberSchema>());
        }

        return await Task.FromResult(list.OrderBy(t => t.JoinTime).ToList());
    }

    public Task SaveAsync(AppMemberSchema member)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentException.ThrowIfNullOrEmpty(member.Id);
        ArgumentException.ThrowIfNullOrEmpty(member.AppId);

        string fileName = string.Format(memberFileName_Format, _metaBaseDir, member.AppId, member.Id);

        string dir = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(fileName, member.ToJson(), Encoding.UTF8);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string appId, string id)
    {
        string fileName = string.Format(memberFileName_Format, _metaBaseDir, appId, id);
        if (File.Exists(fileName))
            File.Delete(fileName);
        return Task.CompletedTask;
    }
}

public class AppRoleFileRepository : FileRepositoryBase, IAppRoleRepository
{
    private static readonly string roleFileName_Format = @"{0}\{1}\role\{2}.json";

    public AppRoleFileRepository(IOptions<MetaOption> metaOption) : base(metaOption)
    {
    }

    public async Task<List<AppRoleSchema>> GetListAsync(string appId)
    {
        List<AppRoleSchema> list = [];

        var folder = Path.Combine(_metaBaseDir, appId, "role");
        if (!Directory.Exists(folder))
            return list;

        foreach (var fileName in Directory.GetFiles(folder, "*.json"))
        {
            var json = ReadAllText(fileName);
            list.Add(json.FromJson<AppRoleSchema>());
        }

        return await Task.FromResult(list);
    }

    public Task SaveAsync(AppRoleSchema role)
    {
        ArgumentNullException.ThrowIfNull(role);
        ArgumentException.ThrowIfNullOrEmpty(role.Key);
        ArgumentException.ThrowIfNullOrEmpty(role.AppId);

        string fileName = string.Format(roleFileName_Format, _metaBaseDir, role.AppId, role.Key);

        string dir = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(fileName, role.ToJson(), Encoding.UTF8);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string appId, string key)
    {
        string fileName = string.Format(roleFileName_Format, _metaBaseDir, appId, key);
        if (File.Exists(fileName))
            File.Delete(fileName);
        return Task.CompletedTask;
    }
}
