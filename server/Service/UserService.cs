using Infrastructure;
using LinqToDB;
using LinqToDB.Data;

namespace Service;

using Infrastructure;
using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;


public sealed class UserService
{
    private readonly DataConnection _db;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(DataConnection db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserSummary> CreateUserAsync(
        string username,
        string password,
        UsersDB.UserRole role,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        if (password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters");

        if (await _db.GetTable<UsersDB>().AnyAsync(x => x.UserName == username, ct))
            throw new InvalidOperationException("Username already taken");

        var user = new UsersDB(
            Guid.NewGuid().ToString(),
            username,
            _passwordHasher.Hash(password),
            role);

        await _db.InsertAsync(user, token: ct);
        return ToSummary(user);
    }

    public async Task<UserSummary?> LoginAsync(
        string username,
        string password,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var user = await _db.GetTable<UsersDB>()
            .FirstOrDefaultAsync(x => x.UserName == username, ct);

        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
            return null;

        return ToSummary(user);
    }

    public async Task<UserSummary?> GetUserAsync(
        string userId,
        CancellationToken ct = default)
    {
        var user = await _db.GetTable<UsersDB>()
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        return user is null ? null : ToSummary(user);
    }

    public async Task<bool> UpdateUserAsync(
        string userId,
        string userName,
        UsersDB.UserRole role,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);

        var updated = await _db.GetTable<UsersDB>()
            .Where(x => x.UserId == userId)
            .Set(x => x.UserName, userName)
            .Set(x => x.Role, role)
            .UpdateAsync(ct);

        return updated > 0;
    }

    public async Task<bool> DeleteUserAsync(
        string userId,
        CancellationToken ct = default)
    {
        var deleted = await _db.GetTable<UsersDB>()
            .Where(x => x.UserId == userId)
            .DeleteAsync(ct);

        return deleted > 0;
    }

    public async Task<List<UserSummary>> GetAllUsersAsync(
        CancellationToken ct = default)
    {
        var users = await _db.GetTable<UsersDB>().ToListAsync(ct);
        return users.Select(ToSummary).ToList();
    }

    private static UserSummary ToSummary(UsersDB user)
    {
        return new UserSummary(user.UserId, user.UserName, user.Role);
    }
}

public sealed record UserSummary(
    string UserId,
    string UserName,
    UsersDB.UserRole Role);