using H.Enterprise.Application.Contracts.Dtos;
using H.Enterprise.Application.Contracts.Services;
using H.Enterprise.EntityFrameworkCore;
using H.Enterprise.EntityFrameworkCore.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Volo.Abp.Application.Services;

namespace H.Enterprise.Application.Services;

/// <summary>
/// 企业管理服务实现
/// </summary>
public class EnterpriseAppService : ApplicationService, IEnterpriseAppService
{
    private readonly EnterpriseDbContext _context;
    private readonly H.SystemPortal.Application.Contracts.IUserAppService _userAppService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EnterpriseAppService(
        EnterpriseDbContext context,
        IHttpContextAccessor httpContextAccessor,
        H.SystemPortal.Application.Contracts.IUserAppService userAppService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userAppService = userAppService;
    }

    public async Task<PagedResult<EnterpriseDto>> GetListAsync(EnterpriseQueryParams queryParams)
    {
        var query = _context.Enterprises
            .Include(e => e.EnterpriseUsers)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.Keyword))
        {
            query = query.Where(e => e.Name.Contains(queryParams.Keyword) ||
                                     (e.Code != null && e.Code.Contains(queryParams.Keyword)) ||
                                     (e.ContactName != null && e.ContactName.Contains(queryParams.Keyword)));
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Status))
        {
            if (Enum.TryParse<EnterpriseStatus>(queryParams.Status, true, out var status))
                query = query.Where(e => e.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.DatabaseMode))
        {
            if (Enum.TryParse<DatabaseMode>(queryParams.DatabaseMode, true, out var mode))
                query = query.Where(e => e.DatabaseMode == mode);
        }

        if (queryParams.IsActivated.HasValue)
        {
            query = query.Where(e => e.IsActivated == queryParams.IsActivated.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((queryParams.PageIndex - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return new PagedResult<EnterpriseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageIndex = queryParams.PageIndex,
            PageSize = queryParams.PageSize
        };
    }

    public async Task<EnterpriseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Enterprises
            .Include(e => e.EnterpriseUsers)
            .FirstOrDefaultAsync(e => e.Id == id);

        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<EnterpriseDto> CreateAsync(CreateEnterpriseDto input)
    {
        // 检查编码唯一性
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            var exists = await _context.Enterprises.AnyAsync(e => e.Code == input.Code);
            if (exists)
                throw new Exception("企业编码已存在");
        }

        var currentUserId = GetCurrentUserId();

        var entity = new EnterpriseEntity
        {
            Id = Guid.NewGuid(),
            Name = input.Name,
            Code = input.Code,
            Description = input.Description,
            ContactName = input.ContactName,
            ContactPhone = input.ContactPhone,
            ContactEmail = input.ContactEmail,
            Status = EnterpriseStatus.Pending,
            IsActivated = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        _context.Enterprises.Add(entity);
        await _context.SaveChangesAsync();

        // 创建者自动成为 Owner
        if (currentUserId.HasValue)
        {
            var userName = await GetUserNameAsync(currentUserId.Value);
            var userEntity = new EnterpriseUserEntity
            {
                Id = Guid.NewGuid(),
                EnterpriseId = entity.Id,
                UserId = currentUserId.Value,
                UserName = userName,
                Role = "Owner",
                IsDefault = true,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };
            _context.EnterpriseUsers.Add(userEntity);
        }

        await _context.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<EnterpriseDto> UpdateAsync(Guid id, UpdateEnterpriseDto input)
    {
        var entity = await _context.Enterprises
            .Include(e => e.EnterpriseUsers)
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new Exception("企业不存在");

        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.Logo = input.Logo;
        entity.ContactName = input.ContactName;
        entity.ContactPhone = input.ContactPhone;
        entity.ContactEmail = input.ContactEmail;
        entity.Remark = input.Remark;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = GetCurrentUserId();

        await _context.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Enterprises
            .Include(e => e.EnterpriseUsers)
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new Exception("企业不存在");

        if (entity.Status == EnterpriseStatus.Active)
            throw new Exception("已激活的企业不能删除");

        _context.Enterprises.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task ActivateAsync(Guid id, ActivateEnterpriseDto input)
    {
        var entity = await _context.Enterprises
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new Exception("企业不存在");

        if (entity.IsActivated)
            throw new Exception("企业已激活，不可重复激活");

        // 设置数据库模式（激活后不可更改）
        if (Enum.TryParse<DatabaseMode>(input.DatabaseMode, true, out var dbMode))
        {
            entity.DatabaseMode = dbMode;
        }
        else
        {
            throw new Exception("无效的数据库模式");
        }

        // 独立数据库模式需要连接字符串
        if (entity.DatabaseMode == DatabaseMode.Independent)
        {
            if (string.IsNullOrWhiteSpace(input.ConnectionString))
                throw new Exception("独立数据库模式必须提供连接字符串");

            entity.ConnectionString = input.ConnectionString;
            // TODO: 动态创建数据库并执行迁移
        }

        entity.Status = EnterpriseStatus.Active;
        entity.IsActivated = true;
        entity.ActivatedAt = DateTime.UtcNow;
        entity.ActivatedBy = GetCurrentUserId();
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task EnableAsync(Guid id)
    {
        var entity = await _context.Enterprises
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new Exception("企业不存在");

        if (!entity.IsActivated)
            throw new Exception("企业尚未激活");

        entity.Status = EnterpriseStatus.Active;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = GetCurrentUserId();

        await _context.SaveChangesAsync();
    }

    public async Task DisableAsync(Guid id)
    {
        var entity = await _context.Enterprises
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new Exception("企业不存在");

        entity.Status = EnterpriseStatus.Disabled;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = GetCurrentUserId();

        await _context.SaveChangesAsync();
    }

    public async Task<List<EnterpriseDto>> GetMyEnterprisesAsync(Guid userId)
    {
        var userEnterprises = await _context.EnterpriseUsers
            .Include(eu => eu.Enterprise)
            .Where(eu => eu.UserId == userId)
            .OrderByDescending(eu => eu.IsDefault)
            .ThenByDescending(eu => eu.JoinedAt)
            .ToListAsync();

        return userEnterprises
            .Where(eu => eu.Enterprise != null)
            .Select(eu => MapToDto(eu.Enterprise!))
            .ToList();
    }

    public async Task SelectEnterpriseAsync(Guid enterpriseId)
    {
        var currentUserId = GetCurrentUserId() ?? throw new Exception("未登录");

        // 验证用户是否属于该企业
        var userEnterprise = await _context.EnterpriseUsers
            .Include(eu => eu.Enterprise)
            .FirstOrDefaultAsync(eu => eu.EnterpriseId == enterpriseId && eu.UserId == currentUserId)
            ?? throw new Exception("您不属于该企业");

        var enterprise = userEnterprise.Enterprise!;
        if (!enterprise.IsActivated || enterprise.Status != EnterpriseStatus.Active)
            throw new Exception("该企业未激活或已禁用");

        // 重新签发 Cookie，追加 TenantId 和 EnterpriseName Claims
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new Exception("无法获取 HTTP 上下文");

        var existingClaims = httpContext.User.Claims.ToList();

        // 移除旧的 TenantId、EnterpriseName、EnterpriseId、EnterpriseRole Claims
        var newClaims = existingClaims
            .Where(c => c.Type != "TenantId" && c.Type != "EnterpriseName" && c.Type != "EnterpriseId" && c.Type != "EnterpriseRole")
            .ToList();

        // 追加企业相关 Claims
        newClaims.Add(new Claim("TenantId", enterprise.Id.ToString()));
        newClaims.Add(new Claim("EnterpriseId", enterprise.Id.ToString()));
        newClaims.Add(new Claim("EnterpriseName", enterprise.Name));

        // 追加企业角色 Claim
        newClaims.Add(new Claim("EnterpriseRole", userEnterprise.Role));

        var claimsIdentity = new ClaimsIdentity(newClaims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
        };

        // 先注销旧的认证，再重新签发
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }

    public async Task<EnterpriseDto?> GetCurrentEnterpriseAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return null;

        var enterpriseIdClaim = httpContext.User.FindFirst("EnterpriseId")?.Value;
        if (string.IsNullOrEmpty(enterpriseIdClaim) || !Guid.TryParse(enterpriseIdClaim, out var enterpriseId))
            return null;

        var entity = await _context.Enterprises
            .Include(e => e.EnterpriseUsers)
            .FirstOrDefaultAsync(e => e.Id == enterpriseId);

        return entity != null ? MapToDto(entity) : null;
    }

    #region 辅助方法

    private Guid? GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userIdClaim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }

    private async Task<string> GetUserNameAsync(Guid userId)
    {
        try
        {
            var user = await _userAppService.GetUserByIdAsync(userId);
            return user?.UserName ?? userId.ToString();
        }
        catch
        {
            return userId.ToString();
        }
    }

    private static EnterpriseDto MapToDto(EnterpriseEntity entity)
    {
        return new EnterpriseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            Description = entity.Description,
            Logo = entity.Logo,
            ContactName = entity.ContactName,
            ContactPhone = entity.ContactPhone,
            ContactEmail = entity.ContactEmail,
            Status = entity.Status.ToString(),
            DatabaseMode = entity.DatabaseMode.ToString(),
            IsActivated = entity.IsActivated,
            ActivatedAt = entity.ActivatedAt,
            CreatedAt = entity.CreatedAt,
            Remark = entity.Remark,
            UserCount = entity.EnterpriseUsers?.Count ?? 0
        };
    }

    #endregion
}
