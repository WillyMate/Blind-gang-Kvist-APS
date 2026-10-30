using LinqToDB;
using LinqToDB.Data;

namespace Infrastructure;

public class UserDatabase(DataOptions<UserDatabase> dataopts) : DataConnection(dataopts.Options)
{
    public ITable<UsersDB> Users()
    {
        return this.GetTable<UsersDB>();
    }
}

    
