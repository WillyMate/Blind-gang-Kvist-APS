using LinqToDB.Mapping;

namespace Infrastructure;

[Table( "Users")]
public class UsersDB
{
 [PrimaryKey]
public string UserId { get; set; }

 [Column("UserName"), NotNull] 
 public string UserName { get; set; }
 
 [Column("Password"), NotNull]
 public string Password { get; set; }
 
 [Column("Role")] 
 public Role role  { get; set; }
 public enum Role
 {
  User = 0,
  SuperUser = 1,
  Admin = 2,
 }
 
 public UsersDB(){}

 public UsersDB(string userId, string userName, string password, Role role)
 {
  UserId = userId;
  UserName = userName;
  UserName = userName;
  Password = password;
  this.role = role; 
 }
}
