using Infrastructure;

namespace API;

public sealed record UserDAO(
    string UserId,
    string UserName,
    UsersDB.UserRole Role
);