using System.ComponentModel.DataAnnotations;

namespace API;

public sealed class SignInDAO
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}