using App_LernPlaner.Models;
using Microsoft.EntityFrameworkCore;

namespace LernPlaner.Core.Repositories;
public sealed class UserRepository(LernPlanerDbContext db)
{
    public async Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserName == username, cancellationToken);
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.Users.AsNoTracking().OrderBy(x => x.UserName).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string username, string email, CancellationToken cancellationToken = default)
    {
        return await db.Users.AnyAsync(x => x.UserName == username || x.Email == email, cancellationToken);
    }

    public async Task<int> InsertAsync(string username, string passwordHash, string email, string role, CancellationToken cancellationToken = default)
    {
        Input.Text(username, 100);
        Input.Email(email);
        Input.Role(role);
        Input.Hash(passwordHash);
        var user = new User
        {
            UserName = username.Trim(),
            PasswordHash = passwordHash,
            Email = email.Trim(),
            Role = role
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return user.UserId;
    }

    public async Task UpdatePasswordAsync(int userId, string newHash, CancellationToken cancellationToken = default)
    {
        Input.Hash(newHash);
        await db.Users.Where(x => x.UserId == userId).ExecuteUpdateAsync(s => s.SetProperty(x => x.PasswordHash, newHash), cancellationToken);
    }

    public async Task UpdateRoleAsync(int userId, string role, CancellationToken cancellationToken = default)
    {
        Input.Role(role);
        await db.Users.Where(x => x.UserId == userId).ExecuteUpdateAsync(s => s.SetProperty(x => x.Role, role), cancellationToken);
    }

    public async Task UpdateActiveAsync(int userId, bool isActive, CancellationToken cancellationToken = default)
    {
        await db.Users.Where(x => x.UserId == userId).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, isActive), cancellationToken);
    }

    public async Task UpdateProfileAsync(int userId, string firstName, string lastName, string? profilePicturePath = null, CancellationToken cancellationToken = default)
    {
        Input.Text(firstName, 100, true);
        Input.Text(lastName, 100, true);
        if (profilePicturePath != null)
            Input.Text(profilePicturePath, 1024, true);
        var u = await db.Users.SingleAsync(x => x.UserId == userId, cancellationToken);
        u.FirstName = firstName;
        u.LastName = lastName;
        if (profilePicturePath != null)
            u.ProfilePicture = profilePicturePath;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddXpAsync(int userId, int amount, CancellationToken cancellationToken = default)
    {
        if (amount < 0 || amount > 10000)
            throw new ArgumentException("Invalid XP.");
        await db.Users.Where(x => x.UserId == userId).ExecuteUpdateAsync(s => s.SetProperty(x => x.XPPoints, x => x.XPPoints + amount), cancellationToken);
    }

    public async Task UpdateUserProfileAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        Input.Email(profile.Email);
        Input.Text(profile.FirstName, 100, true);
        Input.Text(profile.LastName, 100, true);
        var u = await db.Users.SingleAsync(x => x.UserId == profile.UserId, cancellationToken);
        u.FirstName = profile.FirstName;
        u.LastName = profile.LastName;
        u.Email = profile.Email;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserProfile?> GetUserProfileByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var u = await GetByIdAsync(userId, cancellationToken);
        return u == null ? null : new UserProfile
        {
            UserId = u.UserId,
            Username = u.UserName,
            Email = u.Email,
            Role = u.Role,
            ProfilePicturePath = u.ProfilePicture,
            XPPoints = u.XPPoints,
            CreatedAt = u.CreatedAt,
            FirstName = u.FirstName ?? "",
            LastName = u.LastName ?? "",
            ClassName = u.ClassName ?? ""
        };
    }
}
