using System.ComponentModel.DataAnnotations;
using Infrastructure;

namespace API;

public sealed record CreateUserDAO(
    [property: Required, MinLength(3), MaxLength(64)] string UserName,
    [property: Required, MinLength(12)] string Password,
    UsersDB.UserRole Role = UsersDB.UserRole.User);

public sealed record UpdateUserDAO(
    string UserName,
    UsersDB.UserRole Role);

public sealed record LoginDAO(
    string UserName,
    string Password);