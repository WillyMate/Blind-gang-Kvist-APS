using LinqToDB;

namespace Infrastructure;

public static class Seeder
{
    public static void Seed(UserDatabase db, string passwordHash)
    {
        db.CreateTable<UsersDB>(
            tableOptions: TableOptions.CreateIfNotExists);

        if (db.Users().Any())
            return;

        db.Insert(new UsersDB
        {
            UserId = Guid.NewGuid().ToString(),
            UserName = "admin",
            PasswordHash = passwordHash,
            Role = UsersDB.UserRole.Admin
        });
    }
}