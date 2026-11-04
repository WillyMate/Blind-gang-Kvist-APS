using Infrastructure;

namespace API;

public sealed record CreateUserDAO(
    string UserName,
    string Password,
    UsersDB.UserRole Role = UsersDB.UserRole.User);

public sealed record UpdateUserDAO(
    string UserName,
    UsersDB.UserRole Role);

public sealed record LoginDAO(
    string UserName,
    string Password);