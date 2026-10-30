using Infrastructure;
using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;

namespace Service;

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

        var user = new UsersDB(
            Guid.NewGuid().ToString(),
            username,
            _passwordHasher.Hash(password),
            role);

        await _db.InsertAsync(user, token: ct);
        return ToSummary(user);
    }

    public async Task<bool> VerifyPasswordAsync(
        string username,
        string password,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        var user = await _db.GetTable<UsersDB>()
            .FirstOrDefaultAsync(x => x.UserName == username, ct);

        return user is not null && _passwordHasher.Verify(password, user.PasswordHash);
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

    public async Task<bool> UpdateUserAsync(
        string userId,
        UsersDB user,
        CancellationToken ct = default)
    {
        user.UserId = userId;
        return await _db.UpdateAsync(user, token: ct) > 0;
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
