using H.SystemPortal.Application.Contracts;
using H.Util.Base;
using Volo.Abp.Application.Services;

namespace H.SystemPortal.Application;

public class UserAppService : ApplicationService, IUserAppService
{
    private readonly SystemUserStore _store;

    public UserAppService(SystemUserStore store)
    {
        _store = store;
    }

    public Task<BaseOutput<UserDto?>> GetUserByUserNameAsync(string userName)
    {
        var user = _store.FindByUserName(userName);
        return Task.FromResult(BaseOutput<UserDto?>.Ok(user != null ? MapToDto(user) : null));
    }

    public Task<BaseOutput<UserDto?>> GetUserByEmailAsync(string email)
    {
        var user = _store.FindByEmail(email);
        return Task.FromResult(BaseOutput<UserDto?>.Ok(user != null ? MapToDto(user) : null));
    }

    public Task<BaseOutput<UserDto?>> GetUserByIdAsync(Guid userId)
    {
        var user = _store.FindById(userId);
        return Task.FromResult(BaseOutput<UserDto?>.Ok(user != null ? MapToDto(user) : null));
    }

    public Task<BaseOutput<UserDto?>> GetUserDtoByIdAsync(Guid userId)
    {
        return GetUserByIdAsync(userId);
    }

    public Task<BaseOutput<UserDto>> CreateUserAsync(UserDto user)
    {
        var entity = new SystemUserEntity
        {
            Id = Guid.NewGuid(),
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            PasswordHash = SystemUserStore.HashPassword(user.Password),
            UserType = user.UserType,
            RoleNames = user.RoleNames,
            IsActive = user.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _store.Add(entity);
        return Task.FromResult(BaseOutput<UserDto>.Ok(MapToDto(entity)));
    }

    public Task<BaseOutput<UserDto>> CreateUserAsync(CreateUserDto dto, Guid? currentUserId = null)
    {
        if (_store.FindByUserName(dto.UserName) != null)
            throw new Exception("用户名已存在");

        if (!string.IsNullOrEmpty(dto.Email) && _store.FindByEmail(dto.Email) != null)
            throw new Exception("邮箱已存在");

        if (dto.Password != dto.ConfirmPassword)
            throw new Exception("两次密码输入不一致");

        var entity = new SystemUserEntity
        {
            Id = Guid.NewGuid(),
            UserName = dto.UserName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber ?? "",
            PasswordHash = SystemUserStore.HashPassword(dto.Password),
            UserType = dto.UserType,
            RoleNames = dto.RoleNames ?? new(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            Remark = dto.Remark
        };

        _store.Add(entity);
        return Task.FromResult(BaseOutput<UserDto>.Ok(MapToDto(entity)));
    }

    public Task<BaseOutput<bool>> UpdateUserAsync(UserDto user)
    {
        var existing = _store.FindById(user.Id);
        if (existing == null) return Task.FromResult(BaseOutput<bool>.Ok(false));

        existing.UserName = user.UserName;
        existing.Email = user.Email;
        existing.PhoneNumber = user.PhoneNumber;
        _store.Update(existing);
        return Task.FromResult(BaseOutput<bool>.Ok(true));
    }

    public Task<BaseOutput<bool>> UpdateUserAsync(Guid userId, UpdateUserDto dto, Guid? currentUserId = null)
    {
        var existing = _store.FindById(userId);
        if (existing == null) return Task.FromResult(BaseOutput<bool>.Ok(false));

        existing.UserName = dto.UserName;
        existing.Email = dto.Email;
        existing.PhoneNumber = dto.PhoneNumber ?? "";
        existing.UserType = dto.UserType;
        existing.RoleNames = dto.RoleNames ?? existing.RoleNames;
        existing.IsActive = dto.IsActive;
        existing.Remark = dto.Remark;
        _store.Update(existing);
        return Task.FromResult(BaseOutput<bool>.Ok(true));
    }

    public Task<BaseOutput> UpdateUserStatusAsync(Guid userId, UpdateUserStatusDto dto, Guid? currentUserId = null)
    {
        var existing = _store.FindById(userId);
        if (existing == null) throw new Exception("用户不存在");

        existing.IsActive = dto.IsActive;
        _store.Update(existing);
        return Task.FromResult(BaseOutput.Ok());
    }

    public Task<BaseOutput> ResetPasswordAsync(Guid userId, ResetPasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
            throw new Exception("两次密码输入不一致");

        var existing = _store.FindById(userId);
        if (existing == null) throw new Exception("用户不存在");

        existing.PasswordHash = SystemUserStore.HashPassword(dto.NewPassword);
        _store.Update(existing);
        return Task.FromResult(BaseOutput.Ok());
    }

    public Task<BaseOutput> DeleteUserAsync(Guid userId)
    {
        var existing = _store.FindById(userId);
        if (existing == null) throw new Exception("用户不存在");

        _store.Delete(userId);
        return Task.FromResult(BaseOutput.Ok());
    }

    public Task<BaseOutput<bool>> ExistsByUserNameAsync(string userName, Guid? excludeId = null)
    {
        var user = _store.FindByUserName(userName);
        var exists = user != null && user.Id != excludeId;
        return Task.FromResult(BaseOutput<bool>.Ok(exists));
    }

    public Task<BaseOutput<bool>> ExistsByEmailAsync(string email, Guid? excludeId = null)
    {
        var user = _store.FindByEmail(email);
        var exists = user != null && user.Id != excludeId;
        return Task.FromResult(BaseOutput<bool>.Ok(exists));
    }

    public Task<BaseOutput<bool>> VerifyPasswordAsync(string userName, string password)
    {
        var user = _store.FindByUserName(userName);
        if (user == null) return Task.FromResult(BaseOutput<bool>.Ok(false));
        return Task.FromResult(BaseOutput<bool>.Ok(_store.VerifyPassword(user, password)));
    }

    public Task<BaseOutput> UpdateLastLoginTimeAsync(Guid userId)
    {
        var user = _store.FindById(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            _store.Update(user);
        }
        return Task.FromResult(BaseOutput.Ok());
    }

    public Task<BaseOutput<PagedResult<UserDto>>> GetPagedUsersAsync(UserQueryParams queryParams)
    {
        var allUsers = _store.GetAll().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(queryParams.Keyword))
        {
            allUsers = allUsers.Where(u =>
                u.UserName.Contains(queryParams.Keyword, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(queryParams.Keyword, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(u.PhoneNumber) && u.PhoneNumber.Contains(queryParams.Keyword)));
        }

        if (queryParams.UserType.HasValue)
        {
            allUsers = allUsers.Where(u => u.UserType == queryParams.UserType.Value);
        }

        if (queryParams.IsActive.HasValue)
        {
            allUsers = allUsers.Where(u => u.IsActive == queryParams.IsActive.Value);
        }

        var list = allUsers.ToList();
        var total = list.Count;
        var items = list
            .Skip((queryParams.PageIndex - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(MapToDto)
            .ToList();

        return Task.FromResult(BaseOutput<PagedResult<UserDto>>.Ok(new PagedResult<UserDto>
        {
            Items = items,
            Total = total,
            PageIndex = queryParams.PageIndex,
            PageSize = queryParams.PageSize
        }));
    }

    public Task<BaseOutput> AssignRolesToUserAsync(Guid userId, List<string> roleNames)
    {
        var user = _store.FindById(userId);
        if (user == null) throw new Exception("用户不存在");

        user.RoleNames = roleNames;
        user.UserType = DeriveUserType(roleNames);
        _store.Update(user);
        return Task.FromResult(BaseOutput.Ok());
    }

    public Task<BaseOutput<List<string>>> GetUserRoleNamesAsync(Guid userId)
    {
        var user = _store.FindById(userId);
        return Task.FromResult(BaseOutput<List<string>>.Ok(user?.RoleNames ?? new List<string>()));
    }

    private static UserDto MapToDto(SystemUserEntity entity)
    {
        return new UserDto
        {
            Id = entity.Id,
            UserName = entity.UserName,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber,
            UserType = entity.UserType,
            RoleNames = entity.RoleNames,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            LastLoginAt = entity.LastLoginAt,
            Remark = entity.Remark
        };
    }

    private static UserType DeriveUserType(List<string> roles)
    {
        if (roles.Contains(SystemRoleNames.SuperAdmin, StringComparer.OrdinalIgnoreCase))
            return UserType.SuperAdmin;
        if (roles.Contains(SystemRoleNames.Admin, StringComparer.OrdinalIgnoreCase))
            return UserType.Admin;
        return UserType.Normal;
    }
}
