using H.Account.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace H.Account.HttpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// 获取用户分页列表
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] UserQueryParams queryParams)
    {
        var result = await _userService.GetPagedUsersAsync(queryParams);
        return Ok(result);
    }

    /// <summary>
    /// 根据 ID 获取用户
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userService.GetUserDtoByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var user = await _userService.CreateUserAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var user = await _userService.UpdateUserAsync(id, dto, currentUserId);
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 更新用户状态
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            await _userService.UpdateUserStatusAsync(id, dto, currentUserId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 重置用户密码
    /// </summary>
    [HttpPost("{id:guid}/reset-password")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordDto dto)
    {
        try
        {
            await _userService.ResetPasswordAsync(id, dto);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 检查用户名是否存在
    /// </summary>
    [HttpGet("check-username")]
    public async Task<ActionResult<bool>> CheckUserNameExists([FromQuery] string userName, [FromQuery] Guid? excludeId)
    {
        var exists = await _userService.ExistsByUserNameAsync(userName, excludeId);
        return Ok(exists);
    }

    /// <summary>
    /// 检查邮箱是否存在
    /// </summary>
    [HttpGet("check-email")]
    public async Task<ActionResult<bool>> CheckEmailExists([FromQuery] string email, [FromQuery] Guid? excludeId)
    {
        var exists = await _userService.ExistsByEmailAsync(email, excludeId);
        return Ok(exists);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }
}
