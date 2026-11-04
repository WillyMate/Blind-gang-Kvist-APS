<<<<<<< HEAD
﻿
using System.Security.Claims;
using API;
using Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
=======
﻿using Infrastructure;
>>>>>>> 0a9f370 (AAA)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API;

[ApiController]
<<<<<<< HEAD
[Route("api/users")]
=======
[Route("api/[controller]")]
[Authorize]
>>>>>>> 0a9f370 (AAA)
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly TokenService _tokenService;

    public UserController(UserService userService, TokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }
<<<<<<< HEAD

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
=======

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
>>>>>>> 0a9f370 (AAA)
