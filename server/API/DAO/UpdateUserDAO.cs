using System.ComponentModel.DataAnnotations;
using Infrastructure;

namespace API;

public sealed class UpdateUserDAO
{
    [Required]
    [MinLength(3)]
    [MaxLength(64)]
    public string UserName { get; set; } = string.Empty;

    public UsersDB.UserRole Role { get; set; }
}