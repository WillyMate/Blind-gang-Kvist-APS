using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly TokenService _tokenService;

    public UserController(UserService userService, TokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    // Admin
    private bool CanManage(UsersDB.UserRole target) =>
        User.IsInRole(nameof(UsersDB.UserRole.Admin)) ||
        (User.IsInRole(nameof(UsersDB.UserRole.SuperUser)) && target == UsersDB.UserRole.User);

    [AllowAnonymous]
    [HttpPost(nameof(Login))]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginDAO dto,
        CancellationToken ct)
    {
        var user = await _userService.LoginAsync(dto.UserName, dto.Password, ct);

        if (user is null)
            return Unauthorized("Invalid username or password");

        return new LoginResponse(_tokenService.Create(user), user);
    }

    // Any  (User, SuperUser, Admin)
    [HttpGet(nameof(GetUsers))]
    public async Task<List<UserSummary>> GetUsers(CancellationToken ct)
    {
        return await _userService.GetAllUsersAsync(ct);
    }

    [Authorize(Roles = "SuperUser,Admin")]
    [HttpPost(nameof(AddUser))]
    public async Task<ActionResult<UserSummary>> AddUser(
        [FromBody] CreateUserDAO newUser,
        CancellationToken ct)
    {
        if (!CanManage(newUser.Role))
            return Forbid();

        try
        {
            return await _userService.CreateUserAsync(
                newUser.UserName, newUser.Password, newUser.Role, ct);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "SuperUser,Admin")]
    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser(
        string userId,
        [FromBody] UpdateUserDAO dto,
        CancellationToken ct)
    {
        var existing = await _userService.GetUserAsync(userId, ct);
        if (existing is null)
            return NotFound();

        if (!CanManage(existing.Role) || !CanManage(dto.Role))
            return Forbid();

        var ok = await _userService.UpdateUserAsync(userId, dto.UserName, dto.Role, ct);
        return ok ? NoContent() : NotFound();
    }

    [Authorize(Roles = "SuperUser,Admin")]
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(string userId, CancellationToken ct)
    {
        if (userId == User.FindFirst("sub")?.Value)
            return BadRequest("You cannot delete yourself");

        var existing = await _userService.GetUserAsync(userId, ct);
        if (existing is null)
            return NotFound();

        if (!CanManage(existing.Role))
            return Forbid();

        var ok = await _userService.DeleteUserAsync(userId, ct);
        return ok ? NoContent() : NotFound();
    }
}