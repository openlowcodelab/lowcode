using H.Account.Application.Contracts;
using H.Account.Domain;
using H.Account.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace H.Account.Application;

public class UserService : IUserService
{
    private readonly AccountDbContext _dbContext;

    public UserService(AccountDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserDto?> GetUserByUserNameAsync(string userName)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        if (user == null) return null;

        return MapToUserDto(user);
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;

        return MapToUserDto(user);
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) return null;

        return MapToUserDto(user);
    }

    public async Task<UserDto?> GetUserDtoByIdAsync(Guid userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) return null;

        return MapToUserDto(user);
    }

    public async Task<UserDto> CreateUserAsync(UserDto user)
    {
        var userEntity = MapToUserEntity(user);
        
        userEntity.PasswordHash = HashPassword(user.Password);
        userEntity.IsActive = true;

        _dbContext.Users.Add(userEntity);
        await _dbContext.SaveChangesAsync();
        return MapToUserDto(userEntity);
    }

    public async Task<bool> UpdateUserAsync(UserDto user)
    {
        var existingUser = await _dbContext.Users.FindAsync(user.Id);
        if (existingUser == null)
        {
            return false;
        }

        existingUser.UserName = user.UserName;
        existingUser.Email = user.Email;
        existingUser.PhoneNumber = user.PhoneNumber;
        existingUser.UserType = (int)user.UserType;
        existingUser.Roles = user.Roles;
        existingUser.IsActive = user.IsActive;
        existingUser.EmailConfirmed = user.EmailConfirmed;
        existingUser.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
        existingUser.LockoutEnd = user.LockoutEnd;
        existingUser.AccessFailedCount = user.AccessFailedCount;
        existingUser.UpdatedAt = DateTime.UtcNow;
        existingUser.UpdatedBy = user.UpdatedBy;
        existingUser.Remark = user.Remark;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<UserDto>> GetPagedUsersAsync(UserQueryParams queryParams)
    {
        var query = _dbContext.Users.AsQueryable();

        // 关键字搜索
        if (!string.IsNullOrWhiteSpace(queryParams.Keyword))
        {
            query = query.Where(u => u.UserName.Contains(queryParams.Keyword) ||
                                     u.Email.Contains(queryParams.Keyword) ||
                                     u.PhoneNumber.Contains(queryParams.Keyword));
        }

        // 用户类型筛选
        if (queryParams.UserType.HasValue)
        {
            int userType = (int)queryParams.UserType.Value;
            query = query.Where(u => u.UserType == userType);
        }

        // 激活状态筛选
        if (queryParams.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == queryParams.IsActive.Value);
        }

        var total = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((queryParams.PageIndex - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return new PagedResult<UserDto>
        {
            Items = users.Select(MapToUserDto).ToList(),
            Total = total,
            PageIndex = queryParams.PageIndex,
            PageSize = queryParams.PageSize
        };
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto, Guid? currentUserId = null)
    {
        // 验证密码确认
        if (dto.Password != dto.ConfirmPassword)
        {
            throw new ArgumentException("密码和确认密码不匹配");
        }

        // 检查用户名是否已存在
        if (await ExistsByUserNameAsync(dto.UserName))
        {
            throw new ArgumentException("用户名已存在");
        }

        // 检查邮箱是否已存在
        if (await ExistsByEmailAsync(dto.Email))
        {
            throw new ArgumentException("邮箱已存在");
        }

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserName = dto.UserName,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber ?? string.Empty,
            UserType = (int)dto.UserType,
            Roles = dto.Roles,
            IsActive = dto.IsActive,
            EmailConfirmed = dto.EmailConfirmed,
            PhoneNumberConfirmed = dto.PhoneNumberConfirmed,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId,
            Remark = dto.Remark
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return MapToUserDto(user);
    }

    public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserDto dto, Guid? currentUserId = null)
    {
        var user = await _dbContext.Users.FindAsync(userId)
            ?? throw new ArgumentException($"用户 {userId} 不存在");

        // 检查用户名是否已存在（排除自己）
        if (await ExistsByUserNameAsync(dto.UserName, userId))
        {
            throw new ArgumentException("用户名已存在");
        }

        // 检查邮箱是否已存在（排除自己）
        if (await ExistsByEmailAsync(dto.Email, userId))
        {
            throw new ArgumentException("邮箱已存在");
        }

        user.UserName = dto.UserName;
        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber ?? string.Empty;
        user.UserType = (int)dto.UserType;
        user.Roles = dto.Roles;
        user.IsActive = dto.IsActive;
        user.EmailConfirmed = dto.EmailConfirmed;
        user.PhoneNumberConfirmed = dto.PhoneNumberConfirmed;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = currentUserId;
        user.Remark = dto.Remark;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task UpdateUserStatusAsync(Guid userId, UpdateUserStatusDto dto, Guid? currentUserId = null)
    {
        var user = await _dbContext.Users.FindAsync(userId)
            ?? throw new ArgumentException($"用户 {userId} 不存在");

        user.IsActive = dto.IsActive;
        user.Remark = dto.Remark;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = currentUserId;

        await _dbContext.SaveChangesAsync();
    }

    public async Task ResetPasswordAsync(Guid userId, ResetPasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
        {
            throw new ArgumentException("新密码和确认密码不匹配");
        }

        var user = await _dbContext.Users.FindAsync(userId)
            ?? throw new ArgumentException($"用户 {userId} 不存在");

        user.PasswordHash = HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _dbContext.Users.FindAsync(userId)
            ?? throw new ArgumentException($"用户 {userId} 不存在");

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> ExistsByUserNameAsync(string userName, Guid? excludeId = null)
    {
        var query = _dbContext.Users.Where(u => u.UserName == userName);
        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }
        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null)
    {
        var query = _dbContext.Users.Where(u => u.Email == email);
        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }
        return await query.AnyAsync();
    }

    public async Task<bool> VerifyPasswordAsync(string userName, string password)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        if (user == null) return false;

        var hashedPassword = HashPassword(password);
        return user.PasswordHash == hashedPassword;
    }

    public async Task UpdateLastLoginTimeAsync(Guid userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static UserDto MapToUserDto(UserEntity user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            UserType = (UserType)user.UserType,
            Roles = user.Roles,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            LockoutEnd = user.LockoutEnd,
            AccessFailedCount = user.AccessFailedCount,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            CreatedBy = user.CreatedBy,
            UpdatedBy = user.UpdatedBy,
            Remark = user.Remark
        };
    }

    private static UserEntity MapToUserEntity(UserDto user)
    {
        return new UserEntity
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            UserType = (int)user.UserType,
            Roles = user.Roles,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            LockoutEnd = user.LockoutEnd,
            AccessFailedCount = user.AccessFailedCount,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            CreatedBy = user.CreatedBy,
            UpdatedBy = user.UpdatedBy,
            Remark = user.Remark
        };
    }
}