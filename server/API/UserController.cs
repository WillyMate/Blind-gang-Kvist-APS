
using System.Security.Claims;
using API;
using Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    // LOGIN
    [AllowAnonymous]
    [HttpPost("signin")]
    public async Task<IActionResult> SignIn(
        [FromBody] SignInDAO login,
        CancellationToken ct)
    {
        var user = await _userService.AuthenticateAsync(
            login.UserName,
            login.Password,
            ct
        );

        if (user is null)
            return Unauthorized(new { message = "Invalid username or password" });

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal
        );

        return Ok(new UserDAO(
            user.UserId,
            user.UserName,
            user.Role
        ));
    }

    // LOGOUT
    [AllowAnonymous]
    [HttpPost("signout")]
    public async Task<IActionResult> SignOutUser()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        return NoContent();
    }

    // CURRENT USER
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        return Ok(new
        {
            UserName = User.Identity?.Name,
            Role = User.FindFirstValue(ClaimTypes.Role)
        });
    }

    // GET ALL USERS
    [Authorize(Roles = "Admin,SuperUser")]
    [HttpGet]
    public async Task<ActionResult<List<UserDAO>>> GetUsers(
        CancellationToken ct)
    {
        var users = await _userService.GetAllUsersAsync(ct);

        var result = users.Select(user =>
            new UserDAO(
                user.UserId,
                user.UserName,
                user.Role
            )
        ).ToList();

        return Ok(result);
    }

    // CREATE USER
    [Authorize(Roles = "Admin,SuperUser")]
    [HttpPost]
    public async Task<ActionResult<UserDAO>> AddUser(
        [FromBody] CreateUserDAO newUser,
        CancellationToken ct)
    {
        // SuperUsers are only allowed to create normal users.
        var role = User.IsInRole("Admin")
            ? newUser.Role
            : UsersDB.UserRole.User;

        var user = await _userService.CreateUserAsync(
            newUser.UserName,
            newUser.Password,
            role,
            ct
        );

        var result = new UserDAO(
            user.UserId,
            user.UserName,
            user.Role
        );

        return Ok(result);
    }

    // UPDATE USER
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(
        string id,
        [FromBody] UpdateUserDAO updatedUser,
        CancellationToken ct)
    {
        var updated = await _userService.UpdateUserAsync(
            id,
            updatedUser.UserName,
            updatedUser.Role,
            ct
        );

        if (!updated)
            return NotFound();

        return NoContent();
    }

    // DELETE USER
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(
        string id,
        CancellationToken ct)
    {
        var deleted = await _userService.DeleteUserAsync(id, ct);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
