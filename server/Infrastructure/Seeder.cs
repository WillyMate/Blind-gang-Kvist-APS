using LinqToDB;

namespace Infrastructure;

public static class Seeder
{
    public static void seed(UserDatabase db)
    {
        db.CreateTable<UsersDB>(tableOptions: TableOptions.CreateIfNotExists);
        if(db.Users().Any()) return;
    }
}