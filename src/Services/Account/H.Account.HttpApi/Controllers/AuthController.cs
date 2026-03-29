using H.Account.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace H.Account.HttpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (result.Success)
        {
            return Ok(result);
        }
        return Unauthorized(result);
    }

    [HttpGet("validate")]
    public async Task<ActionResult<bool>> ValidateToken([FromQuery] string token)
    {
        var isValid = await _authService.ValidateTokenAsync(token);
        return Ok(isValid);
    }
}