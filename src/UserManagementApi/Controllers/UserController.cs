using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Models.User;
using UserManagementApi.Services.UserService;

namespace UserManagementApi.Controllers;

[Authorize]
[Route("api/users")]
[ApiController]
public class UserController : ControllerBase {
    private readonly IUserService _userService;

    public UserController(IUserService userService) {
        _userService = userService;
    }

    [HttpGet("")]
    public async Task<ActionResult<List<GetUserResponse>>> GetAllUsers() {
        var users = await _userService.GetAllUsers();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetUserResponse>> GetUser(int id) {
        var user = await _userService.GetUserById(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<GetUserResponse>> CreateUser(AddUserRequest request) {
        try {
            var user = await _userService.AddUser(request);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        } catch (InvalidOperationException ex) {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<GetUserResponse>> UpdateUser(int id, UpdateUserRequest request) {
        try {
            var user = await _userService.UpdateUser(id, request);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] 
    public async Task<IActionResult> DeleteUser(int id) {
        var deleted = await _userService.DeleteUser(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}