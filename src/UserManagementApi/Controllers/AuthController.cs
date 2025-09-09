using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Models.User;
using UserManagementApi.Services.UserService;

namespace UserManagementApi.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase {
    private readonly IUserService _userService;

    public AuthController(IUserService userService) {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(AddUserRequest request) {
        try
        {
            var token = await _userService.Register(request);
            return Ok(new { token });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginRequest request) {
        var token = await _userService.Login(request);
        if (token == null)
            return Unauthorized(new { message = "Invalid email or password" });

        return Ok(new { token });
    }
}