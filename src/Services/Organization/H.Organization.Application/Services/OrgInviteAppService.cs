using H.Organization.Application.Contracts;
using H.Organization.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.Organization.Application;

/// <summary>
/// 组织邀请服务实现
/// </summary>
public class OrgInviteAppService : ApplicationService, IOrgInviteAppService
{
    private readonly OrganizationDbContext _context;
    private readonly ISmsSender _smsSender;
    private readonly IConfiguration _configuration;

    public OrgInviteAppService(OrganizationDbContext context, ISmsSender smsSender, IConfiguration configuration)
    {
        _context = context;
        _smsSender = smsSender;
        _configuration = configuration;
    }

    /// <summary>
    /// 创建邀请
    /// </summary>
    public async Task<InviteDto> CreateInviteAsync(CreateInviteDto input)
    {
        var org = await _context.Organizations.FirstOrDefaultAsync(x => x.Id == input.OrganizationId);
        if (org == null)
            throw new UserFriendlyException("部门不存在");

        var expireDays = input.ExpireDays <= 0 ? 7 : input.ExpireDays;

        var entity = new OrgInviteEntity
        {
            Id = Guid.NewGuid(),
            Token = Guid.NewGuid().ToString("N"),
            OrganizationId = input.OrganizationId,
            MemberType = input.MemberType <= 0 ? 1 : input.MemberType,
            Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim(),
            ExpiresAt = DateTime.UtcNow.AddDays(expireDays),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = CurrentUser.Id
        };

        _context.OrgInvites.Add(entity);
        await _context.SaveChangesAsync();

        var inviteUrl = $"{GetBaseUrl()}/organization/join?token={entity.Token}";

        var smsSent = false;
        if (!string.IsNullOrWhiteSpace(entity.Phone))
        {
            var content = $"邀请你加入「{org.Name}」，点击链接加入：{inviteUrl}";
            await _smsSender.SendAsync(entity.Phone, content);
            smsSent = true;
        }

        return new InviteDto
        {
            Token = entity.Token,
            InviteUrl = inviteUrl,
            OrganizationId = org.Id,
            OrganizationName = org.Name,
            ExpiresAt = entity.ExpiresAt,
            SmsSent = smsSent
        };
    }

    /// <summary>
    /// 获取邀请信息
    /// </summary>
    public async Task<InviteInfoDto> GetInviteInfoAsync(string token)
    {
        var invite = await _context.OrgInvites
            .Include(x => x.Organization)
            .FirstOrDefaultAsync(x => x.Token == token);

        if (invite == null)
            return new InviteInfoDto { Valid = false, Message = "邀请链接无效" };

        if (invite.IsUsed)
            return new InviteInfoDto { Valid = false, Message = "邀请链接已被使用" };

        if (invite.ExpiresAt < DateTime.UtcNow)
            return new InviteInfoDto { Valid = false, Message = "邀请链接已过期" };

        return new InviteInfoDto
        {
            Valid = true,
            OrganizationName = invite.Organization?.Name ?? string.Empty,
            MemberType = invite.MemberType
        };
    }

    /// <summary>
    /// 接受邀请
    /// </summary>
    public async Task AcceptInviteAsync(string token)
    {
        if (CurrentUser.Id == null)
            throw new UserFriendlyException("请先登录后再加入组织");

        var invite = await _context.OrgInvites
            .FirstOrDefaultAsync(x => x.Token == token);

        if (invite == null)
            throw new UserFriendlyException("邀请链接无效");
        if (invite.IsUsed)
            throw new UserFriendlyException("邀请链接已被使用");
        if (invite.ExpiresAt < DateTime.UtcNow)
            throw new UserFriendlyException("邀请链接已过期");

        var userId = CurrentUser.Id.Value;

        // 若已是该部门成员，直接消费令牌
        var exists = await _context.Members.AnyAsync(x =>
            x.OrganizationId == invite.OrganizationId && x.UserId == userId);

        if (!exists)
        {
            var member = new MemberEntity
            {
                Id = Guid.NewGuid(),
                OrganizationId = invite.OrganizationId,
                UserId = userId,
                UserName = CurrentUser.UserName ?? string.Empty,
                MemberType = invite.MemberType,
                Sort = 0,
                IsMain = false,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            _context.Members.Add(member);
        }

        invite.IsUsed = true;
        invite.UsedByUserId = userId;
        invite.UsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private string GetBaseUrl()
    {
        var baseUrl = _configuration["App:SelfUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            baseUrl = _configuration["RemoteServices:Organization:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            baseUrl = "https://localhost:7065";
        return baseUrl.TrimEnd('/');
    }
}
