using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost(nameof(AddUser))]
    public async Task<UserSummary> AddUser(
        [FromBody] CreateUserDAO newUser,
        CancellationToken ct)
    {
        return await _userService.CreateUserAsync(
            newUser.UserName,
            newUser.Password,
            newUser.Role,
            ct);
    }

    [HttpPost(nameof(Login))]
    public async Task<ActionResult<UserSummary>> Login(
        [FromBody] LoginDAO dto,
        CancellationToken ct)
    {
        var user = await _userService.LoginAsync(dto.UserName, dto.Password, ct);

        if (user is null)
            return Unauthorized("Invalid username or password");

        return user;
    }

    [HttpGet(nameof(GetUsers))]
    public async Task<List<UserSummary>> GetUsers(CancellationToken ct)
    {
        return await _userService.GetAllUsersAsync(ct);
    }

    [HttpPut("{userId}")]
    public async Task<bool> UpdateUser(
        string userId,
        [FromBody] UpdateUserDAO dto,
        CancellationToken ct)
    {
        return await _userService.UpdateUserAsync(userId, dto.UserName, dto.Role, ct);
    }

    [HttpDelete("{userId}")]
    public async Task<bool> DeleteUser(string userId, CancellationToken ct)
    {
        return await _userService.DeleteUserAsync(userId, ct);
    }
}