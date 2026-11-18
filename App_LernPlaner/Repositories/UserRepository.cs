using App_LernPlaner.Models;
using LernPlaner.Core;
using App_LernPlaner.Remote;

namespace App_LernPlaner.Repositories;
public class UserRepository
{
    public async Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<User?>("User.FindByUsernameAsync", new object? [] { username }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.UserRepository(db).FindByUsernameAsync(username, cancellationToken);
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<User?>("User.FindByEmailAsync", new object? [] { email }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.UserRepository(db).FindByEmailAsync(email, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<User?>("User.GetByIdAsync", new object? [] { userId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.UserRepository(db).GetByIdAsync(userId, cancellationToken);
    }

    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<User>>("User.GetAllAsync", new object? [] { }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.UserRepository(db).GetAllAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string username, string email, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<bool>("User.ExistsAsync", new object? [] { username, email }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.UserRepository(db).ExistsAsync(username, email, cancellationToken);
    }

    public async Task<int> InsertAsync(string username, string passwordHash, string email, string role, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<int>("User.InsertAsync", new object? [] { username, passwordHash, email, role }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.UserRepository(db).InsertAsync(username, passwordHash, email, role, cancellationToken);
    }

    public async Task UpdatePasswordAsync(int userId, string newHash, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("User.UpdatePasswordAsync", new object? [] { userId, newHash }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.UserRepository(db).UpdatePasswordAsync(userId, newHash, cancellationToken);
    }

    public async Task UpdateRoleAsync(int userId, string role, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("User.UpdateRoleAsync", new object? [] { userId, role }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.UserRepository(db).UpdateRoleAsync(userId, role, cancellationToken);
    }

    public async Task UpdateActiveAsync(int userId, bool isActive, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("User.UpdateActiveAsync", new object? [] { userId, isActive }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.UserRepository(db).UpdateActiveAsync(userId, isActive, cancellationToken);
    }

    public async Task UpdateProfileAsync(int userId, string firstName, string lastName, string? profilePicturePath = null, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("User.UpdateProfileAsync", new object? [] { userId, firstName, lastName, profilePicturePath }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.UserRepository(db).UpdateProfileAsync(userId, firstName, lastName, profilePicturePath, cancellationToken);
    }

    public async Task AddXpAsync(int userId, int amount, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("User.AddXpAsync", new object? [] { userId, amount }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.UserRepository(db).AddXpAsync(userId, amount, cancellationToken);
    }

    public async Task UpdateUserProfileAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("User.UpdateUserProfileAsync", new object? [] { profile }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.UserRepository(db).UpdateUserProfileAsync(profile, cancellationToken);
    }

    public async Task<UserProfile?> GetUserProfileByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<UserProfile?>("User.GetUserProfileByIdAsync", new object? [] { userId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.UserRepository(db).GetUserProfileByIdAsync(userId, cancellationToken);
    }
}
