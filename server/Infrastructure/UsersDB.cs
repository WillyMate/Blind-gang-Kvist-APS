using LinqToDB.Mapping;

namespace Infrastructure;

[Table("Users")]
public sealed class UsersDB
{
    [PrimaryKey]
    public string UserId { get; set; } = null!;

    [Column("UserName"), NotNull]
    public string UserName { get; set; } = null!;

    [Column("PasswordHash"), NotNull]
    public string PasswordHash { get; set; } = null!;

    [Column("Role"), NotNull]
    public UserRole Role { get; set; }

    public enum UserRole
    {
        User = 0,
        SuperUser = 1,
        Admin = 2
    }

    public UsersDB()
    {
    }

    public UsersDB(string userId, string userName, string passwordHash, UserRole role)
    {
        UserId = userId;
        UserName = userName;
        PasswordHash = passwordHash;
        Role = role;
    }
}
