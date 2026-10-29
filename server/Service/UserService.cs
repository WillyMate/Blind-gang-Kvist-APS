using Infrastructure;
using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;

namespace Service;

public class   UserService
{
     private readonly DataConnection _db;
    
     
     public UserService(DataConnection db)
     {
         _db = db;
       
     }
     //Creates User
     public async Task<UsersDB> CreateUserAsync(string username, string password, UsersDB.Role role, CancellationToken ct = default)
     {
         var user = new UsersDB(Guid.NewGuid().ToString(),username, password, role);
         await _db.InsertAsync(user, token: ct);
         return user;
     }
     
     //Deletes User
     public async Task<bool> DeleteUserAsync(string userId, CancellationToken ct = default)
     {
         var delete = await _db.GetTable<UsersDB>().Where(x => x.UserId == userId).DeleteAsync(ct);

         return delete > 0;
     }
     
     //Update User
     public async Task<bool> UpdateUserAsync(string UserId, UsersDB user, CancellationToken ct = default)
     {
         user.UserId = UserId;
         return await _db.UpdateAsync(user, token: ct) > 0;
     }
     
     //Read Users 
     public async Task<List<UsersDB>>GetAllUsersAsync(CancellationToken ct = default)
     {
         return await _db.GetTable<UsersDB>().ToListAsync(ct);
     }

}
