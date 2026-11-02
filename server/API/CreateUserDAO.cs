using Infrastructure;

namespace API;

public class CreateUserDAO
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public UsersDB.UserRole Role { get; set; }
}