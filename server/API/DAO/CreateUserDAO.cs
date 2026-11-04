using System.ComponentModel.DataAnnotations;
using Infrastructure;

namespace API;

public sealed class CreateUserDAO
{
    [Required]
    [MinLength(3)]
    [MaxLength(64)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MinLength(12)]
    public string Password { get; set; } = string.Empty;

    public UsersDB.UserRole Role { get; set; } =
        UsersDB.UserRole.User;
}